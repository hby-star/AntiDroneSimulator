using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayShooter : MonoBehaviour
{
    [SerializeField] AudioSource soundSource;
    [SerializeField] AudioClip fireSound;
    [SerializeField] GameObject bulletImpact;

    private Camera _camera;

    void Start()
    {
        _camera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        int crosshairSize = 12;
        int lineLength = 5;
        int lineWidth = 2;
        float posX = _camera.pixelWidth / 2 - crosshairSize / 4;
        float posY = _camera.pixelHeight / 2 - crosshairSize / 2;

        // Create a 1x1 texture for lines
        Texture2D lineTexture = new Texture2D(1, 1);
        lineTexture.SetPixel(0, 0, Color.white);
        lineTexture.Apply();

        // Horizontal line
        GUI.DrawTexture(
            new Rect(_camera.pixelWidth / 2 - lineLength / 2, _camera.pixelHeight / 2 - lineWidth / 2, lineLength,
                lineWidth), lineTexture);
        // Vertical line
        GUI.DrawTexture(
            new Rect(_camera.pixelWidth / 2 - lineWidth / 2, _camera.pixelHeight / 2 - lineLength / 2, lineWidth,
                lineLength), lineTexture);
        // Left line
        GUI.DrawTexture(
            new Rect(_camera.pixelWidth / 2 - lineLength - crosshairSize / 2, _camera.pixelHeight / 2 - lineWidth / 2,
                lineLength, lineWidth), lineTexture);
        // Right line
        GUI.DrawTexture(
            new Rect(_camera.pixelWidth / 2 + crosshairSize / 2, _camera.pixelHeight / 2 - lineWidth / 2, lineLength,
                lineWidth), lineTexture);
        // Top line
        GUI.DrawTexture(
            new Rect(_camera.pixelWidth / 2 - lineWidth / 2, _camera.pixelHeight / 2 - lineLength - crosshairSize / 2,
                lineWidth, lineLength), lineTexture);
        // Bottom line
        GUI.DrawTexture(
            new Rect(_camera.pixelWidth / 2 - lineWidth / 2, _camera.pixelHeight / 2 + crosshairSize / 2, lineWidth,
                lineLength), lineTexture);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            soundSource.PlayOneShot(fireSound);
            Vector3 point = new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0);
            Ray ray = _camera.ScreenPointToRay(point);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.transform.gameObject;
                ReactiveTarget target = hitObject.GetComponent<ReactiveTarget>();
                if (target != null)
                {
                    target.ReactToHit();
                }
                else
                {
                    StartCoroutine(BulletImpact(hit.point, hit.normal));
                }
            }
        }
    }

    private IEnumerator BulletImpact(Vector3 pos, Vector3 normal)
    {
        // Calculate the rotation so that the prefab's Y-axis points in the direction of the normal
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

        // Instantiate the bullet impact prefab with the adjusted rotation
        GameObject impactEffect = Instantiate(bulletImpact, pos, rotation);

        // Optional: Adjust if the effect should disappear after some time
        yield return new WaitForSeconds(1);

        Destroy(impactEffect);
    }
}