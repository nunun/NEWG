using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// インスタンスコンテナ
public class InstanceContainer<T> {
    //-------------------------------------------------------------------------- 変数
    Dictionary<string,T> instances       = new Dictionary<string,T>();
    T                    defaultInstance = default(T);

    //-------------------------------------------------------------------------- 初期化
    // コンストラクタ
    public InstanceContainer() {
        Init();
    }

    //-------------------------------------------------------------------------- 実装ポイント
    // 初期化
    protected virtual void Init() {
        Clear();
    }

    // クリア
    protected virtual void Clear() {
        instances.Clear();
        defaultInstance = null;
    }

    //-------------------------------------------------------------------------- 操作
    public T Find(string name) {
        if (string.IsNullOrEmpty(name)) {
            return defaultInstance;
        }
        if (instances.HasKey(name)) {
            return instances[name];
        }
        return null;
    }

    public void Add(string[] nameConfig, T instance) {
        if (nameConfig == null || nameConfig.Length == 0) {
            nameConfig = new string[] {""};
        }
        var names = nameConfig;
        for (int i = 0; i < names.Length ; ++) {
            var name = names[i];
            if (name == "") {
                if (this.defaultInstance != null) {
                    logger.info("default instance already exists. please set name to this insntance.");
                    continue;
                }
                this.defaultInstance = instance;
            } else {
                if (instances.HasKey(name)) {
                    logger.info("instance named '" + clientName + "' already exists. please set other name to this instance.");
                    continue;
                }
                this.instances[name] = instance;
            }
        }
    }

    public void Remove(T instance) {
        var removeList = default(List<T>);
        foreach (var pair in this.instances) {
            if (pair.Value == instance) {
                if (removeList == null) {
                    removeList = new List<T>();
                }
                removeList.Add(pair.Key);
            }
        }
        if (removeList != null) {
            for (int i = 0; i < removeList.Count; i++) {
                var key = removeList[i];
                this.instances.Remove(key);
            }
            removeList.Clear();
        }
    }
}
