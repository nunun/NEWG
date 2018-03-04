using System;
namespace Services.Protocols.WebAPI {
    public class WebAPI {
        
        [Serializable]
        public struct Test_Request {
            public int reqValue;
        }

        [Serializable]
        public struct Test_Response {
            public int resValue;
        }

        // Test
        // テストインターフェイス
        public static WebAPIClient.Request Test(int reqValue, Action<string,Test_Response> callback, string[] queries = null, string[] forms = null, string[] headers = null) {
            var client = WebAPIClient.GetClient();
            var data = new Test_Request();
            data.reqValue = reqValue;
            return client.Post<Test_Request,Test_Response>("/test", data, callback, queries, forms, headers);
        }
        
    }
}
