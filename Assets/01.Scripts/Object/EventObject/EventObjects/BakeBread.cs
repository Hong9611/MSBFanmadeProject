using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BakeBread : MonoBehaviour, IEventObject
{
    public string breadPoolTag = "Bread";
    public Transform spawnPoint;
    public Transform basket;
    public float spawnInterval = 1f;
    public int maxBread = 8;

    private int currentBreadCount = 0;
    private List<GameObject> breads = new List<GameObject>();
    private Coroutine currentMoveToPlayerCoroutine;

    void Start()
    {
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager Null.");
            enabled = false;
            return;
        }
        StartCoroutine(MakeBread());
    }

    IEnumerator MakeBread()
    {
        while (true)
        {
            if (currentBreadCount < maxBread)
            {
                Vector3 currentSpawnPos = new Vector3(-1, -1, -1);
                if (spawnPoint != null)
                {
                    currentSpawnPos = spawnPoint.localPosition;
                }
                else
                {
                    Debug.LogError("spawnPoint Null");
                }

                GameObject newBread = ObjectPoolManager.Instance.SpawnFromPool(breadPoolTag, currentSpawnPos, spawnPoint.rotation);

                if (newBread != null)
                {
                    newBread.transform.position = spawnPoint.position;
                    newBread.transform.rotation = spawnPoint.rotation;

                    Rigidbody rb = newBread.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.position = spawnPoint.position;
                        rb.rotation = spawnPoint.rotation;
                    }

                    MoveBread(newBread);
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void MoveBread(GameObject breadObject)
    {
        if (breadObject != null)
        {
            breadObject.transform.position = spawnPoint.position;
        }

        Vector3 startPos = spawnPoint.position;
        float dropY = spawnPoint.position.y;

        Vector3 targetXZPos = new Vector3(basket.position.x - 0.8f, dropY, basket.position.z);
        Vector3 direction = (targetXZPos - startPos).normalized;
        Vector3 forceVector = direction * 2;

        Rigidbody rb = breadObject.GetComponent<Rigidbody>();
        if (rb == null || basket.position == null) return;

        rb.AddForce(forceVector, ForceMode.Impulse);

        breads.Add(breadObject);
        currentBreadCount++;
    }

    public void ReturnBreadToPool(GameObject breadObject)
    {
        if (breadObject == null) return;

        Rigidbody rb = breadObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        ObjectPoolManager.Instance.ReturnToPool(breadPoolTag, breadObject);
        breads.Remove(breadObject);
        currentBreadCount = Mathf.Max(0, currentBreadCount - 1);
    }

    public void OnUnitEnter(Collider Unit)
    {
        if (Unit.CompareTag("Player"))
        {
            Player player = Unit.GetComponent<Player>();

            if (currentMoveToPlayerCoroutine != null)
            {
                StopCoroutine(currentMoveToPlayerCoroutine);
            }
            currentMoveToPlayerCoroutine = StartCoroutine(MoveBreadToPlayerStackContinuously(player));
            GameManager.Instance.ChangeTarget(Arrows.breadDisplay);
        }
    }

    private IEnumerator MoveBreadToPlayerStackContinuously(Player player)
    {
        while (player.curStack < player.maxStack)
        {
            if (breads.Count > 0)
            {
                yield return MoveSingleBreadToPlayerStack(player);
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                yield return null;
            }
        }
        currentMoveToPlayerCoroutine = null;
    }

    private IEnumerator MoveSingleBreadToPlayerStack(Player player)
    {
        if (breads.Count == 0) yield break;

        var breadObject = breads[0];

        Rigidbody rb = breadObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Vector3 startPosition = breadObject.transform.position;
        Vector3 targetPosition = player.stack.position + Vector3.up * (player.curStack * player.stackHeightOffset);

        float duration = 0.2f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            breadObject.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        breadObject.transform.position = targetPosition;

        if (rb != null) rb.isKinematic = false;

        ObjectPoolManager.Instance.ReturnToPool(breadPoolTag, breadObject);
        player.AddBreadToStack();
        breads.RemoveAt(0);
        currentBreadCount--;
    }

    public void OnUnitExit(Collider Unit)
    {
        if (Unit.CompareTag("Player"))
        {
            if (currentMoveToPlayerCoroutine != null)
            {
                StopCoroutine(currentMoveToPlayerCoroutine);
                currentMoveToPlayerCoroutine = null;
            }
        }
    }
}