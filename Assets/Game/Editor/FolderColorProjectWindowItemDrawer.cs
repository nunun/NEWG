using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ItemInfo = GameEditor.FolderColorContext.ItemInfo;

namespace GameEditor {
    #if UNITY_EDITOR
    [InitializeOnLoad]
    public partial class FolderColorProjectWindowItemDrawer {
        //--------------------------------------------------------------------- インスタンス
        // スタティックコンストラクタ
        static FolderColorProjectWindowItemDrawer() {
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
            EditorApplication.RepaintProjectWindow();
        }

        //--------------------------------------------------------------------- 描画
        // ProjectWindowItem の描画
        static void ProjectWindowItemOnGUI(string guid, Rect rect) {
            if (guid == null || guid == string.Empty) {
                return;
            }

            // フォルダでなければ、このアイテム自体は描画しない
            string path = AssetDatabase.GUIDToAssetPath(guid);
            //if (!AssetDatabase.IsValidFolder(path)) {
            //    return;
            //}

            // コンテキスト取得
            FolderColorContext ctx = FolderColorContext.Instance;

            // この guid の色設定があるかどうか?
            if (ctx.HasItemInfo(guid)) {
                ItemInfo info = ctx.GetItemInfo(guid);
                if (info.isUseFolderColor) {
                    DrawIconColor(rect, info.folderColor);
                } else {
                    DrawIconColorWithSubFolderColor(ctx, path, rect);
                }
            } else {
                DrawIconColorWithSubFolderColor(ctx, path, rect);
            }
        }

        // サブディレクトリカラーを描画
        static void DrawIconColorWithSubFolderColor(FolderColorContext ctx, string path, Rect rect) {
            string parentGuid = GetParentPathGuid(path);
            if (parentGuid != null) {
                ItemInfo info = ctx.GetItemInfo(parentGuid);
                if (info != null) {
                    DrawIconColor(rect, info.folderColor);
                }
            }
        }

        // ProjectWindowItem アイコン色の描画
        static void DrawIconColor(Rect rect, Color color) {
            // 小さいアイコン (16px) の時じゃないと
            // 見た目が汚いので描画しないようにしておく
            bool isSmallIcon = (rect.height <= 16.0f) ? true : false;
            if (isSmallIcon) {
                var background = GUI.skin.box.normal.background;
                GUI.skin.box.normal.background = GetColorTexture(color);
                // アイコンカラーを描画
                Rect folderRect = rect;
                folderRect.x     += (rect.x < 30.0f)? 4.0f : 1.0f;
                folderRect.y     += 3.0f;
                folderRect.width  = 13.0f;
                folderRect.height = 12.0f;
                GUI.Box(folderRect, GUIContent.none);
                //// 帯カラーを描画
                //Rect lineRect = rect;
                //lineRect.x      = 0.0f;
                ////lineRect.y    = 0.0f;
                //lineRect.width  = 6.0f;
                //lineRect.height = 16.0f;
                //GUI.Box(lineRect, GUIContent.none);
                GUI.skin.box.normal.background = background;
            }
        }

        //--------------------------------------------------------------------- キャッシュクリア
        public static void ClearCache() {
            ClearPathGuidCache();
            ClearColorTextureCache();
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    // パスと Guid のキャッシュ
    public partial class FolderColorProjectWindowItemDrawer {
        //--------------------------------------------------------------------- 定義
        // パス GUID キャッシュ
        static Dictionary<string, string> pathGuidCache = null;

        // パス GUID キャッシュ取得インターフェイス
        static Dictionary<string, string> PathGuidCache {
            get {
                if (pathGuidCache == null) {
                    ReloadPathGuidCache();
                }
                return pathGuidCache;
            }
        }

        //--------------------------------------------------------------------- 設定のリロードなど
        // 設定のリロード
        public static void ReloadPathGuidCache() {
            // なければ作成。
            if (pathGuidCache == null) {
                pathGuidCache = new Dictionary<string, string>();
            }

            // 一旦クリアも実施。
            pathGuidCache.Clear();

            // ■ 注意
            // コンテキストから即座にキャッシュを復元する
            FolderColorContext ctx = FolderColorContext.Instance;
            foreach (var pair in ctx.ItemMap) {
                string guid = pair.Key;
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path != null && pair.Value.isChangeSubFolderColor) {
                    pathGuidCache[path] = guid;
                }
            }
        }

        // 自分のアイテムカラーを パス GUID キャッシュから探す
        public static string GetParentPathGuid(string path) {
            string p = path;
            while (p != null
                   && p != string.Empty
                   && (p = Path.GetDirectoryName(p)) != null) {
                if (PathGuidCache.ContainsKey(p)) {
                    return PathGuidCache[p];
                }
            }
            return null;
        }

        //--------------------------------------------------------------------- キャッシュクリア
        static void ClearPathGuidCache() {
            ReloadPathGuidCache();
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    // カラーテクスチャのキャッシュ
    public partial class FolderColorProjectWindowItemDrawer {
        //--------------------------------------------------------------------- 定義
        // 色テクスチャキャッシュ
        static Dictionary<Color, Texture2D> colorTextureCache = null;

        // 色テクスチャキャッシュ取得インターフェイス
        static Dictionary<Color, Texture2D> ColorTextureCache {
            get {
                if (colorTextureCache == null) {
                    colorTextureCache = new Dictionary<Color, Texture2D>();
                }
                return colorTextureCache;
            }
        }

        //--------------------------------------------------------------------- テクスチャの取得
        // テクスチャをキャッシュから取得
        static Texture2D GetColorTexture(Color color) {
            Texture2D texture = null;
            // キャッシュをチェックする
            if (ColorTextureCache.ContainsKey(color)) {
                texture = ColorTextureCache[color];
            }
            // キャッシュに無い場合は作成
            if (texture == null) {
                texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, color);
                texture.Apply();
                // ■ 注意
                // シーンに保存されないオブジェクトとして HideFlags.DontSave を設定。
                // これが無いとシーン保存時にリークエラーが表示されてしまう。
                // (シーン内のゲームオブジェクトに紐づけてないオブジェクトがインスタンス化されているため)
                // http://answers.unity3d.com/questions/133718/leaking-textures-in-custom-editor.html?sort=oldest
                texture.hideFlags = HideFlags.DontSave;
                ColorTextureCache[color] = texture;
            }
            return texture;
        }

        //--------------------------------------------------------------------- キャッシュクリア
        static void ClearColorTextureCache() {
            ColorTextureCache.Clear();
        }
    }
    #endif
}
