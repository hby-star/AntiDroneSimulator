using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ObjectDetectionHelper
{
    private const string DroneServerObjectDetectionUrl = "http://localhost:8000//drone_object_detection/";

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
}