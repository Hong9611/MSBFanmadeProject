using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour, IPooledObject
{
    public int request;
    public int curRequest;
    public TMP_Text requestText;

    public GameObject requstObject;
    public GameObject calculation;
    public GameObject wantEat;
    public GameObject satisfaction;
    public GameObject balloon;
    public GameObject paperBack;

    public Transform stack;

    public Animator Anim => m_Anim;
    public NavMeshAgent Agent => m_Agent;
    public CustomerStateMachine StateMachine => m_StateMachine;

    private bool m_EndOn = false;

    private NavMeshAgent m_Agent;
    private Animator m_Anim;

    private CustomerStateMachine m_StateMachine;

    private Coroutine m_CurrentMovementCoroutine;
    private readonly List<GameObject> m_StackedBreads = new List<GameObject>();

    private void Awake()
    {
        CacheComponents();

        m_StateMachine = new CustomerStateMachine(this);
    }

    private void Update()
    {
        m_StateMachine?.Update();
    }

    private void CacheComponents()
    {
        if (m_Agent == null)
            m_Agent = GetComponent<NavMeshAgent>();

        if (m_Anim == null)
            m_Anim = GetComponent<Animator>();
    }

    public void OnObjectSpawn(string maker = null)
    {
        CacheComponents();

        m_EndOn = false;

        ResetStackVisual();
        ResetVisualFlags();

        SetRequest();

        // 스폰 시점에 초기 상태로 진입
        m_StateMachine.ChangeState(m_StateMachine.IdleState);
    }

    private void ResetStackVisual()
    {
        if (stack == null)
            return;

        for (int i = stack.childCount - 1; i >= 0; i--)
        {
            Transform child = stack.GetChild(i);
            ObjectPoolManager.Instance.ReturnToPool("Stack", child.gameObject);
        }

        m_StackedBreads.Clear();
    }

    private void ResetVisualFlags()
    {
        if (m_Anim != null)
        {
            m_Anim.SetBool("Move", false);
            m_Anim.SetBool("Stack", false);
            m_Anim.SetBool("Sit", false);
        }

        if (satisfaction != null) satisfaction.SetActive(false);
        if (calculation != null) calculation.SetActive(false);
        if (paperBack != null) paperBack.SetActive(false);
        if (wantEat != null) wantEat.SetActive(false);

        if (balloon != null) balloon.SetActive(true);
        if (requstObject != null) requstObject.SetActive(true);
    }

    private void SetRequest()
    {
        request = Random.Range(1, 4);
        curRequest = request;
        UpdateRequest();
    }

    public void UpdateRequest()
    {
        if (requestText != null)
            requestText.text = curRequest.ToString();

        var customerManager = CustomerLineManager.Instance;

        if (curRequest == 0)
        {
            var findVal = customerManager.GetKeyByValue(customerManager.CanWait, gameObject);

            if (findVal != null)
            {
                customerManager.CanWait[findVal] = null;
            }

            if (requstObject != null)
                requstObject.SetActive(false);

            if (request == 1)
            {
                if (wantEat != null) wantEat.SetActive(true);

                var eatPoint = customerManager.SecondLinePoint;
                if (customerManager.LineQueues.ContainsKey(eatPoint))
                {
                    customerManager.Enqueue(eatPoint, this);
                }
            }
            else
            {
                if (calculation != null) calculation.SetActive(true);

                var calculationPoint = customerManager.FirstLinePoint;
                if (customerManager.LineQueues.ContainsKey(calculationPoint))
                {
                    customerManager.Enqueue(calculationPoint, this);
                }
            }
        }
    }

    public void SetCustomerDestination(Vector3 target, Vector3? lookAt = null)
    {
        StopMovementCoroutine();

        if (!gameObject.activeInHierarchy) return;

        CacheComponents();
        if (m_Agent == null || !m_Agent.isOnNavMesh) return;

        m_StateMachine.RequestMove(target, lookAt);
    }

    internal void TryStartMove(Vector3 target, Vector3? lookAt)
    {
        StopMovementCoroutine();

        // endPoint 근처인지로 판정 (float 비교 오차 방지)
        if (Vector3.Distance(target, CustomerLineManager.Instance.EndPoint.position) < 0.01f)
        {
            m_EndOn = true;
        }
        else
        {
            m_EndOn = false;
        }

        if (!gameObject.activeInHierarchy) return;
        if (m_Agent == null || !m_Agent.isOnNavMesh) return;

        m_Agent.SetDestination(target);

        if (m_Anim != null)
        {
            m_CurrentMovementCoroutine = StartCoroutine(MoveAndAnimate(lookAt));
        }
    }

    internal void StopMovementCoroutine()
    {
        if (m_CurrentMovementCoroutine != null)
        {
            StopCoroutine(m_CurrentMovementCoroutine);
            m_CurrentMovementCoroutine = null;
        }

        if (m_Anim != null)
            m_Anim.SetBool("Move", false);
    }

    private IEnumerator MoveAndAnimate(Vector3? lookAt = null)
    {
        if (m_Anim != null)
            m_Anim.SetBool("Move", true);

        while (m_Agent != null && (m_Agent.pathPending || m_Agent.remainingDistance > m_Agent.stoppingDistance))
        {
            yield return null;
        }

        if (m_Anim != null)
            m_Anim.SetBool("Move", false);

        if (lookAt != null)
        {
            Vector3 directionToTarget = lookAt.Value - transform.position;

            if (directionToTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion startRot = transform.rotation;
                Quaternion targetRot = Quaternion.LookRotation(directionToTarget);

                float rotateTime = 0.25f;
                float t = 0f;

                while (t < 1f)
                {
                    t += Time.deltaTime / rotateTime;
                    transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                    yield return null;
                }

                transform.rotation = targetRot;
            }
        }

        m_CurrentMovementCoroutine = null;

        // 이동 완료 후 후처리/상태 전환
        HandleMoveCompleted();
    }

    private void HandleMoveCompleted()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (m_EndOn)
        {
            if (paperBack != null && paperBack.activeSelf)
                paperBack.SetActive(false);

            CustomerLineManager.Instance.RemoveFromAllLines(this);

            ObjectPoolManager.Instance.ReturnToPool("Customer", gameObject);
            GameManager.Instance.customerNum--;
            return;
        }

        // 도착 지점이 endPoint가 아니라면, 의도에 따라 상태 전환
        if (wantEat != null && wantEat.activeSelf)
        {
            m_StateMachine.ChangeState(m_StateMachine.EatState);
        }
        else
        {
            m_StateMachine.ChangeState(m_StateMachine.IdleState);
        }
    }

    public void AddBreadToDisplay()
    {
        if (curRequest <= 0)
        {
            Debug.Log("Max");
            return;
        }

        SoundManager.Instance.PlaySFXOneShot("GetItem");
        GameObject newBread = ObjectPoolManager.Instance.SpawnFromPool("Stack", Vector3.zero, Quaternion.Euler(0, 0, 0));

        if (newBread == null)
        {
            Debug.LogWarning("Failed to spawn bread from pool.");
            return;
        }

        if (stack != null)
        {
            newBread.transform.SetParent(stack);
            newBread.transform.localPosition = new Vector3(0, 0.25f * m_StackedBreads.Count, 0);
            newBread.transform.rotation = new Quaternion(0, 0, 0, 0);
        }

        if (m_Anim != null && !m_Anim.GetBool("Stack"))
            m_Anim.SetBool("Stack", true);

        m_StackedBreads.Add(newBread);
        curRequest--;
        UpdateRequest();
    }

    public void RemoveBreadToStack()
    {
        if (m_StackedBreads == null)
        {
            Debug.Log("stack Null");
            return;
        }

        if (curRequest >= request)
        {
            Debug.Log("인덱스 아웃남");
            return;
        }

        SoundManager.Instance.PlaySFXOneShot("PutItem");

        int index = (request - curRequest) - 1;
        if (index < 0 || index >= m_StackedBreads.Count)
            return;

        GameObject lastBread = m_StackedBreads[index];
        m_StackedBreads.RemoveAt(index);

        curRequest++;
        ObjectPoolManager.Instance.ReturnToPool("Stack", lastBread);

        if (m_Anim != null && m_StackedBreads.Count == 0)
            m_Anim.SetBool("Stack", false);
    }

    public bool HasBread()
    {
        return m_StackedBreads.Count > 0;
    }
}
