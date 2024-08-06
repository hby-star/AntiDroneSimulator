using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        TrainingDataPath += this.drone.name + "_training_data.json";
    }

    private const string DroneServerObjectDetectionUrl = "http://localhost:8000//drone_object_detection/";
    public string TrainingDataPath = "Assets/Scripts/Drone/HandleReuqestAndResponse/";
    private Drone drone;

    public Vector3[] Directions = new Vector3[]
    {
        // horizontal
        new (-2,0,4),
        new (-1,0,4),
        new (0,0,4),
        new (1,0,4),
        new (2,0,4),

        // up & horizontal
        new (-2,2,4),
        new (-1,2,4),
        new (0,2,4),
        new (1,2,4),
        new (2,2,4),

        // down & horizontal
        new (-2,-2,4),
        new (-1,-2,4),
        new (0,-2,4),
        new (1,-2,4),
        new (2,-2,4),
    };

    public Vector3[] RandomDirections = new Vector3[]
    {
        new (0,0,1),
        new (0,0,-1),
        new (0,1,0),
        new (0,-1,0),
        new (1,0,0),
        new (-1,0,0),

        new (0,1,1),
        new (0,1,-1),
        new (0,-1,1),
        new (0,-1,-1),
        new (1,0,1),
        new (1,0,-1),
        new (-1,0,1),
        new (-1,0,-1),
        new (1,1,0),
        new (1,-1,0),
        new (-1,1,0),
        new (-1,-1,0),
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
        form.AddBinaryData("drone_image", requestImageBytes, "screenshot.jpg", "image/jpeg");

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
        form.AddBinaryData("drone_image", requestImageBytes, "screenshot.jpg", "image/jpeg");

        // 发送POST请求
        UnityWebRequest www = UnityWebRequest.Post(DroneServerObjectDetectionUrl, form);
        www.SendWebRequest();

        while (!www.isDone)
        {
            // 等待请求完成
        }

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
                //detectedObjects.Add(detectedObject);

                if (detectedObject.Name == "person")
                {
                    trainingData.PlayerPositionInCamera = new Vector4(detectedObject.X1, detectedObject.Y1,
                        detectedObject.X2, detectedObject.Y2);
                    SaveTrainData(TrainingDataPath);
                    return true;
                }
            }

            // 未检测到人时，无人机随机移动
            // 无人机随机移动
            Vector3 direction = RandomDirections[new Random().Next(RandomDirections.Length)];
            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);
            if (horizontalDirection != Vector3.zero)
            {
                drone.transform.forward = horizontalDirection.normalized;
            }
            drone.transform.position += direction;
            //Debug.Log("No person detected, drone moves randomly.");
        }

        return false;
    }


    public void SaveTrainData(string path)
    {
        try
        {
            // 以json格式保存训练数据
            trainingData.Direction = Directions[new Random().Next(Directions.Length)];
            trainingData.Reward = ComputeReward(trainingData.DronePosition, trainingData.PlayerPositionInCamera,
                trainingData.ObstalePosition);
            trainingData.NextDronePosition = trainingData.DronePosition + trainingData.Direction;
            trainingData.Done = IsDone(trainingData.DronePosition, trainingData.PlayerPositionInCamera);

            // 配置JsonSerializerSettings来忽略自引用循环
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(trainingData, Formatting.Indented, settings);

            // 检查json内容
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("JSON content is empty or null.");
                return;
            }

            // 以追加方式写入文件
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine(json + ",");
            }
            Debug.Log($"TrainingData appended to {path}");

            // 更新无人机位置
            Vector3 horizontalDirection = new Vector3(trainingData.Direction.x, 0, trainingData.Direction.z);
            if (horizontalDirection != Vector3.zero)
            {
                drone.transform.forward = horizontalDirection.normalized;
            }
            drone.transform.position = trainingData.NextDronePosition;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save TrainingData: {ex.Message}");
        }
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
            reward -= -10;
        }
        else
        {
            reward += 1 / (1 + distance);
        }

        float distanceToObstacle = Vector3.Distance(dronePosition, obstaclePosition);
        distanceToObstacle /= trainingData.DetectObstacleDistance;
        if (distanceToObstacle < 1)
        {
            reward -= -10;
        }
        else
        {
            reward += 1 / (1 + distanceToObstacle);
        }

        if(drone.CanAttackPlayer())
        {
            reward += 30;
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

        if(drone.CanAttackPlayer())
        {
            return true;
        }

        return false;
    }
}