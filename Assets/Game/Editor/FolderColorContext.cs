using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif


namespace GameEditor {
    #if UNITY_EDITOR
    // フォルダカラーコンテキスト
    [Serializable]
    public class FolderColorContext {
        //--------------------------------------------------------------------- 定義
        // アイテムの色情報
        public class ItemInfo {
            public string guid;
            public Color  folderColor;
            public Color  annotationColor;
            public string annotationText;
            public bool   isUseFolderColor;
            public bool   isUseAnnotationColor;
            public bool   isUseAnnotationText;
            public bool   isChangeSubFolderColor;

            // コンストラクタ
            public ItemInfo() {
                guid                   = null;
                folderColor            = new Color(0.0f, 0.0f, 0.0f, 0.28f);
                annotationColor        = new Color(1.0f, 0.0f, 0.0f, 0.85f);
                isUseFolderColor       = false;
                isUseAnnotationColor   = false;
                isUseAnnotationText    = false;
                isChangeSubFolderColor = false;
            }

            // コンストラクタ (Guid 指定)
            public ItemInfo(string assetGuid) : this() {
                guid = assetGuid;
            }

            // コピーコンストラクタ
            public ItemInfo(ItemInfo info) {
                guid = info.guid;
                Set(info);
            }

            // 設定
            public void Set(ItemInfo info) {
                //guid                 = info.guid;
                folderColor            = info.folderColor;
                annotationColor        = info.annotationColor;
                isUseFolderColor       = info.isUseFolderColor;
                isUseAnnotationColor   = info.isUseAnnotationColor;
                isUseAnnotationText    = info.isUseAnnotationText;
                isChangeSubFolderColor = info.isChangeSubFolderColor;
            }
        }

        // "Assets" の GUID (定数)
        const string ROOT_GUID = "00000000000000001000000000000000";

        // 設定ファイル
        const string SETTINGS_PATH = "Assets/FolderColorSettings.xml";

        // インスタンス
        static FolderColorContext instance = null;

        // インスタンスの取得
        public static FolderColorContext Instance {
            get {
                if (instance == null) {
                    instance = Load();
                }
                return instance;
            }
        }

        //--------------------------------------------------------------------- 変数
        // 色情報リスト (保存用)
        [XmlArray("ItemList")]
        [XmlArrayItem("ItemInfo", typeof(ItemInfo))]
        public List<ItemInfo> itemList = new List<ItemInfo>();

        // 色情報ディクショナリ
        [XmlIgnore]
        private Dictionary<string,ItemInfo> itemMap = new Dictionary<string,ItemInfo>();

        // 色情報ディクショナリの取得
        [XmlIgnore]
        public Dictionary<string,ItemInfo> ItemMap { get { return itemMap; }}

        //--------------------------------------------------------------------- 保存とロード
        // 読込
        public static FolderColorContext Load() {
            var ctx = default(FolderColorContext);
            if (!File.Exists(SETTINGS_PATH)) {
                ctx = new FolderColorContext();
                ctx.Save();
            }

            // デシリアライズ
            try {
                var serializer = new XmlSerializer(typeof(FolderColorContext));
                using (var sr = new StreamReader(SETTINGS_PATH, Encoding.UTF8)) {
                    ctx = serializer.Deserialize(sr) as FolderColorContext;
                }

                // リストからマップに変換
                ctx.itemMap.Clear();
                foreach (ItemInfo info in ctx.itemList) {
                    ctx.itemMap[info.guid] = info;
                }
            } catch {
                ctx = new FolderColorContext();
            }
            return ctx;
        }

        // 保存
        public void Save() {
            // マップからリストに変換
            itemList.Clear();
            foreach (var pair in itemMap) {
                itemList.Add(pair.Value);
            }

            // ディレクトリが無ければ作成
            var dir = Path.GetDirectoryName(SETTINGS_PATH);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            // シリアライズ
            var serializer = new XmlSerializer(typeof(FolderColorContext));
            using (var sw = new StreamWriter(SETTINGS_PATH, false, Encoding.UTF8)) {
                serializer.Serialize(sw, this);
            }

            // アセットデータベースをリフレッシュ
            AssetDatabase.Refresh();
        }

        //--------------------------------------------------------------------- getter/setter
        // 色情報があるかどうか
        public bool HasItemInfo(string guid) {
            return itemMap.ContainsKey(guid);
        }

        // 色情報の取得
        public ItemInfo GetItemInfo(string guid) {
            if (guid == null) {
                guid = ROOT_GUID;
            }
            if (itemMap.ContainsKey(guid)) {
                return itemMap[guid];
            }
            return null;
        }

        // 色情報の獲得 (無い場合は作成)
        public ItemInfo ObtainItemInfo(string guid) {
            ItemInfo info = GetItemInfo(guid);
            if (info == null) {
                info = new ItemInfo(guid);
                itemMap[guid] = info;
            }
            return info;
        }

        // 色情報の設定
        public void SetItemInfo(string guid, ItemInfo info) {
            // ■ 注意
            // アイコン色または注釈色の設定が無いときは
            // 設定を削除する点に注意
            if (info == null
                || (info.isUseFolderColor == false
                    && info.isUseAnnotationColor == false)) {
                RemoveItemInfo(guid);
            } else {
                itemMap[guid] = info;
            }
        }

        // 色情報の削除
        public void RemoveItemInfo(string guid) {
            if (itemMap.ContainsKey(guid)) {
                itemMap.Remove(guid);
            }
        }
    }
    #endif
}
