﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if STANDALONE_MODE
using Services.Protocols;
using Services.Protocols.Consts;
using Services.Protocols.Models;
#endif

// スタンドアローンシミュレータ設定
[CreateAssetMenu(fileName = "StandaloneSimulatorSettings", menuName = "ScriptableObject/StandaloneSimulatorSettings", order = 1001)]
public partial class StandaloneSimulatorSettings : ScriptableObject {
    // NOTE
    // パーシャルクラスを参照
}


////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
public partial class StandaloneSimulatorSettings {
    //-------------------------------------------------------------------------- 変数
    public string playerName = "StandaloneSimulatedPlayer"; // プレイヤー名

    // プレイヤー名の取得
    public static string PlayerName { get { return instance.playerName; }}
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
#if STANDALONE_MODE

// WebAPI の処理
public partial class StandaloneSimulatorSettings {
    //-------------------------------------------------------------------------- 定義
    static readonly float DEBUG_DELAY = 0.5f; // デバッグディレイ

    //-------------------------------------------------------------------------- 変数
    WebAPIClient.Request debugRequest = null; // デバッグ中のリクエスト
    float                debugDelay   = 0.0f; // デバッグディレイ

    // スタンドアローンモードかどうか (常に true)
    public static bool IsStandaloneMode { get { return true; }}

    //-------------------------------------------------------------------------- WebAPI エミュレーション
    public static bool SimulateWebAPI(WebAPIClient.Request request, float deltaTime) {
        if (instance.debugRequest == null) {
            Debug.LogFormat("StandaloneSimulatorSettings: WebAPI リクエストを処理 ({0})", request.ToString());
            instance.debugRequest = request;
            instance.debugDelay   = DEBUG_DELAY;
        }
        if (instance.debugDelay > 0.0f) {//WebAPIっぽい待ちディレイをつけておく
            instance.debugDelay -= deltaTime;
            return true;
        }
        instance.SimulateWebAPIRequest(request);
        instance.debugRequest = null;
        return false;
    }

    //-------------------------------------------------------------------------- WebAPI エミュレーションの処理
    void SimulateWebAPIRequest(WebAPIClient.Request request) {
        switch (request.APIPath) {
        case "/signup"://サインアップ
            {
                //var req = JsonUtility.FromJson<WebAPI.SignupRequest>(request.Parameters.GetText());

                var playerData = new PlayerData();
                playerData.playerId   = "(dummy playerId)";
                playerData.playerName = playerName;

                var sessionData = new SessionData();
                sessionData.sessionToken = "(dummy sessionToken)";

                var credentialData = new CredentialData();
                credentialData.signinToken = "(dummy signinToken)";

                var playerDataJson     = string.Format("\"playerData\":{{\"active\":true,\"data\":{0}}}",     JsonUtility.ToJson(playerData));
                var sessionDataJson    = string.Format("\"sessionData\":{{\"active\":true,\"data\":{0}}}",    JsonUtility.ToJson(sessionData));
                var credentialDataJson = string.Format("\"credentialData\":{{\"active\":true,\"data\":{0}}}", JsonUtility.ToJson(credentialData));
                var response = string.Format("{{\"activeData\":{{{0},{1},{2}}}}}", playerDataJson, sessionDataJson, credentialDataJson);
                request.SetResponse(null, response);
            }
            break;
        case "/signin"://サインイン
            {
                //var req = JsonUtility.FromJson<WebAPI.SignupRequest>(request.Parameters.GetText());

                var playerData = new PlayerData();
                playerData.playerId   = "(dummy playerId)";
                playerData.playerName = playerName;

                var sessionData = new SessionData();
                sessionData.sessionToken = "(dummy sessionToken)";

                var playerDataJson  = string.Format("\"playerData\":{{\"active\":true,\"data\":{0}}}",  JsonUtility.ToJson(playerData));
                var sessionDataJson = string.Format("\"sessionData\":{{\"active\":true,\"data\":{0}}}", JsonUtility.ToJson(sessionData));
                var response = string.Format("{{\"activeData\":{{{0},{1}}}}}", playerDataJson, sessionDataJson);
                request.SetResponse(null, response);
            }
            break;
        case "/matching"://マッチング
            {
                //var req = JsonUtility.FromJson<WebAPI.SignupRequest>(request.Parameters.GetText());

                var matchingResponse = new WebAPI.MatchingResponse();
                matchingResponse.matchingServerUrl = "ws//localhost:7755?matchingId=dummy_token";

                var matchingResponseJson = JsonUtility.ToJson(matchingResponse);
                var response = string.Format("{0}", matchingResponseJson);
                request.SetResponse(null, response);
            }
            break;
        default:
            Debug.LogErrorFormat("スタンドアローンデバッグで処理できない API パス ({0})", request.APIPath);
            break;
        }
    }
}
#endif
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// StandaloneSimulatorSettings 実装
public partial class StandaloneSimulatorSettings {
    //-------------------------------------------------------------------------- 変数
    // インスタンス
    static StandaloneSimulatorSettings instance = null;

    //-------------------------------------------------------------------------- 実装 (MonoBehaviour)
    void OnEnable() {
        if (instance != null) {
            return;
        }
        instance = this;
    }

    void OnDisable() {
        if (instance != this) {
            return;
        }
        instance = null;
    }
}