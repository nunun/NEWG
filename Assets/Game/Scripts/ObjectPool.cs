using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// オブジェクトプール
public class ObjectPool<T> where T : new() {
    //-------------------------------------------------------------------------- 定義
    static readonly int POOL_MAX_LENGTH = 16; // プールの最大長

    //-------------------------------------------------------------------------- 変数
    static Queue<T> pool = null; // オブジェクトプール

    //-------------------------------------------------------------------------- プールのクリアと後始末
    public static void Clear() {
        if (pool != null) {
            pool.Clear();
        }
    }

    public static void Dispose() {
        Clear();
        pool = null;
    }

    //-------------------------------------------------------------------------- オブジェクトの確保と開放
    public static T RentObject() {
        if (pool != null && pool.Count > 0) {
            return pool.Dequeue();
        }
        return new T();
    }

    public static void ReturnObject(T instance) {
        if (pool == null) {
            pool = new Queue<T>();
        }
        if (pool.Count < POOL_MAX_LENGTH) {
            pool.Enqueue(instance);
        }
    }
}
