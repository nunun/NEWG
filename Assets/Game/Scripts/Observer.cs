using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// オブサーバ
// サブジェクトを払い出し、サブジェクトの完了を監視するためのクラスです。
public partial class Observer {
    //-------------------------------------------------------------------------- 変数
    // サブジェクト一覧
    List<Subject> subjects = new List<Subject>();

    // TODO
    // 各種情報の取得
    public string Message  { get; protected set; }
    public float  Progress { get; protected set; }
    public float  isDone   { get; protected set; }

    //-------------------------------------------------------------------------- オブザーブ
    // サブジェクトの作成
    public Subject Observe(string name = null) {
        var subject = Subject.RentObject(this, name);
        subjects.Add(subject);
        return subject;
    }

    //-------------------------------------------------------------------------- 操作
    // オブザーバの更新
    // NOTE これを呼ばないと状態が更新されません。
    public void Update() {
        // TODO
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サブジェクトの実装
public partial class Observer {
    public partial class Subject {
        //---------------------------------------------------------------------- 変数
        public Observer observer   = null;  // このサブジェクトのオブザーバ
        public string   name       = null;  // 名前
        public string   message    = null;  // メッセージ
        public float    prgress    = 0.0f;  // 進捗
        public bool     isDone     = false; // 完了フラグ
        public bool     isDisposed = false; // 破棄フラグ

        //---------------------------------------------------------------------- 確保と解放
        public static Subject RentObject(Observer observer, string name = null) {
            var subject = ObjectPool<Subject>.RentObject();
            subject.observer   = observer;
            subject.name       = name;
            subject.message    = null;
            subject.progress   = 0.0f;
            subject.isDone     = false;
            subject.isDisposed = false;
            return subject;
        }

        public static void ReturnObject(Subject subject) {
            subject.observer   = null;
            subject.name       = null;
            subject.message    = null;
            subject.progress   = 0.0f;
            subject.isDone     = false;
            subject.isDisposed = false;
            ObjectPool<Subject>.ReturnObject(this);
        }

        //---------------------------------------------------------------------- 操作
        public void Done() {
            isDone = true;
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サブジェクトの IDisposable 対応
public partial class Observer {
    public partial class Subject : IDisposable {
        //---------------------------------------------------------------------- 操作
        public void Dispose() {
            Dispose(true);
        }

        //---------------------------------------------------------------------- 実装 (IDisposable)
        protected override void Dispose(bool disposing) {
            if (!isDisposed) {
                isDisposed = true;
                Debug.Assert(
                Done();
            }
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サブジェクトの Coroutine 対応
public partial class Object {
    public partial class Subject : CustomYieldInstruction {
        //---------------------------------------------------------------------- 変数
        public override bool keepWaiting { get { return !isDone; }}
    }
}
