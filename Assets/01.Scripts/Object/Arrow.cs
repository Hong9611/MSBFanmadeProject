using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float startY = 4f;       
    public float endY = 3.5f;       
    public float duration = 1f;     

    private Coroutine _movementCoroutine;

    void OnEnable()
    {
        _movementCoroutine = StartCoroutine(MoveYCoroutine());
    }

    IEnumerator MoveYCoroutine()
    {
        while (true)
        {
            float elapsed = 0f;
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = new Vector3(currentPosition.x, endY, currentPosition.z);

            while (elapsed < duration)
            {
                currentPosition.y = Mathf.Lerp(startY, endY, elapsed / duration);
                transform.position = currentPosition;

                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;

            elapsed = 0f;
            currentPosition = transform.position;
            targetPosition = new Vector3(currentPosition.x, startY, currentPosition.z);

            while (elapsed < duration)
            {
                currentPosition.y = Mathf.Lerp(endY, startY, elapsed / duration);
                transform.position = currentPosition;

                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;
        }
    }

    void OnDisable()
    {
        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
        }
    }
}
