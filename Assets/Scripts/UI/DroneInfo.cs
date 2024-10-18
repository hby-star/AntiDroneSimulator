using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DroneInfo : MonoBehaviour
{
    public GameObject droneStatePrefab;
    public Swarm swarm;
    public List<GameObject> detectDroneInfoObjects;
    public List<GameObject> attackDroneInfoObjects;

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
                if (i < swarm.detectDrones.Count)
                {
                    droneInfoText.text = i + 1 + "-侦查无人机-搜索敌人";
                    swarm.detectDrones[i].OnDetectDroneStateChange +=
                        UpdateDetectDroneState;
                    detectDroneInfoObjects.Add(droneInfoObject);
                }
                else
                {
                    droneInfoText.text = i + 1 + "-攻击无人机-待机";
                    swarm.attackDrones[i - swarm.detectDrones.Count].OnAttackDroneStateChange +=
                        UpdateAttackDroneState;
                    attackDroneInfoObjects.Add(droneInfoObject);
                }
            }
        }
    }

    private void UpdateDetectDroneState(int droneID, DetectDrone.DetectDroneState detectDroneState)
    {
        TextMeshProUGUI droneInfoText = detectDroneInfoObjects[droneID].GetComponentInChildren<TextMeshProUGUI>();
        droneInfoText.text = droneID + 1 + "-侦查无人机-" +
                             (detectDroneState == DetectDrone.DetectDroneState.Patrol ? "搜索敌人" : "锁定敌人");
    }

    private void UpdateAttackDroneState(int droneID, AttackDrone.AttackDroneState attackDroneState)
    {
        TextMeshProUGUI droneInfoText = attackDroneInfoObjects[droneID - detectDroneInfoObjects.Count]
            .GetComponentInChildren<TextMeshProUGUI>();
        droneInfoText.text = droneID + 1 + "-攻击无人机-" +
                             (attackDroneState == AttackDrone.AttackDroneState.Idle
                                 ? "待机"
                                 : attackDroneState == AttackDrone.AttackDroneState.TrackPlayer
                                     ? "锁定敌人"
                                     : attackDroneState == AttackDrone.AttackDroneState.Patrol
                                         ? "搜索敌人"
                                         : "返回基地");
    }
}