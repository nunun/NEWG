using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

// WebGL用ウェブブラウザプラグイン
// このアプリを動かしているブラウザのロケーション URL を取得する、等。
public class WebBrowser {
    //---------------------------------------------------------------------- 変数
    // クエリパラメータのキャッシュ
    static Dictionary<string,string> locationQueryParametersCache = null;

    //---------------------------------------------------------------------- DLL インポート (GameWebGLPlugin.jslib)
    [DllImport("__Internal")]
    private static extern string _GetLocationURL();

    //---------------------------------------------------------------------- ラッパーメソッド
    // URL を取得
    public static string GetLocationURL() {
        #if UNITY_EDITOR
        return "http://localhost/";
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
        if (locationQueryParametersCache != null) {
            return locationQueryParametersCache;
        }
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
        locationQueryParametersCache = p;
        return locationQueryParametersCache;
    }
}
