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
        }

        [Serializable]
        public struct SignupResponse {
        }

        // Signup
        // サインアップAPI
        public static WebAPIClient.Request Signup(Action<string,SignupResponse> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new SignupRequest();
            return client.Post<SignupRequest,SignupResponse>("/signup", data, callback, queries, forms, headers);
        }
        [Serializable]
        public struct SigninRequest {
            public string signinToken; // サインイントークン
        }

        [Serializable]
        public struct SigninResponse {
        }

        // Signin
        // サインインAPI
        public static WebAPIClient.Request Signin(string signinToken, Action<string,SigninResponse> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new SigninRequest();
            data.signinToken = signinToken; // サインイントークン
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
        // ユニットテスト用
        public static WebAPIClient.Request Test(int reqValue, Action<string,TestResponse> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new TestRequest();
            data.reqValue = reqValue; // リクエストの値
            return client.Post<TestRequest,TestResponse>("/test", data, callback, queries, forms, headers);
        }
    }
}
