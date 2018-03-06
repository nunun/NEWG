using System;
using System.Collections;
using System.Collections.Generic;
using Services.Protocols.Models;
using Services.Protocols.Consts;
using UnityEngine;
namespace Services.Protocols {
    public class WebAPI {
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
