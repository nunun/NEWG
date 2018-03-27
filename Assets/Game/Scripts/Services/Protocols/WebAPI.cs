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
            public string player_name; // プレイヤー名
        }

        [Serializable]
        public struct SignupResponse {
            public PlayerData player_data; // プレイヤーデータ
            public SessionData session_data; // セッションデータ
            public CredentialData credential_data; // 認証データ
        }

        // Signup
        // サインアップAPI
        public static WebAPIClient.Request Signup(string player_name, Action<string,SignupResponse> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new SignupRequest();
            data.player_name = player_name; // プレイヤー名
            return client.Post<SignupRequest,SignupResponse>("/signup", data, callback, queries, forms, headers);
        }
        [Serializable]
        public struct SigninRequest {
            public string signin_token; // サインイントークン
        }

        [Serializable]
        public struct SigninResponse {
            public PlayerData player_data; // プレイヤーデータ
            public SessionData session_data; // セッションデータ
        }

        // Signin
        // サインインAPI
        public static WebAPIClient.Request Signin(string signin_token, Action<string,SigninResponse> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new SigninRequest();
            data.signin_token = signin_token; // サインイントークン
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
