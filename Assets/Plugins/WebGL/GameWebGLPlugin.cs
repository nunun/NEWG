using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

// ゲーム用 WebGL プラグイン
// URL のクエリパラメータから情報を取得する、等の
// WebPlayer 用のプラグイン。
public class GameWebGLPlugin {
    //-------------------------------------------------------------------------- DLL インポート (GameWebGLPlugin.jslib)
    [DllImport("__Internal")]
    private static extern string _GetLocationURL();

    //-------------------------------------------------------------------------- ラッパーメソッド
    // URL を取得
    public static string GetLocationURL() {
        #if UNITY_EDITOR
        return "http://localhost/?a=localhost&p=7777";
        #else
        return _GetLocationURL();
        #endif
    }

    // ホスト名を取得
    public static string GetLocationHostName() {
        var u = new Uri(GetLocationURL());
        if (u.Scheme != "http" && u.Scheme != "https") {
            return "localhost"; // NOTE 今の所全てローカルホストにする
        }
        return u.Host;
    }

    // クエリパラメータの辞書で取得
    public static Dictionary<string,string> GetLocationQueryParameters() {
        var u = new Uri(GetLocationURL());
        var q = u.Query;
        var p = new Dictionary<string,string>();
        if (q.StartsWith("?")) {
            q = q.Substring(1);
            foreach (var i in q.Split('&')) {
                var kv = i.Split('=');
                if (kv.Length == 2) {
                    p.Add(kv[0], kv[1]);
                }
            }
        }
        return p;
    }
}
