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
}
