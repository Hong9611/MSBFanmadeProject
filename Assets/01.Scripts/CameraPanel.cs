using UnityEngine;

public class CameraPanel : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camUp = Camera.main.transform.up;

        transform.SetPositionAndRotation(
            transform.position,
            Quaternion.LookRotation(camForward, camUp)
        );
    }
}