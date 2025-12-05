using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가

public class Player : MonoBehaviour
{
    public FloatingJoystick joystick;
    public Transform pivotTransform;
    public Transform stack;
    public GameObject maxPannel;

    public float speed;
    public int curStack = 0;
    public int maxStack = 8;
    public float stackHeightOffset = 0.3f;

    private Rigidbody rb;
    private Animator anim;
    private Vector3 moveVector;
    private PlayerState currentState;

    private List<GameObject> stackedBreads = new List<GameObject>();
    private string breadPoolTag = "Stack";

    public enum PlayerState
    {
        Idle,
        Move
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        maxPannel.SetActive(false);

        if (rb == null)
        {
            Debug.LogError("Player Rigidbody Null.");
            enabled = false;
        }

        if (anim == null)
        {
            Debug.LogError("Player Animator Null.");
            enabled = false;
        }

        if (joystick == null)
        {
            Debug.LogError("Player FloatingJoystick Null.");
            enabled = false;
        }

        currentState = PlayerState.Idle;
    }

    void Update()
    {
        HandleTransitions();
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                IdleUpdate();
                break;
            case PlayerState.Move:
                MoveUpdate();
                break;
        }
    }

    private void IdleUpdate()
    {
        if (curStack > 0)
        {
            anim.SetBool("Stack", true);
            anim.SetBool("Move", false);
        }
        else
        {
            anim.SetBool("Stack", false);
            anim.SetBool("Move", false);
        }

        rb.velocity = Vector3.zero;
    }

    private void MoveUpdate()
    {
        if (curStack > 0)
        {
            anim.SetBool("Stack", true);
            anim.SetBool("Move", true);
        }
        else
        {
            anim.SetBool("Stack", false);
            anim.SetBool("Move", true);
        }

        float x = -joystick.Vertical;
        float z = joystick.Horizontal;

        moveVector = new Vector3(x, 0, z) * speed * Time.deltaTime;
        rb.MovePosition(rb.position + moveVector);

        if (moveVector.sqrMagnitude == 0)
            return;

        Quaternion dirQuat = Quaternion.LookRotation(moveVector);
        pivotTransform.localRotation = Quaternion.Slerp(pivotTransform.rotation, dirQuat, 0.3f);
    }

    private void HandleTransitions()
    {
        float x = -joystick.Vertical;
        float z = joystick.Horizontal;

        if (currentState == PlayerState.Idle)
        {
            if (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f)
                currentState = PlayerState.Move;
        }
        else if (currentState == PlayerState.Move)
        {
            if (Mathf.Abs(x) < 0.1f && Mathf.Abs(z) < 0.1f)
                currentState = PlayerState.Idle;
        }
    }

    public PlayerState GetCurrentState()
    {
        return currentState;
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