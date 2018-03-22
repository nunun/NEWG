using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI 関連の非同期コールバックレシーバ
public class UIWait : AsyncCallback {
    Action<string> callback = null;
    public Action<string> Callback { get { return callback; }}
    public static UIWait RentFromPool() {
        var instance = ObjectPool<UIWait>.RentObject();
        instance.callback   = instance.callback ?? instance.WaitCallback;
        instance.isDone     = false;
        instance.error      = null;
        instance.isDisposed = false;
        return instance;
    }
    public void ReturnToPool() {
        //this.callback = null;
        this.error      = null;
        this.isDone     = false;
        ObjectPool<UIWait>.ReturnObject(this);
    }
    void WaitCallback(string error) {
        Debug.Assert(!isDisposed && !isDone, "コールバック不正");
        //this.callback = null;
        this.error      = error;
        this.isDone     = true;
    }
    protected override void Dispose(bool disposing) {
        if (!isDisposed) {
            isDisposed = true;
            ReturnToPool();
        }
    }
}

// UI 関連の非同期コールバックレシーバ (T1)
public class UIWait<T1> : AsyncCallback {
    Action<string,T1> callback = null;
    T1                v1       = default(T1);
    public Action<string,T1> Callback { get { return callback; }}
    public T1                Value1   { get { return v1;       }}
    public static UIWait<T1> RentFromPool() {
        var instance = ObjectPool<UIWait<T1>>.RentObject();
        instance.callback   = instance.callback ?? instance.WaitCallback;
        instance.error      = null;
        instance.v1         = default(T1);
        instance.isDone     = false;
        instance.isDisposed = false;
        return instance;
    }
    public void ReturnToPool() {
        //this.callback = null;
        this.error      = null;
        this.v1         = default(T1);
        this.isDone     = false;
        ObjectPool<UIWait<T1>>.ReturnObject(this);
    }
    void WaitCallback(string error, T1 v1) {
        Debug.Assert(!isDone, "コールバック二重コール");
        //this.callback = null;
        this.error      = error;
        this.v1         = v1;
        this.isDone     = true;
    }
    protected override void Dispose(bool disposing) {
        if (!isDisposed) {
            isDisposed = true;
            ReturnToPool();
        }
    }
}

// UI 関連の非同期コールバックレシーバ (T1, T2)
public class UIWait<T1,T2> : AsyncCallback {
    Action<string,T1,T2> callback = null;
    T1                   v1       = default(T1);
    T2                   v2       = default(T2);
    public Action<string,T1,T2> Callback { get { return callback; }}
    public T1                   Value1   { get { return v1;       }}
    public T2                   Value2   { get { return v2;       }}
    public static UIWait<T1,T2> RentFromPool() {
        var instance = ObjectPool<UIWait<T1,T2>>.RentObject();
        instance.callback   = instance.callback ?? instance.WaitCallback;
        instance.error      = null;
        instance.v1         = default(T1);
        instance.v2         = default(T2);
        instance.isDone     = false;
        instance.isDisposed = false;
        return instance;
    }
    public void ReturnToPool() {
        //this.callback = null;
        this.error      = null;
        this.v1         = default(T1);
        this.v2         = default(T2);
        this.isDone     = false;
        ObjectPool<UIWait<T1,T2>>.ReturnObject(this);
    }
    void WaitCallback(string error, T1 v1, T2 v2) {
        Debug.Assert(!isDone, "コールバック二重コール");
        //this.callback = null;
        this.error      = error;
        this.v1         = v1;
        this.v2         = v2;
        this.isDone     = true;
    }
    protected override void Dispose(bool disposing) {
        if (!isDisposed) {
            isDisposed = true;
            ReturnToPool();
        }
    }
}

// UI 関連の非同期コールバックレシーバ (T1, T2, T3)
public class UIWait<T1,T2,T3> : AsyncCallback {
    Action<string,T1,T2,T3> callback = null;
    T1                      v1       = default(T1);
    T2                      v2       = default(T2);
    T3                      v3       = default(T3);
    public Action<string,T1,T2,T3> Callback { get { return callback; }}
    public T1                      Value1   { get { return v1;       }}
    public T2                      Value2   { get { return v2;       }}
    public T3                      Value3   { get { return v3;       }}
    public static UIWait<T1,T2,T3> RentFromPool() {
        var instance = ObjectPool<UIWait<T1,T2,T3>>.RentObject();
        instance.callback   = instance.callback ?? instance.WaitCallback;
        instance.error      = null;
        instance.v1         = default(T1);
        instance.v2         = default(T2);
        instance.v3         = default(T3);
        instance.isDone     = false;
        instance.isDisposed = false;
        return instance;
    }
    public void ReturnToPool() {
        //this.callback = null;
        this.error      = null;
        this.v1         = default(T1);
        this.v2         = default(T2);
        this.v3         = default(T3);
        this.isDone     = false;
        ObjectPool<UIWait<T1,T2,T3>>.ReturnObject(this);
    }
    void WaitCallback(string error, T1 v1, T2 v2, T3 v3) {
        Debug.Assert(!isDone, "コールバック二重コール");
        //this.callback = null;
        this.error      = error;
        this.v1         = v1;
        this.v2         = v2;
        this.v3         = v3;
        this.isDone     = true;
    }
    protected override void Dispose(bool disposing) {
        if (!isDisposed) {
            isDisposed = true;
            ReturnToPool();
        }
    }
}
