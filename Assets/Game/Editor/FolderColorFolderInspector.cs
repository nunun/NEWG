using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ItemInfo = GameEditor.FolderColorContext.ItemInfo;

namespace GameEditor {
    #if UNITY_EDITOR
    // フォルダインスペクタ
    [CustomEditor(typeof(DefaultAsset))]
    public sealed class FolderColorFolderInspector : UnityEditor.Editor {
        //------------------------------------------------------------------------- 定義
        // アセット情報の定義
        public class AssetInfo {
            public string guid;
            public string path;

            // コンストラクタ
            public AssetInfo(UnityEngine.Object target) {
                this.path = AssetDatabase.GetAssetPath(target);
                this.guid = AssetDatabase.AssetPathToGUID(this.path);
            }
        }

        private static AssetInfo assetInfo = null;  // アセット情報
        private static ItemInfo  editInfo  = null;  // 編集情報
        private static ItemInfo  copyInfo  = null;  // 保存情報
        private static bool      isFoldout = true;  // Foldout
        private static bool      isDirty   = false; // 変更があったかどうか

        //------------------------------------------------------------------------- 実装 (MonoBehaviour)
        // インスペクタ
        public override void OnInspectorGUI() {
            // フォルダでないなら描画しない
            var path = AssetDatabase.GetAssetPath(target);
            if (!AssetDatabase.IsValidFolder(path)) {
                return;
            }

            // コンテキスト
            var ctx = FolderColorContext.Instance;
            if (ctx == null) {
                return;
            }

            // アセット情報
            if (assetInfo == null || assetInfo.path != path) {
                assetInfo = new AssetInfo(target);
                editInfo  = null;
                isDirty   = false;
            }

            // アイテム情報
            if (editInfo == null) {
                editInfo = ctx.GetItemInfo(assetInfo.guid);
                if (editInfo == null) {
                    editInfo = ctx.ObtainItemInfo(assetInfo.guid);
                }
            }

            // インスペクタ GUI を描画
            GUI.enabled = true;
            {
                GUILayoutOption fitWidht = GUILayout.ExpandWidth(false);

                isFoldout = EditorGUILayout.Foldout(isFoldout, "Folder Color");
                if (isFoldout) {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(3);

                        // アイコン色
                        //EditorGUIStateUtility.BeginState();
                        {
                            // アイコン色を設定する
                            GUILayout.BeginHorizontal();
                            {
                                editInfo.isUseFolderColor = GUILayout.Toggle(editInfo.isUseFolderColor, "", fitWidht);
                                isDirty |= GUI.changed;

                                GUILayout.Space(-5);
                                GUILayout.Label("Use Folder Color", "WordWrappedLabel", fitWidht);
                            }
                            GUILayout.EndHorizontal();
                            GUI.enabled = editInfo.isUseFolderColor;

                            EditorGUILayout.BeginVertical("box");
                            {
                                // アイコンカラーピッカー
                                EditorGUI.BeginChangeCheck();
                                editInfo.folderColor = EditorGUILayout.ColorField("Folder Color", editInfo.folderColor);
                                isDirty |= EditorGUI.EndChangeCheck();

                                GUILayout.Space(3);

                                // サブフォルダの色も変更する
                                GUILayout.BeginHorizontal();
                                {
                                    editInfo.isChangeSubFolderColor = GUILayout.Toggle(editInfo.isChangeSubFolderColor, "", fitWidht);
                                    isDirty |= GUI.changed;

                                    GUILayout.Space(-5);
                                    GUILayout.Label("Change SubFolder Color", "WordWrappedLabel", fitWidht);
                                }
                                GUILayout.EndHorizontal();
                                GUILayout.Space(3);
                            }
                            EditorGUILayout.EndVertical();
                        }
                        //EditorGUIStateUtility.EndState();
                        GUILayout.Space(10);

                        // 適用ボタン
                        //GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal();
                        {
                            var lastColor = GUI.color;
                            GUI.backgroundColor = Color.red;
                            if (GUILayout.Button("Clear", "MiniButton", fitWidht)) {
                                editInfo.Set(new ItemInfo());
                                isDirty = true;
                            }
                            GUI.backgroundColor = lastColor;

                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("Copy", "MiniButton", fitWidht)) {
                                copyInfo = new ItemInfo(editInfo);
                            }

                            GUI.enabled = (copyInfo != null);
                            {
                                if (GUILayout.Button("Paste", "MiniButton", fitWidht)) {
                                    editInfo.Set(copyInfo);
                                    isDirty = true;
                                }
                            }
                            GUI.enabled = true;
                            //GUILayout.Label("/");
                            GUILayout.Space(5);

                            GUI.enabled = isDirty;
                            {
                                if (GUILayout.Button("Apply", "MiniButton", fitWidht)) {
                                    ctx.SetItemInfo(assetInfo.guid, editInfo);
                                    ctx.Save();
                                    FolderColorProjectWindowItemDrawer.ClearCache();
                                    isDirty = false;
                                }
                            }
                            GUI.enabled = true;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
            }
            GUI.enabled = false;
        }
    }
    #endif
}
