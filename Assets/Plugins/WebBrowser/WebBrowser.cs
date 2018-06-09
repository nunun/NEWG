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
    static Uri                       locationURI     = null;
    static Dictionary<string,string> locationQueries = null;

    //---------------------------------------------------------------------- DLL インポート (GameWebGLPlugin.jslib)
    [DllImport("__Internal")]
    private static extern string _GetLocationURL();

    //---------------------------------------------------------------------- ラッパーメソッド
    // ホスト名を取得
    public static string GetLocationHostName() {
        return GetLocationURI().Host;
    }

    // クエリパラメータの辞書で取得
    public static string GetLocationQuery(string key) {
        var queries = GetLocationQueries();
        if (!queries.ContainsKey(key)) {
            return null;
        }
        return queries[key];
    }

    //---------------------------------------------------------------------- 内部処理
    // URL を取得
    static string GetLocationURL() {
        #if UNITY_EDITOR
        return "http://localhost/";
        #else
        var locationUrl = _GetLocationURL();
        if (locationUrl.StartsWith("http://") || locationUrl.StartsWith("https://")) {
            return locationUrl;
        }
        return "http://localhost/";
        #endif
    }

    // URI を取得
    static Uri GetLocationURI() {
        return locationURI ?? (locationURI = new Uri(GetLocationURL()));
    }

    // クエリ一覧を取得
    static Dictionary<string,string> GetLocationQueries() {
        if (locationQueries == null) {
            locationQueries = new Dictionary<string,string>();
            var u = GetLocationURI();
            var q = u.Query;
            if (q.StartsWith("?")) {
                q = q.Substring(1);
                foreach (var i in q.Split('&')) {
                    var kv = i.Split('=');
                    if (kv.Length == 2) {
                        locationQueries.Add(kv[0], kv[1]);
                    }
                }
            }
        }
        return locationQueries;
    }
}
