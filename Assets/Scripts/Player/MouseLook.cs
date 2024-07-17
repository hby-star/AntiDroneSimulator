using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float sensitivityVert = 3.0f;

    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;


    private float _verticalRot = 0;

    void Start()
    {
    }

    void Update()
    {
        _verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVert;
        _verticalRot = Mathf.Clamp(_verticalRot, minimumVert, maximumVert);

        float horizontalRot = transform.localEulerAngles.y;

        transform.localEulerAngles = new Vector3(_verticalRot, horizontalRot, 0);
    }
}