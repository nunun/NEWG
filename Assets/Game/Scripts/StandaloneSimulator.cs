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

// スタンドアローンシミュレータ
[CreateAssetMenu(fileName = "StandaloneSimulator", menuName = "ScriptableObject/StandaloneSimulator", order = 1000)]
public partial class StandaloneSimulator : ScriptableObject {
    //-------------------------------------------------------------------------- 変数
    // プレイヤー名
    public string playerName = "StandaloneSimulator";
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
#if STANDALONE_MODE

// WebAPI の処理
public partial class StandaloneSimulator {
    //-------------------------------------------------------------------------- 変数
    static readonly float DEBUG_DELAY = 0.5f; // デバッグディレイ

    //-------------------------------------------------------------------------- 変数
    WebAPIClient.Request debugRequest = null; // デバッグ中のリクエスト
    float                debugDelay   = 0.0f; // デバッグディレイ

    //-------------------------------------------------------------------------- WebAPI エミュレーション
    public bool SimulateWebAPI(WebAPIClient.Request request, float deltaTime) {
        if (debugRequest == null) {
            Debug.LogFormat("StandaloneSimulator: WebAPI リクエストを処理 ({0})", request.ToString());
            debugRequest = request;
            debugDelay   = DEBUG_DELAY;
        }
        if (debugDelay > 0.0f) {//WebAPIっぽい待ちディレイをつけておく
            debugDelay -= deltaTime;
            return true;
        }
        SimulateWebAPIRequest(request);
        debugRequest = null;
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
