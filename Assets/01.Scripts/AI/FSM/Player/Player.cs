using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public Animator animator { get; private set; }
    public PlayerController input { get; private set; }
    public PlayerStateMachine stateMachine { get; private set; }

    public Transform stack;
    public GameObject maxPannel;

    public int curStack = 0;
    public int maxStack = 8;
    public float stackHeightOffset = 0.3f;

    private List<GameObject> stackedBreads = new List<GameObject>();
    private string breadPoolTag = "Stack";

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        input = GetComponentInChildren<PlayerController>();
        stateMachine = new PlayerStateMachine(this);

        maxPannel.SetActive(false);

        if (animator == null)
        {
            Debug.LogError("Player Animator Null.");
            enabled = false;
        }
    }
    void Start()
    {
        stateMachine.ChangeState(stateMachine.IdleState);
    }

    private void Update()
    {
        input.ReadInput();
        stateMachine.Update();
    }

    public void AddBreadToStack()
    {
        maxPannel.SetActive(false);
        if (curStack >= maxStack)
        {
            Debug.Log("Max");
            return;
        }

        GameObject newBread = ObjectPoolManager.Instance.SpawnFromPool(breadPoolTag, Vector3.zero, Quaternion.Euler(0, 90, 0));

        if (newBread == null)
        {
            Debug.LogWarning("Failed to spawn bread from pool.");
            return;
        }

        SoundManager.Instance.PlaySFXOneShot("GetItem");

        newBread.transform.SetParent(stack);
        newBread.transform.localPosition = Vector3.up * (curStack * stackHeightOffset);
        newBread.transform.localRotation = Quaternion.Euler(0, 90, 0);

        stackedBreads.Add(newBread);
        curStack++;
        if (curStack >= maxStack)
        {
            maxPannel.SetActive(true);
        }
    }

    public void RemoveBreadFromStack()
    {
        if (curStack > 0)
        {
            SoundManager.Instance.PlaySFXOneShot("PutItem");
            GameObject lastBread = stackedBreads[curStack - 1];
            stackedBreads.RemoveAt(curStack - 1);

            ObjectPoolManager.Instance.ReturnToPool(breadPoolTag, lastBread);
            curStack--;
            if (curStack < maxStack)
                maxPannel.SetActive(false);
        }
        else
        {
            Debug.Log("Stack Empty");
        }
    }
}