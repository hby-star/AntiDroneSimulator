using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class RoutePlanningHelper
{
    private const string DroneServerRoutePlanningUrl = "http://localhost:8000//drone_route_planning/";

    public struct RoutePlanningRequest
    {
        public Vector3 DronePosition;
        public Vector4 PlayerPositionInCamera;
        public Vector3 ObstalePosition;

        public Vector2 ScreenSize;
        public float DetectObstacleDistance;
    }

    private RoutePlanningRequest routePlanningRequest;
    private Vector3 responseDirection;

    public void SetRoutePlanningRequest(Vector3 dronePosition, Vector4 playerPositionInCamera,
        Vector4 obstaleInfo, Vector2 screenSize, float detectObstacleDistance)
    {
        routePlanningRequest.DronePosition = dronePosition;
        routePlanningRequest.PlayerPositionInCamera = playerPositionInCamera;
        routePlanningRequest.ObstalePosition = obstaleInfo;
        routePlanningRequest.ScreenSize = screenSize;
        routePlanningRequest.DetectObstacleDistance = detectObstacleDistance;
    }

    public IEnumerator SendRoutePlanningRequest()
    {
        // 将请求数据序列化为JSON
        string routePlanningRequestJson = JsonConvert.SerializeObject(routePlanningRequest);

        // 创建一个UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(DroneServerRoutePlanningUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(routePlanningRequestJson);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 发送请求
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            responseDirection = JsonConvert.DeserializeObject<Vector3>(responseJson);
        }
        else
        {
            Debug.Log("RoutePlanningRequestFail: " + request.error);
        }
    }
}