using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームオブジェクトタグ
[DefaultExecutionOrder(int.MinValue)]
public class GameObjectTag : MonoBehaviour {
    //------------------------------------------------------------------------- 変数
    // 検索用タグ名 (Awake 時に確定)
    string tagName = null;

    // ストック一覧
    static List<GameObjectTag> gameObjectTags = new List<GameObjectTag>();

    //------------------------------------------------------------------------- 操作
    protected static GameObjectTag FindGameObjectTag(string tagName, GameObject parent, bool rent) {
        for (int i = gameObjectTags.Count - 1; i >= 0; i--) {
            var gameObjectTag = gameObjectTags[i];
            if (gameObjectTag.tagName == tagName) {
                if (parent != null) {
                    var p = gameObjectTag.gameObject;
                    while (p != null) {
                        if (p == parent) {
                            break;
                        }
                        p = p.transform.parent.gameObject;
                    }
                    if (p == null) {
                        continue;
                    }
                }
                if (rent) {
                    gameObjectTags.Remove(gameObjectTag);
                }
                return gameObjectTag;
            }
        }
        return null;
    }

    protected static void ReturnGameObjectTag(GameObjectTag gameObjectTag) {
        gameObjectTags.Add(gameObjectTag);
    }

    //------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void Awake() {
        // NOTE
        // ひとまず、ゲームオブジェクトタグを貼り付けた
        // オブジェクト名で検索できるようにしておく。
        this.tagName = name;
        gameObjectTags.Add(this);
    }

    void OnDestroy() {
        gameObjectTags.Remove(this);
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// ゲームオブジェクトタグ (検索用インターフェイス)
public class GameObjectTag<T> : GameObjectTag where T : Component {
    //------------------------------------------------------------------------- 貸し出しと返却
    public static T RentObject(string tagName = null, GameObject parent = null) {
        tagName = tagName ?? typeof(T).Name;
        var gameObjectTag = FindGameObjectTag(tagName, parent, true);
        if (gameObjectTag == null) {
            return default(T);
        }
        if (typeof(T) == typeof(GameObjectTag)) {
            return gameObjectTag as T;
        }
        return gameObjectTag.gameObject.GetComponent<T>();
    }

    public static T RentObject(GameObject parent) {
        return RentObject(null, parent);
    }

    public static void ReturnObject(T instance) {
        var gameObjectTag = instance.gameObject.GetComponent<GameObjectTag>();
        Debug.Assert(gameObjectTag != null, "ゲームオブジェクトタグなし");
        ReturnGameObjectTag(gameObjectTag);
    }

    //------------------------------------------------------------------------- 検索
    public static T Find(string tagName = null, GameObject parent = null) {
        tagName = tagName ?? typeof(T).Name;
        var gameObjectTag = FindGameObjectTag(tagName, parent, false);
        if (gameObjectTag == null) {
            return default(T);
        }
        if (typeof(T) == typeof(GameObjectTag)) {
            return gameObjectTag as T;
        }
        return gameObjectTag.gameObject.GetComponent<T>();
    }

    public static T Find(GameObject parent) {
        return Find(null, parent);
    }
}
