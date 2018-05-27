// TaskQueue
// 投入されたタスクを上から順に一定数並列に処理するキューです。
class TaskQueue {
    //-------------------------------------------------------------------------- 生成と破棄
    // コンストラクタ
    constructor(config, logger) {
        this.config   = config;
        this.logger   = logger;
        this.queue    = [];
        this.parallel = config.parallel || 3;

        // 各種イベントハンドラ
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
    // 中断イベントリスナの設定
    setAbortEventListener(eventListener) {
        this.abortEventListener = eventListener;
    }

    //-------------------------------------------------------------------------- データの追加と削除と中断
    // 追加
    add(action, key, data) {
        var task = {action:action, data:data, key:key, busy:false};
        this.queue.push(task);
        this.update();
    }

    // 削除
    remove(task) {
        var i = this.indexOf(task);
        if (i >= 0) {
            var queuedTask = this.queue[i];
            this.queue.splice(i, 1);
        }
    }

    // キーから削除
    removeKey(key) {
        var i = this.indexOfKey(key);
        if (i >= 0) {
            var queuedTask = this.queue[i];
            this.queue.splice(i, 1);
        }
    }

    // 中断
    abort(err, task) {
        this.abortEventListener(err, task);
    }

    // キーから中断
    abortKey(err, key) {
        var i = this.indexOfKey(key);
        if (i >= 0) {
            var queuedTask = this.queue[i];
            this.abort(err, queuedTask);
        }
    }

    // タスクを探す
    indexOf(task) {
        for (i = this.queue.length - 1; i >= 0; i--) {
            var queuedTask = this.queue[i];
            if (queuedTask == task) {
                return i;
            }
        }
        return -1;
    }

    // タスクをキーから探す
    indexOfKey(key) {
        for (i = this.queue.length - 1; i >= 0; i--) {
            var queuedTask = this.queue[i];
            if ((queuedTask.key instanceof Array && queuedTask.key.indexOf(key) >= 0)
                || (queuedTask.key == key)) {
                return i;
            }
        }
        return -1;
    }

    //-------------------------------------------------------------------------- 更新と処理
    // 更新
    update() {
        // キューが空
        if (this.queue.length <= 0) {
            return;
        }

        // 処理中でないタスクを処理
        for (var i = 0; i < this.parallel; i++) {
            var task = this.queue[i];
            if (!task.busy) {
                exec(task);
            }
        }
    }

    // タスクの実行
    async exec(task) {
        task.busy = true;
        try {
            nextAction = await task.action(this, task);
            task.busy = false;
            if (!nextAction) {
                this.remove(task);
                return;
            }
            task.action = nextAction;
        } catch (err) {
            task.busy = false;
            this.abort(err, task);
        } finally {
            this.update();
        }
    }
}
module.exports = TaskQueue;
