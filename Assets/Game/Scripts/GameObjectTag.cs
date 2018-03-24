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
    protected static GameObjectTag FindGameObjectTag(string tagName, GameObject parent, bool forceParent, bool rent) {
        var found = default(GameObjectTag);
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
                    if (p != null) {
                        found = gameObjectTag;//指定した親を持つタグが見つかった
                        break;
                    }
                }
                // 指定した親に限定する場合、親が見つからない場合は次
                if (forceParent) {
                    continue;
                }
                // 親を指定しない場合は、一つ見つかったら終わり。
                // 親を指定した場合は、親を持つタグが見つかるまで全て探す。
                found = gameObjectTag;
                if (parent == null) {
                    break;
                }
            }
        }
        if (rent && found != null) {
            gameObjectTags.Remove(found);
        }
        return found;
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
    public static T RentObject(string tagName = null, GameObject parent = null, bool forceParent = false) {
        tagName = tagName ?? typeof(T).Name;
        var gameObjectTag = FindGameObjectTag(tagName, parent, forceParent, true);
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
    public static T Find(string tagName = null, GameObject parent = null, bool forceParent = false) {
        tagName = tagName ?? typeof(T).Name;
        var gameObjectTag = FindGameObjectTag(tagName, parent, forceParent, false);
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
