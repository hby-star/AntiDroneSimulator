using UnityEngine;

[AddComponentMenu("Control Script/Mouse Look")]
public class MouseLook : MonoBehaviour {
    public enum RotationAxes {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }
    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;

    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;

    public Transform target;

    private float verticalRot = 0;

    void Start() {
        Rigidbody body = target != null ? target.GetComponent<Rigidbody>() : GetComponent<Rigidbody>();
        if (body != null) {
            body.freezeRotation = true;
        }

    }

    void Update() {
        if (axes == RotationAxes.MouseX) {
            float rotationY = Input.GetAxis("Mouse X") * sensitivityHor;
            target.Rotate(0, rotationY, 0);
        }
        else if (axes == RotationAxes.MouseY) {
            verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVert;
            verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);
            target.localEulerAngles = new Vector3(verticalRot, target.localEulerAngles.y, 0);
        }
        else {
            verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVert;
            verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);

            float delta = Input.GetAxis("Mouse X") * sensitivityHor;
            float horizontalRot = target.localEulerAngles.y + delta;

            target.localEulerAngles = new Vector3(verticalRot, horizontalRot, 0);
        }
    }
}