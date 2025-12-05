using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BreadDisplay : MonoBehaviour, IEventObject
{
    public int curStack = 0;
    public int maxStack = 8;
    public Transform display;

    private string breadPoolTag = "DisplayBread";
    private Coroutine moveToDisplayCoroutine;

    // 손님별 코루틴 관리용
    private Dictionary<Customer, Coroutine> customerCoroutines = new Dictionary<Customer, Coroutine>();

    // 빵 받는 순서를 관리하는 큐 (도착 순서)
    private List<Customer> breadQueue = new List<Customer>();

    private List<GameObject> displayBreads = new List<GameObject>();

    private Vector3[] customerPoints = new Vector3[]
    {
        new Vector3(0f, 0f, 1.2f),
        new Vector3(1.5f, 0f, 0f),
        new Vector3(0f, 0f, -1.2f),
    };

    private List<Customer> occupyingCustomers = new List<Customer>();

    private void Awake()
    {
        GameManager.Instance.AddBreadDisplay(this);
    }

    public void AddBreadToStack()
    {
        if (curStack >= maxStack) return;

        var newBread = ObjectPoolManager.Instance.SpawnFromPool(breadPoolTag, Vector3.zero, Quaternion.identity);
        if (!newBread) return;

        newBread.transform.SetParent(display);

        int row = curStack % 4;
        int col = curStack / 4;
        newBread.transform.localPosition = new Vector3(-0.75f + 0.5f * row, 0f, -0.35f + 0.7f * col);
        newBread.transform.localRotation = Quaternion.Euler(0, -35, 0);

        displayBreads.Add(newBread);
        curStack++;
    }

    public void RemoveBreadToStack()
    {
        if (curStack <= 0) return;

        GameObject lastBread = displayBreads[curStack - 1];
        displayBreads.RemoveAt(curStack - 1);
        ObjectPoolManager.Instance.ReturnToPool(breadPoolTag, lastBread);
        curStack--;
    }

    public Vector3? GetAvailablePoint(Customer newCustomer = null)
    {
        foreach (var localPos in customerPoints)
        {
            Vector3 worldPos = transform.TransformPoint(localPos);
            if (!NavMesh.SamplePosition(worldPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                continue;

            bool occupied = false;
            foreach (var c in occupyingCustomers)
            {
                if (c && Vector3.Distance(c.transform.position, hit.position) < 0.5f)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                if (newCustomer != null) occupyingCustomers.Add(newCustomer);
                return hit.position;
            }
        }
        return null;
    }

    public void OnUnitEnter(Collider Unit)
    {
        Debug.Log("Enter:" + Unit.name);
        if (Unit.CompareTag("Player"))
        {
            var player = Unit.GetComponent<Player>();
            if (player != null)
            {
                moveToDisplayCoroutine = StartCoroutine(MoveBreadToDisplay(player));
                GameManager.Instance.ChangeTarget(Arrows.counter);
            }
        }
        else if (Unit.CompareTag("Customer"))
        {
            var customer = Unit.GetComponent<Customer>();
            if (customer != null)
            {
                // 큐에 없으면 대기열 맨 뒤에 추가
                if (!breadQueue.Contains(customer))
                    breadQueue.Add(customer);

                // 코루틴도 없으면 시작
                if (!customerCoroutines.ContainsKey(customer))
                {
                    var co = StartCoroutine(MoveBreadFromDisplay(customer));
                    customerCoroutines.Add(customer, co);
                }
            }
        }
    }

    private IEnumerator MoveBreadToDisplay(Player player)
    {
        while (true)
        {
            if (player == null)
                yield break;

            if (player.curStack > 0 && curStack < maxStack)
            {
                player.RemoveBreadFromStack();
                AddBreadToStack();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator MoveBreadFromDisplay(Customer customer)
    {
        while (true)
        {
            if (customer == null)
                yield break;

            // 이 손님이 지금 빵을 받을 "순번"이 아니면 대기
            if (breadQueue.Count == 0 || breadQueue[0] != customer)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            // 이 손님의 요청이 끝났으면 큐에서 제거하고 종료
            if (customer.curRequest <= 0)
            {
                if (breadQueue.Count > 0 && breadQueue[0] == customer)
                    breadQueue.RemoveAt(0);
                else
                    breadQueue.Remove(customer); // 혹시나 리스트 중간에 있을 경우 방어 코드

                yield break;
            }

            // 빵이 있고, 이 손님의 차례일 때만 빵 지급
            if (curStack > 0 && customer.curRequest > 0)
            {
                customer.AddBreadToDisplay();
                RemoveBreadToStack();
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnUnitExit(Collider Unit)
    {
        if (Unit.CompareTag("Player"))
        {
            if (moveToDisplayCoroutine != null)
            {
                StopCoroutine(moveToDisplayCoroutine);
                moveToDisplayCoroutine = null;
            }
        }
        else if (Unit.CompareTag("Customer"))
        {
            var customer = Unit.GetComponent<Customer>();

            // 자리 점유 해제
            ReleaseCustomer(customer);

            if (customer != null)
            {
                // 큐에서도 제거
                breadQueue.Remove(customer);

                // 코루틴 정리
                if (customerCoroutines.TryGetValue(customer, out var co))
                {
                    StopCoroutine(co);
                    customerCoroutines.Remove(customer);
                }
            }
        }
    }

    public void ReleaseCustomer(Customer c)
    {
        if (c && occupyingCustomers.Contains(c))
            occupyingCustomers.Remove(c);
    }

    private void OnDisable()
    {
        occupyingCustomers.Clear();
        breadQueue.Clear();

        foreach (var kvp in customerCoroutines)
        {
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);
        }
        customerCoroutines.Clear();

        if (moveToDisplayCoroutine != null)
        {
            StopCoroutine(moveToDisplayCoroutine);
            moveToDisplayCoroutine = null;
        }
    }
}