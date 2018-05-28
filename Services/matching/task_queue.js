// TaskQueue
// 投入されたタスクを上から順に一定数並列に処理するキューです。
class TaskQueue {
    //-------------------------------------------------------------------------- 生成と破棄
    // コンストラクタ
    constructor(config, logger) {
        this.config   = config;
        this.logger   = logger;
        this.queue    = [];
        this.capacity = config.capacity || -1;
        this.parallel = (config.parallel === null)? 1 : config.parallel;

        // 各種イベントハンドラ
        this.addEventListener   = null;
        this.abortEventListener = null;

        // 念のための更新タイマー
        setTimeout(() => {
            this.update();
        }, 500);
    }

    // アクティベート
    static activate(config, logger) {
        return new TaskQueue(config, logger);
    }

    //-------------------------------------------------------------------------- 各種イベントリスナ
    // 追加イベントリスナの設定
    setAddEventListener(eventListener) {
        this.addEventListener = eventListener;
    }

    // 中断イベントリスナの設定
    setAbortEventListener(eventListener) {
        this.abortEventListener = eventListener;
    }

    //-------------------------------------------------------------------------- キュー操作
    // タスクを取得
    getTaskAt(index) {
        return this.queue[index];
    }

    // キャパシティの取得
    getLength() {
        return this.queue.length;
    }

    // キャパシティの取得
    getCapacity() {
        return this.capacity;
    }

    // 満杯チェック
    isFull() {
        return (this.capacity >= 0 && this.queue.length >= this.capacity);
    }

    //-------------------------------------------------------------------------- タスク操作
    // キーからタスクを作成
    static createTaskFromKey(key) {
        return {_action:null, _key:key, _busy:false, _count:0};
    }

    // タスクを追加
    add(task) {
        if (this.isFull()) {
            return false;
        }
        if (this.addEventListener) {
            task._action = this.addEventListener(task);
            task._count  = 0;
        }
        this.queue.push(task);
        this.update();
        return true;
    }

    // タスクを削除
    remove(task) {
        var i = this.indexOf(task);
        if (i >= 0) {
            var queuedTask = this.queue[i];
            this.queue.splice(i, 1);
        }
    }

    // タスクをキーから削除
    removeKey(key) {
        var i = this.indexOfKey(key);
        if (i >= 0) {
            var queuedTask = this.queue[i];
            this.queue.splice(i, 1);
        }
    }

    // タスクを中断
    abort(err, task) {
        if (this.abortEventListener) {
            this.abortEventListener(err, task);
        }
    }

    // タスクをキーから中断
    abortKey(err, key) {
        var i = this.indexOfKey(key);
        if (i >= 0) {
            var queuedTask = this.queue[i];
            this.abort(err, queuedTask);
        }
    }

    // タスクを探す
    indexOf(task) {
        for (var i = this.queue.length - 1; i >= 0; i--) {
            var queuedTask = this.queue[i];
            if (queuedTask == task) {
                return i;
            }
        }
        return -1;
    }

    // タスクをキーから探す
    indexOfKey(key) {
        for (var i = this.queue.length - 1; i >= 0; i--) {
            var queuedTask = this.queue[i];
            if ((queuedTask._key instanceof Array && queuedTask._key.indexOf(key) >= 0)
                || (queuedTask._key == key)) {
                return i;
            }
        }
        return -1;
    }

    //-------------------------------------------------------------------------- タスクの更新
    // 更新
    update() {
        // キューが空
        if (this.queue.length <= 0) {
            return;
        }

        // 処理中でないタスクを処理
        for (var i = 0; i < this.parallel; i++) {
            var task = this.queue[i];
            if (!task._busy) {
                this.action(task);
            }
        }
    }

    // タスクの実行
    async action(task) {
        // タスクを処理
        task._busy = true;
        try {
            var next = null;
            if(task._action) {
                next = await task._action(task);
                task._count++;
            }
            if (next) {
                if (typeof next == "number") {
                    await this.sleep(next);
                } else {
                    task._action = next;
                    task._count  = 0;
                }
            }
            task._busy = false;
        } catch (err) {
            task._busy = false;
            this.abort(err, task);
        } finally {
            this.update();
        }
    }

    // スリープ
    sleep(mtime) {
        return new Promise((resolved) => {
            setTimeout(resolved, mtime);
        });
    }
}
module.exports = TaskQueue;
