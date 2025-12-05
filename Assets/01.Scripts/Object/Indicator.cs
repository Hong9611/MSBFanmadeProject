using UnityEngine;
using System.Collections;

public class Indicator : MonoBehaviour
{
    public Transform playerTransform;

    private Transform targetTransform;
    private float distanceFromPlayer = 0.8f;
    private Coroutine _indicatorCoroutine;

    void OnEnable()
    {
        if (playerTransform == null || targetTransform == null)
        {
            Debug.LogWarning("Player Transform or Target Transform Null.");
            return;
        }

        if (_indicatorCoroutine != null)
        {
            StopCoroutine(_indicatorCoroutine);
        }
        _indicatorCoroutine = StartCoroutine(PositionAndLookAtFlatCoroutine());
    }

    void OnDisable()
    {
        if (_indicatorCoroutine != null)
        {
            StopCoroutine(_indicatorCoroutine);
        }
    }

    public void SetIndicatorTarget()
    {
        gameObject.SetActive(true);

        targetTransform = GameManager.Instance.target;
        if (targetTransform == null) { gameObject.SetActive(false);}
    }

    IEnumerator PositionAndLookAtFlatCoroutine()
    {
        while (true)
        {
            if (playerTransform == null || targetTransform == null)
            {
                Debug.LogWarning("Player Transform or Target Transform Null");
                yield break;
            }

            Vector3 directionToTargetRaw = targetTransform.position - playerTransform.position;
            Vector3 directionToTargetXZ = new Vector3(directionToTargetRaw.x, 0, directionToTargetRaw.z).normalized;

            Vector3 newPosition = playerTransform.position + directionToTargetXZ * distanceFromPlayer;
            newPosition.y = playerTransform.position.y + 0.1f;
            transform.position = newPosition;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ, Vector3.up);

            transform.rotation = targetRotation * Quaternion.Euler(90, 0, 0);

            yield return null;
        }
    }
}