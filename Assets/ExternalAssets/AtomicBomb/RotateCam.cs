using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCam : MonoBehaviour
{

    public GameObject targetObj;
    Vector3 targetPos;

    float rX;
    float rY;

    float moveSpeed = 700f;

    Vector3 defaultPosition;

    void Start()
    {
        targetPos = targetObj.transform.position;
        defaultPosition = transform.position;
    }

    void Update()
    {
        transform.position += targetObj.transform.position - targetPos;
        targetPos = targetObj.transform.position;

        float rX = Input.GetAxis("Mouse X");
        float rY = Input.GetAxis("Mouse Y");

        if (Input.GetMouseButton(0))
        {
            transform.RotateAround(targetPos, Vector3.up, rX * Time.deltaTime * moveSpeed);
            transform.RotateAround(targetPos, transform.right, rY * Time.deltaTime * moveSpeed);
        }

        if (Input.GetMouseButton(1))
        {
            transform.Translate(Vector3.up * Time.deltaTime * rY, Space.Self);
            transform.Translate(Vector3.right * Time.deltaTime * rX, Space.Self);
        }

        if(Input.GetKeyDown(KeyCode.Space)){
            transform.position = defaultPosition;
            transform.LookAt(targetPos);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll;
    }
}
