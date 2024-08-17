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
    public RoutePlanningHelper(Drone drone)
    {
        this.drone = drone;
    }

    private const string DroneServerRoutePlanningUrl = "http://localhost:8000//drone_route_planning/";

    private Drone drone;

    public struct RoutePlanningRequest
    {
        public Vector3 DronePosition;
        public Vector4 PlayerPositionInCamera;
        public Vector3 ObstalePosition;

        public Vector2 ScreenSize;
        public float DetectObstacleDistance;
    }

    private RoutePlanningRequest routePlanningRequest;
    public Vector3 responseDirection;
    private byte[] requestImageBytes;

    public void SetRoutePlanningRequest(Vector3 dronePosition, Vector4 playerPositionInCamera,
        Vector4 obstaleInfo, Vector2 screenSize, float detectObstacleDistance)
    {
        routePlanningRequest.DronePosition = dronePosition;
        routePlanningRequest.PlayerPositionInCamera = playerPositionInCamera;
        routePlanningRequest.ObstalePosition = obstaleInfo;
        routePlanningRequest.ScreenSize = screenSize;
        routePlanningRequest.DetectObstacleDistance = detectObstacleDistance;
    }

    private string Vector3ToJsonArray(Vector3 vector)
    {
        return $"[{vector.x},{vector.y},{vector.z}]";
    }



    public IEnumerator SendRoutePlanningRequest(Camera camera)
    {
        // 将渲染的结果保存到RenderTexture
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        camera.targetTexture = renderTexture;

        // 创建一个Texture2D来保存图像数据
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        // 渲染摄像机
        camera.Render();

        // 从RenderTexture读取像素
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();

        // 重置摄像机的目标纹理和活动渲染纹理
        camera.targetTexture = null;
        RenderTexture.active = null;

        // 销毁RenderTexture
        MonoBehaviour.Destroy(renderTexture);

        // 编码图像为JPEG
        requestImageBytes = screenShot.EncodeToJPG();

        // 创建一个WWWForm并添加图像数据
        WWWForm form = new WWWForm();
        //--form 'drone_image=@"C:\\Users\\dell\\Pictures\\test.png"' \
        //--form 'drone_position="[0,0,0]"' \
        //--form 'obstacle_positions="[-5,0,0]"'
        form.AddBinaryData("drone_image", requestImageBytes, "screenshot.jpg", "image/jpeg");
        form.AddField("obstacle_positions", Vector3ToJsonArray(drone.GetObstaclePosition()));


        // 发送POST请求
        UnityWebRequest www = UnityWebRequest.Post(DroneServerRoutePlanningUrl, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            ProcessObjectDetectionResponse(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("ObjectDetectionRequestFail: " + www.error);
        }
    }

    private void ProcessObjectDetectionResponse(string downloadHandlerText)
    {
        // action = dqn_select_action(state)
        //
        // return JsonResponse({'Direction': drone_move_directions[action].tolist()})
        var json = JObject.Parse(downloadHandlerText);
        var direction = json["Direction"];
        var noPerson = json["No person detected"];

        if (direction != null)
        {
            Debug.Log("Found player, set direction : [" + direction[0] + " " + direction[1] + " " + direction[2] + "]");
            responseDirection = new Vector3((float)direction[0], (float)direction[1], (float)direction[2]);
            drone.FoundPlayer = true;
        }
        else if (noPerson != null)
        {
            Debug.Log("No person detected");
            responseDirection = new Vector3(0, 0, 0);
            drone.FoundPlayer = false;
        }
    }
}