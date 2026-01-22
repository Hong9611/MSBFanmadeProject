//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.AI;

//public class CustomerLegacy : MonoBehaviour, IPooledObject
//{
//    public int request;
//    public int curRequest;
//    public TMP_Text requestText;

//    public GameObject requstObject;
//    public GameObject calculation;
//    public GameObject wantEat;
//    public GameObject satisfaction;
//    public GameObject balloon;
//    public GameObject paperBack;

//    public Transform stack;

//    public Animator Anim => anim;

//    private bool endOn = false;

//    private NavMeshAgent agent;
//    private Animator anim;
//    private Coroutine currentMovementCoroutine;

//    private List<GameObject> stackedBreads = new List<GameObject>();

//    private void Awake()
//    {
//        agent = GetComponent<NavMeshAgent>();
//    }

//    public void OnObjectSpawn(string maker = null)
//    {
//        endOn = false;

//        for (int i = stack.childCount - 1; i >= 0; i--)
//        {
//            Transform child = stack.GetChild(i);
//            ObjectPoolManager.Instance.ReturnToPool("Stack", child.gameObject);
//        }
//        stackedBreads.Clear();

//        agent = GetComponent<NavMeshAgent>();
//        anim = GetComponent<Animator>();

//        anim.SetBool("Move", false);
//        anim.SetBool("Stack", false);
//        anim.SetBool("Sit", false);
//        satisfaction.SetActive(false);
//        calculation.SetActive(false);
//        paperBack.SetActive(false);
//        wantEat.SetActive(false);

//        balloon.SetActive(true);
//        requstObject.SetActive(true);

//        SetRequest();
//    }

//    void SetRequest()
//    {
//        request = Random.Range(1, 4);
//        curRequest = request;
//        UpdateRequest();
//    }

//    public void UpdateRequest()
//    {
//        requestText.text = curRequest.ToString();
//        var customerManager = CustomerLineManager.Instance;

//        if (curRequest == 0)
//        {
//            var findVal = customerManager.GetKeyByValue(customerManager.CanWait, gameObject);

//            if (findVal.HasValue)
//            {
//                customerManager.CanWait[findVal.Value] = null;
//            }
//            requstObject.SetActive(false);

//            if (request == 1)
//            {
//                wantEat.SetActive(true);
//                if (customerManager.LineQueues.ContainsKey(new Vector3(-1.65f, 0.5f, 2.75f)))
//                {
//                    //customerManager.Enqueue(new Vector3(-1.65f, 0.5f, 2.75f), this);
//                }
//            }
//            else
//            {
//                calculation.SetActive(true);
//                if (customerManager.LineQueues.ContainsKey(new Vector3(-1.65f, 0.5f, 0.25f)))
//                {
//                    //customerManager.Enqueue(new Vector3(-1.65f, 0.5f, 0.25f), this);
//                }
//            }
//        }
//    }

//    public void SetCustomerDestination(Vector3 target, Vector3? lookAt = null)
//    {
//        if (currentMovementCoroutine != null)
//        {
//            StopCoroutine(currentMovementCoroutine);
//            currentMovementCoroutine = null;
//        }

//        // endPoint 근처인지로 판정 (float 비교 오차 방지)
//        if (Vector3.Distance(target, CustomerLineManager.Instance.EndLine) < 0.01f)
//        {
//            endOn = true;
//        }
//        else
//        {
//            endOn = false;
//        }

//        if (!gameObject.activeInHierarchy) return;
//        if (agent == null || !agent.isOnNavMesh) return;

//        agent.SetDestination(target);
//        if (anim != null)
//        {
//            currentMovementCoroutine = StartCoroutine(MoveAndAnimate(lookAt));
//        }
//    }

//    IEnumerator MoveAndAnimate(Vector3? lookAt = null)
//    {
//        anim.SetBool("Move", true);

//        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
//        {
//            yield return null;
//        }

//        if (anim != null)
//        {
//            anim.SetBool("Move", false);
//        }

//        if (lookAt != null)
//        {
//            Vector3 directionToTarget = lookAt.Value - transform.position;

//            if (directionToTarget.sqrMagnitude > 0.0001f)
//            {
//                Quaternion startRot = transform.rotation;
//                Quaternion targetRot = Quaternion.LookRotation(directionToTarget);

//                float rotateTime = 0.25f;
//                float t = 0f;

//                while (t < 1f)
//                {
//                    t += Time.deltaTime / rotateTime;
//                    transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
//                    yield return null;
//                }

//                transform.rotation = targetRot;
//            }
//        }

//        currentMovementCoroutine = null;

//        if (endOn)
//        {
//            if (paperBack.activeSelf)
//            {
//                paperBack.SetActive(false);
//            }

//            //CustomerLineManager.Instance.RemoveFromAllLines(this);

//            ObjectPoolManager.Instance.ReturnToPool("Customer", gameObject);
//            GameManager.Instance.customerNum--;
//        }
//    }

//    public void AddBreadToDisplay()
//    {
//        if (curRequest <= 0)
//        {
//            Debug.Log("Max");
//            return;
//        }

//        SoundManager.Instance.PlaySFXOneShot("GetItem");
//        GameObject newBread = ObjectPoolManager.Instance.SpawnFromPool("Stack", Vector3.zero, Quaternion.Euler(0, 0, 0));

//        if (newBread == null)
//        {
//            Debug.LogWarning("Failed to spawn bread from pool.");
//            return;
//        }

//        newBread.transform.SetParent(stack);
//        newBread.transform.localPosition = new Vector3(0, 0.25f * stackedBreads.Count, 0);
//        newBread.transform.rotation = new Quaternion(0, 0, 0, 0);

//        if (!anim.GetBool("Stack"))
//            anim.SetBool("Stack", true);

//        stackedBreads.Add(newBread);
//        curRequest--;
//        UpdateRequest();
//    }

//    public void RemoveBreadToStack()
//    {

//        if (stackedBreads == null)
//        {
//            Debug.Log("stack Null");
//            return;
//        }

//        if (curRequest >= request)
//        {
//            Debug.Log("인덱스 아웃남");
//            return;
//        }

//        SoundManager.Instance.PlaySFXOneShot("PutItem");
//        GameObject lastBread = stackedBreads[(request - curRequest) - 1];
//        stackedBreads.RemoveAt((request - curRequest) - 1);

//        curRequest++;
//        ObjectPoolManager.Instance.ReturnToPool("Stack", lastBread);
//    }

//    public bool HasBread()
//    {
//        return stackedBreads.Count > 0;
//    }
//}
