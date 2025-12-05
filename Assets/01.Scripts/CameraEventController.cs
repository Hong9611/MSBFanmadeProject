using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraEventController : MonoBehaviour
{
    public CinemachineVirtualCamera vcamPlayer;
    public CinemachineVirtualCamera[] eventCameras;

    public float holdDuration = 1.5f;
    public float blendTime = 2f;

    public void ShowEvent(int index)
    {
        StartCoroutine(EventCameraSequence(index));
    }

    private IEnumerator EventCameraSequence(int index)
    {
        eventCameras[index].Priority = 20;
        vcamPlayer.Priority = 0;

        yield return new WaitForSeconds(blendTime);


        yield return new WaitForSeconds(holdDuration);

        eventCameras[index].Priority = 0;
        vcamPlayer.Priority = 20;

        yield return new WaitForSeconds(blendTime);
    }
}