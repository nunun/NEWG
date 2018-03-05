using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class SmokeTest {
    //-------------------------------------------------------------------------- WebSocketConnectorのテスト
    //[SetUp]
    //public void Init() {
    //    SceneManager.LoadScene("Main");
    //}

    [UnityTest]
    public IEnumerator WebSocketConnectorTest() {
        //yield return null;
        //yield return new WaitForSeconds (1);
        //Assert.AreEqual(true, false);
        yield return new MonoBehaviourTest<WebSocketConnectorTest>();
    }

    [UnityTest]
    public IEnumerator MindlinkConnectorTest() {
        yield return new MonoBehaviourTest<MindlinkConnectorTest>();
    }

    [UnityTest]
    public IEnumerator WebAPIClientTest() {
        yield return new MonoBehaviourTest<WebAPIClientTest>();
    }
}
