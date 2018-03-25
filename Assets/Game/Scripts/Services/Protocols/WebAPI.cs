using System;
using System.Collections;
using System.Collections.Generic;
using Services.Protocols.Models;
using Services.Protocols.Consts;
using UnityEngine;
namespace Services.Protocols {
    public class WebAPI {
        [Serializable]
        public struct Signup_Request {
            public string name; // プレイヤー名
        }

        [Serializable]
        public struct Signup_Response {
            public Player player; // プレイヤー情報
            public SessionData session_data; // セッションデータ
            public LoginData login_data; // ログインデータ
        }

        // Signup
        // サインアップAPI
        public static WebAPIClient.Request Signup(string name, Action<string,Signup_Response> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new Signup_Request();
            data.name = name; // プレイヤー名
            return client.Post<Signup_Request,Signup_Response>("/login", data, callback, queries, forms, headers);
        }
        [Serializable]
        public struct Signin_Request {
            public string login_token; // ログイントークン
        }

        [Serializable]
        public struct Signin_Response {
            public Player player; // プレイヤー情報
            public SessionData session_data; // セッションデータ
        }

        // Signin
        // ログインAPI
        public static WebAPIClient.Request Signin(string login_token, Action<string,Signin_Response> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new Signin_Request();
            data.login_token = login_token; // ログイントークン
            return client.Post<Signin_Request,Signin_Response>("/login", data, callback, queries, forms, headers);
        }
        [Serializable]
        public struct Test_Request {
            public int reqValue; // リクエストの値
        }

        [Serializable]
        public struct Test_Response {
            public int resValue; // レスポンスの値
        }

        // Test
        // テストインターフェイス
        public static WebAPIClient.Request Test(int reqValue, Action<string,Test_Response> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new Test_Request();
            data.reqValue = reqValue; // リクエストの値
            return client.Post<Test_Request,Test_Response>("/test", data, callback, queries, forms, headers);
        }
    }
}
