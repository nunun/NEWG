using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 非同期コールバックレシーバ親クラス
// 詳しい実装は UIWait.cs 等を参照して下さい。
public partial class AsyncCallback : CustomYieldInstruction {
    //-------------------------------------------------------------------------- 変数
    public string error  { get; protected set; }
    public bool   isDone { get; protected set; }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// Dispose 対応
public partial class AsyncCallback : IDisposable {
    //-------------------------------------------------------------------------- 変数
    protected bool isDisposed { get; set; }

    //-------------------------------------------------------------------------- 操作
    public void Dispose() {
        Dispose(true);
    }

    //-------------------------------------------------------------------------- 実装 (IDisposable)
    protected virtual void Dispose(bool disposing) {
        // NOTE
        // 継承して実装
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// Coroutine 対応
public partial class AsyncCallback : CustomYieldInstruction {
    //-------------------------------------------------------------------------- 変数
    public override bool keepWaiting { get { return !isDone; }}
}
