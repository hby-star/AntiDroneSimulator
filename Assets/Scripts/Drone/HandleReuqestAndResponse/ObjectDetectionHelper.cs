using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Random = System.Random;

public class ObjectDetectionHelper
{
    public ObjectDetectionHelper(Drone drone)
    {
        this.drone = drone;
    }

    private const string DroneServerObjectDetectionUrl = "http://localhost:8000//drone_object_detection/";
    public const string TrainingDataPath = "./TrainingData.json";
    private Drone drone;

    public Vector3[] Directions = new Vector3[]
    {
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),

        new Vector3(1, 1, 1),
        new Vector3(-1, -1, -1),
        new Vector3(1, -1, 1),
        new Vector3(-1, 1, -1),
        new Vector3(1, 1, -1),
        new Vector3(-1, -1, 1),
        new Vector3(1, -1, -1),
        new Vector3(-1, 1, 1),
    };

    public struct RoutePlanningTrainingData
    {
        // environment
        public Vector2 ScreenSize;
        public float DetectObstacleDistance;

        // state
        public Vector3 DronePosition;
        public Vector4 PlayerPositionInCamera;
        public Vector3 ObstalePosition;

        // action
        public Vector3 Direction;

        // reward
        public float Reward;

        // next state
        public Vector3 NextDronePosition;

        // done
        public bool Done;
    }

    private struct DetectedObject
    {
        public string Name;
        public float Confidence;
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;
    }

    private byte[] requestImageBytes;
    private Time responseTime;
    private List<DetectedObject> detectedObjects = new List<DetectedObject>();
    private RoutePlanningTrainingData trainingData;

    public IEnumerator SendObjectDetectionRequest(Camera camera)
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
        form.AddBinaryData("image", requestImageBytes, "screenshot.jpg", "image/jpeg");

        // 发送POST请求
        UnityWebRequest www = UnityWebRequest.Post(DroneServerObjectDetectionUrl, form);
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

    public void ProcessObjectDetectionResponse(string jsonResponse)
    {
        var json = JObject.Parse(jsonResponse);
        var output = json["output"].ToString();
        var outputArray = JArray.Parse(output);

        detectedObjects.Clear();

        foreach (var item in outputArray)
        {
            var detectedObject = new DetectedObject
            {
                Name = item["name"].ToString(),
                Confidence = item["confidence"].ToObject<float>(),
                X1 = item["box"]["x1"].ToObject<float>(),
                Y1 = item["box"]["y1"].ToObject<float>(),
                X2 = item["box"]["x2"].ToObject<float>(),
                Y2 = item["box"]["y2"].ToObject<float>()
            };
            detectedObjects.Add(detectedObject);
        }

        // 输出检测到的对象信息
        // foreach (var obj in detectedObjects)
        // {
        //     Debug.Log($"Detected object: {obj.Name}, Confidence: {obj.Confidence}, Box: ({obj.X1}, {obj.Y1}, {obj.X2}, {obj.Y2})");
        // }
    }

    public bool GetTrainingDataSendRequest(Camera camera)
    {
        // 获取训练数据
        trainingData.ScreenSize = new Vector2(Screen.width, Screen.height);
        trainingData.DetectObstacleDistance = drone.detectObstacleDistance;
        trainingData.DronePosition = drone.transform.position;
        trainingData.ObstalePosition = drone.GetObstaclePosition();

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
        form.AddBinaryData("image", requestImageBytes, "screenshot.jpg", "image/jpeg");

        // 发送POST请求
        UnityWebRequest www = UnityWebRequest.Post(DroneServerObjectDetectionUrl, form);
        www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            var json = JObject.Parse(www.downloadHandler.text);
            var output = json["output"].ToString();
            var outputArray = JArray.Parse(output);

            foreach (var item in outputArray)
            {
                var detectedObject = new DetectedObject
                {
                    Name = item["name"].ToString(),
                    Confidence = item["confidence"].ToObject<float>(),
                    X1 = item["box"]["x1"].ToObject<float>(),
                    Y1 = item["box"]["y1"].ToObject<float>(),
                    X2 = item["box"]["x2"].ToObject<float>(),
                    Y2 = item["box"]["y2"].ToObject<float>()
                };
                detectedObjects.Add(detectedObject);

                if (detectedObject.Name == "person" && detectedObject.Confidence > 0.5)
                {
                    trainingData.PlayerPositionInCamera = new Vector4(detectedObject.X1, detectedObject.Y1,
                        detectedObject.X2, detectedObject.Y2);
                    SaveTrainData(TrainingDataPath);
                    return true;
                }
            }
        }

        return false;
    }


    public void SaveTrainData(string path)
    {
        // 以json格式保存训练数据
        trainingData.Direction = Directions[new Random().Next(Directions.Length)];
        trainingData.Reward = ComputeReward(trainingData.DronePosition, trainingData.PlayerPositionInCamera,
            trainingData.ObstalePosition);
        trainingData.NextDronePosition = trainingData.DronePosition + trainingData.Direction;
        trainingData.Done = IsDone(trainingData.DronePosition, trainingData.PlayerPositionInCamera);

        string json = JsonConvert.SerializeObject(trainingData, Formatting.Indented);
        System.IO.File.WriteAllText(path, json);

        // 更新Drone的位置
        Vector3 horizontalDirection = new Vector3(trainingData.Direction.x, 0, trainingData.Direction.z);
        drone.transform.forward = horizontalDirection.normalized;
        drone.transform.position = trainingData.NextDronePosition;
    }

    float ComputeReward(Vector3 dronePosition, Vector4 playerPositionInCamera, Vector3 obstaclePosition)
    {
        float reward = 0;
        Vector2 screenCenter = new Vector2(trainingData.ScreenSize.x / 2, trainingData.ScreenSize.y / 2);
        Vector2 playerCenter = new Vector2((playerPositionInCamera.x + playerPositionInCamera.z) / 2,
            (playerPositionInCamera.y + playerPositionInCamera.w) / 2);
        float distance = Vector2.Distance(playerCenter, screenCenter);
        distance /= Vector2.Distance(Vector2.zero, screenCenter);
        if (distance > Vector2.Distance(Vector2.zero, screenCenter))
        {
            reward = -10;
        }
        else
        {
            reward = 1 / (1 + distance);
        }

        float distanceToObstacle = Vector3.Distance(dronePosition, obstaclePosition);
        distanceToObstacle /= trainingData.DetectObstacleDistance;
        if (distanceToObstacle < 1)
        {
            reward = -10;
        }
        else
        {
            reward += 1 / (1 + distanceToObstacle);
        }

        return reward;
    }

    bool IsDone(Vector3 dronePosition, Vector4 playerPositionInCamera)
    {
        Vector2 screenCenter = new Vector2(trainingData.ScreenSize.x / 2, trainingData.ScreenSize.y / 2);
        Vector2 playerCenter = new Vector2((playerPositionInCamera.x + playerPositionInCamera.z) / 2,
            (playerPositionInCamera.y + playerPositionInCamera.w) / 2);
        float distance = Vector2.Distance(playerCenter, screenCenter);
        distance /= Vector2.Distance(Vector2.zero, screenCenter);
        if (distance > Vector2.Distance(Vector2.zero, screenCenter))
        {
            return true;
        }

        return false;
    }
}