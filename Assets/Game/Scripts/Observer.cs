using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// オブサーバ
// サブジェクトを発行し、その完了を監視するためのクラスです。
public partial class Observer {
    //-------------------------------------------------------------------------- 変数
    string currentMessage;
    bool   dirtyMessage;
    float  currentProgress;
    bool   dirtyProgress;

    // サブジェクト一覧
    List<Subject> subjects = new List<Subject>();

    // 各種情報の取得
    public string message         { get { dirtyMessage  = false; return currentMessage;  } protected set { currentMessage  = value; }}
    public bool   isDirtyMessage  { get { return dirtyMessage;  }}
    public float  progress        { get { dirtyProgress = false; return currentProgress; } protected set { currentProgress = value; }}
    public bool   isDirtyProgress { get { return dirtyProgress; }}
    public bool   isDone          { get; protected set; }
    public int    Count           { get { return subjects.Count; }}

    //-------------------------------------------------------------------------- クリア
    public void Clear() {
        message  = "";
        progress = 0.0f;
        isDone   = false;
        subjects.Clear();
    }

    //-------------------------------------------------------------------------- オブザーブ
    // サブジェクトの作成
    public Subject Observe(string name = null) {
        Debug.Assert(!isDone, "既に完了した");
        var subject = Subject.RentObject(this, name);
        subjects.Add(subject);
        return subject;
    }

    // サブジェクトの作成
    public Subject Find(string name) {
        for (int i = 0; i < subjects.Count; i++) {
            var subject = subjects[i];
            if (subject.name == name) {
                return subject;
            }
        }
        return null;
    }

    //-------------------------------------------------------------------------- 操作
    // オブザーバの更新
    // NOTE これを呼ばないと状態が更新されません。
    public void Update() {
        if (isDone) {
            return; // 既に完了した
        }
        var message       = default(string);
        var progress      = 0.0f;
        var totalProgress = 0.0f;

        // サブジェクト
        for (int i = subjects.Count - 1; i >= 0; i--) {
            var subject = subjects[i];
            if (subject.isDone) {
                subjects.RemoveAt(i);
            }
            message        = subject.message ?? message;
            progress      += subject.progress;
            totalProgress += 1.0f;
        }

        // メッセージ更新
        if (currentMessage != message) {
            currentMessage = message;
            dirtyMessage   = true;
        }

        // プログレス更新
        progress = Mathf.Clamp(progress / totalProgress, 0.0f, 1.0f);
        if (currentProgress != progress) {
            currentProgress = progress;
            dirtyProgress   = true;
        }

        // 完了？
        if (subjects.Count <= 0) {
            isDone = true;
        }
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
        public float    progress   = 0.0f;  // 進捗
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
            ObjectPool<Subject>.ReturnObject(subject);
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
        protected void Dispose(bool disposing) {
            if (!isDisposed) {
                isDisposed = true;
                Done();
            }
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// サブジェクトの Coroutine 対応
public partial class Observer {
    public partial class Subject : CustomYieldInstruction {
        //---------------------------------------------------------------------- 変数
        public override bool keepWaiting { get { return !isDone; }}
    }
}
