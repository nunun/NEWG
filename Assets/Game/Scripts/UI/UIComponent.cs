using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [UI コンポーネントの実装例]
//public class SampleUI : UIComponent<string> {
//    public static SampleUI Open(Action<string> callback = null) {
//        var component = GameObjectPool<SampleUI>.RentGameObject();
//        component.SetUICallback(callback);
//        component.Open();
//        return component;
//    }
//    void ReturnToPool() {
//        SetUIDone();
//        GameObjectPool<SampleUI>.ReturnObject(this);
//    }
//    void Awake() {
//        SetUIRecycle(ReturnToPool);
//    }
//    void OnDestroy() {
//        SetUIDone();
//    }
//}
//var ui = SampleUI.RentFromPool(callback);
//ui.Close();
//GameObject.Destroy(ui.gameObject);


// UI コンポーネント
public class UIComponent : UIBehaviour {
    UIResult result = null;
    protected void SetUICallback(Action<string> callback) {
        Debug.Assert(this.result == null);
        this.result = UIResult.RentFromPool(callback);
    }
    protected void SetUIResult(string error) {
        Debug.Assert(this.result != null);
        var result = this.result as UIResult;
        Debug.Assert(result != null);
        result.SetUIResult(error);
    }
    protected void SetUIDone() {
        var result = this.result;
        this.result = null;
        if (result != null) {
            result.Callback();
            result.ReturnToPool();
        }
    }
}

// UI コンポーネント <T1>
public class UIComponent<T1> : UIBehaviour {
    UIResult<T1> result = null;
    protected void SetUICallback(Action<string,T1> callback) {
        Debug.Assert(this.result == null);
        this.result = UIResult<T1>.RentFromPool(callback);
    }
    protected void SetUIResult(string error, T1 v1) {
        Debug.Assert(this.result != null);
        var result = this.result as UIResult<T1>;
        Debug.Assert(result != null);
        result.SetUIResult(error, v1);
    }
    protected void SetUIDone() {
        var result = this.result;
        this.result = null;
        if (result != null) {
            result.Callback();
            result.ReturnToPool();
        }
    }
}

// UI コンポーネント <T1,T2>
public class UIComponent<T1,T2> : UIBehaviour {
    UIResult<T1,T2> result = null;
    protected void SetUICallback(Action<string,T1,T2> callback) {
        Debug.Assert(this.result == null);
        this.result = UIResult<T1,T2>.RentFromPool(callback);
    }
    protected void SetUIResult(string error, T1 v1, T2 v2) {
        Debug.Assert(this.result != null);
        var result = this.result as UIResult<T1,T2>;
        Debug.Assert(result != null);
        result.SetUIResult(error, v1, v2);
    }
    protected void SetUIDone() {
        var result = this.result;
        this.result = null;
        if (result != null) {
            result.Callback();
            result.ReturnToPool();
        }
    }
}

