using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DroneInfo : MonoBehaviour
{
    public GameObject droneStatePrefab;
    public Swarm swarm;
    public List<GameObject> droneInfoObjects;

    void Start()
    {
        if (Display.displays.Length > 1)
        {
            float divide = Mathf.Ceil(Mathf.Sqrt(swarm.droneCount + 1));
            float droneViewWidth = Display.displays[1].systemWidth / divide;
            float droneViewHeight = Display.displays[1].systemHeight / divide;


            for (int i = 0; i < swarm.droneCount; i++)
            {
                int row = i / (int)divide;
                int col = i % (int)divide;
                Rect rect = new Rect(col * droneViewWidth,
                    Display.displays[1].systemHeight - (row + 1) * droneViewHeight,
                    droneViewWidth, droneViewHeight);

                GameObject droneInfoObject = Instantiate(droneStatePrefab, Vector3.zero, Quaternion.identity);
                droneInfoObject.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(rect.x + 0.5f * rect.width, rect.y + 0.5f * rect.height);
                droneInfoObject.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(rect.width, rect.height);
                droneInfoObject.transform.SetParent(transform);
                TextMeshProUGUI droneInfoText = droneInfoObject.GetComponentInChildren<TextMeshProUGUI>();
                droneInfoText.text = i + 1 + "-" + (i < swarm.detectDrones.Count ? "侦查" : "攻击") + "无人机";
                droneInfoObjects.Add(droneInfoObject);
            }
        }
    }
}