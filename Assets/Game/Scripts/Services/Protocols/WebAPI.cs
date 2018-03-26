using System;
using System.Collections;
using System.Collections.Generic;
using Services.Protocols.Models;
using Services.Protocols.Consts;
using UnityEngine;
namespace Services.Protocols {
    public class WebAPI {
        [Serializable]
        public struct SignupRequest {
            public string name; // プレイヤー名
        }

        [Serializable]
        public struct SignupResponse {
            public PlayerData player_data; // プレイヤーデータ
            public SessionData session_data; // セッションデータ
            public LoginData login_data; // ログインデータ
        }

        // Signup
        // サインアップAPI
        public static WebAPIClient.Request Signup(string name, Action<string,SignupResponse> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new SignupRequest();
            data.name = name; // プレイヤー名
            return client.Post<SignupRequest,SignupResponse>("/signup", data, callback, queries, forms, headers);
        }
        [Serializable]
        public struct SigninRequest {
            public string login_token; // ログイントークン
        }

        [Serializable]
        public struct SigninResponse {
            public PlayerData player_data; // プレイヤーで０タ
            public SessionData session_data; // セッションデータ
        }

        // Signin
        // ログインAPI
        public static WebAPIClient.Request Signin(string login_token, Action<string,SigninResponse> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new SigninRequest();
            data.login_token = login_token; // ログイントークン
            return client.Post<SigninRequest,SigninResponse>("/signin", data, callback, queries, forms, headers);
        }
        [Serializable]
        public struct TestRequest {
            public int reqValue; // リクエストの値
        }

        [Serializable]
        public struct TestResponse {
            public int resValue; // レスポンスの値
        }

        // Test
        // テストインターフェイス
        public static WebAPIClient.Request Test(int reqValue, Action<string,TestResponse> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new TestRequest();
            data.reqValue = reqValue; // リクエストの値
            return client.Post<TestRequest,TestResponse>("/test", data, callback, queries, forms, headers);
        }
    }
}