// UI コンポーネント <T1,T2,T3>
public class UIComponent<T1,T2,T3> : UIBehaviour {
    UIResult<T1,T2,T3> result = null;
    protected void SetUICallback(Action<string,T1,T2,T3> callback) {
        Debug.Assert(this.result == null);
        this.result = UIResult<T1,T2,T3>.RentFromPool(callback);
    }
    protected void SetUIResult(string error, T1 v1, T2 v2, T3 v3) {
        Debug.Assert(this.result != null);
        var result = this.result as UIResult<T1,T2,T3>;
        Debug.Assert(result != null);
        result.SetUIResult(error, v1, v2, v3);
    }
    protected void SetUIDone() {
        var result = this.result;
        this.result = null;
        if (result != null) {
            result.Callback();
            result.ReturnToPool();
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// UI 結果
public class UIResult {
    string         error    = default(string);
    Action<string> callback = default(Action<string>);
    public static UIResult RentFromPool(Action<string> callback) {
        var result = ObjectPool<UIResult>.RentObject();
        result.error    = default(string);
        result.callback = callback;
        return result;
    }
    public void SetUIResult(string error) {
        this.error = error;
    }
    public void Callback() {
        if (this.callback != null) {
            var callback = this.callback;
            this.callback = null;
            callback(this.error);
        }
        this.error    = default(string);
        this.callback = default(Action<string>);
    }
    public void Abort(string error) {
        SetUIResult(error ?? "aborted.");
        Callback();
    }
    public void ReturnToPool() {
        ObjectPool<UIResult>.ReturnObject(this);
    }
}

// UI 結果 <T1>
public class UIResult<T1> {
    string            error    = default(string);
    T1                v1       = default(T1);
    Action<string,T1> callback = default(Action<string,T1>);
    public static UIResult<T1> RentFromPool(Action<string,T1> callback) {
        var result = ObjectPool<UIResult<T1>>.RentObject();
        result.error    = default(string);
        result.v1       = default(T1);
        result.callback = callback;
        return result;
    }
    public void SetUIResult(string error, T1 v1) {
        this.error = error;
        this.v1    = v1;
    }
    public void Callback() {
        if (this.callback != null) {
            var callback = this.callback;
            this.callback = null;
            callback(this.error, this.v1);
        }
        this.error    = default(string);
        this.v1       = default(T1);
        this.callback = default(Action<string,T1>);
        ObjectPool<UIResult<T1>>.ReturnObject(this);
    }
    public void Abort(string error) {
        SetUIResult(error ?? "aborted.", default(T1));
        Callback();
    }
    public void ReturnToPool() {
        ObjectPool<UIResult<T1>>.ReturnObject(this);
    }
}

// UI 結果 <T1,T2>
public class UIResult<T1,T2> {
    string               error    = default(string);
    T1                   v1       = default(T1);
    T2                   v2       = default(T2);
    Action<string,T1,T2> callback = default(Action<string,T1,T2>);
    public static UIResult<T1,T2> RentFromPool(Action<string,T1,T2> callback) {
        var result = ObjectPool<UIResult<T1,T2>>.RentObject();
        result.error    = default(string);
        result.v1       = default(T1);
        result.v2       = default(T2);
        result.callback = callback;
        return result;
    }
    public void SetUIResult(string error, T1 v1, T2 v2) {
        this.error = error;
        this.v1    = v1;
        this.v2    = v2;
    }
    public void Callback() {
        if (this.callback != null) {
            var callback = this.callback;
            this.callback = null;
            callback(this.error, this.v1, this.v2);
        }
        this.error    = default(string);
        this.v1       = default(T1);
        this.v2       = default(T2);
        this.callback = default(Action<string,T1,T2>);
    }
    public void Abort(string error) {
        SetUIResult(error ?? "aborted.", default(T1), default(T2));
        Callback();
    }
    public void ReturnToPool() {
        ObjectPool<UIResult<T1,T2>>.ReturnObject(this);
    }
}

// UI 結果 <T1,T2,T3>
public class UIResult<T1,T2,T3> {
    string                  error    = default(string);
    T1                      v1       = default(T1);
    T2                      v2       = default(T2);
    T3                      v3       = default(T3);
    Action<string,T1,T2,T3> callback = default(Action<string,T1,T2,T3>);
    public static UIResult<T1,T2,T3> RentFromPool(Action<string,T1,T2,T3> callback) {
        var result = ObjectPool<UIResult<T1,T2,T3>>.RentObject();
        result.error    = default(string);
        result.v1       = default(T1);
        result.v2       = default(T2);
        result.v3       = default(T3);
        result.callback = callback;
        return result;
    }
    public void SetUIResult(string error, T1 v1, T2 v2, T3 v3) {
        this.error = error;
        this.v1    = v1;
        this.v2    = v2;
        this.v3    = v3;
    }
    public void Callback() {
        if (this.callback != null) {
            var callback = this.callback;
            this.callback = null;
            callback(this.error, this.v1, this.v2, this.v3);
        }
        this.error    = default(string);
        this.v1       = default(T1);
        this.v2       = default(T2);
        this.v3       = default(T3);
        this.callback = default(Action<string,T1,T2,T3>);
    }
    public void Abort(string error) {
        SetUIResult(error ?? "aborted.", default(T1), default(T2), default(T3));
        Callback();
    }
    public void ReturnToPool() {
        ObjectPool<UIResult<T1,T2,T3>>.ReturnObject(this);
    }
}
