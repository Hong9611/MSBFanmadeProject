using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerLineManager : Singleton<CustomerLineManager>
{
    public Dictionary<Vector3, Queue<Customer>> LineQuDic => lineQuDic;
    public Vector3 firstLine => startPoint[0];
    public Vector3 secondLine => startPoint[1];
    public Vector3 endPoint => startPoint[2];

    private Dictionary<Vector3, Queue<Customer>> lineQuDic = new Dictionary<Vector3, Queue<Customer>>();
    private Vector3[] startPoint = new Vector3[]
    {
        new Vector3(-1.65f, 0.5f, 0.25f),
        new Vector3(-1.65f, 0.5f, 2.75f),
        new Vector3(-9.5f, 0.5f, 0)
    };

    public Dictionary<Vector3, GameObject> canWait = new Dictionary<Vector3, GameObject>()
    {
        {new Vector3(-6.5f,0.5f,-3.8f), null },
        {new Vector3(-5.0f,0.5f,-5f), null },
        {new Vector3(-6.5f,0.5f,-6.2f), null }
    };

    private void Awake()
    {
        lineQuDic.Clear();
        for (int i = 0; i < startPoint.Length; i++)
        {
            lineQuDic.Add(startPoint[i], new Queue<Customer>());
        }
    }

    private void Update()
    {
        if (lineQuDic.TryGetValue(secondLine, out Queue<Customer> que))
        {
            var game = GameManager.Instance;

            if (que.Count > 0 && game.areaOpenBool.TryGetValue("Floor_Lock", out bool open))
            {
                if (!open) return;

                var lockArea = game.LockAreas[0];

                // LockArea가 예약 가능 상태가 아니면 대기
                if (!lockArea.IsAvailable)
                    return;

                var c = GetFirstCustomer(secondLine);
                if (c == null) return;

                // 큐에서 제거
                RemoveCustomerInQueue(secondLine, c);

                // 예약 시도
                if (!lockArea.ReserveCustomer(c))
                {
                    // 예약에 실패하면 다시 큐에 넣고 정렬
                    Enqueue(secondLine, c);
                    return;
                }

                // 예약 성공 → waitPoint로 이동
                c.SetCustomerDestination(lockArea.waitPoint);
                // SetEatingCustomer는 이제 필요 없음 (ReserveCustomer에서 eatingCustomer 설정)
            }
        }
    }

    public void Enqueue(Vector3 lineStart, Customer c, Vector3? lookAt = null)
    {
        if (!lineQuDic.ContainsKey(lineStart))
        {
            if (canWait.ContainsKey(lineStart))
            {
                canWait[lineStart] = c.gameObject;
            }
            c.SetCustomerDestination(lineStart, lookAt);
        }
        else
        {
            var lineQueue = lineQuDic[lineStart];
            lineQueue.Enqueue(c);
            UpdateQueuePositions();
        }
    }

    public void RemoveCustomerInQueue(Vector3 lineStart, Customer c)
    {
        Queue<Customer> newQueue = new Queue<Customer>();
        foreach (var cust in lineQuDic[lineStart])
        {
            if (cust != c)
                newQueue.Enqueue(cust);
        }
        lineQuDic[lineStart] = newQueue;

        UpdateQueuePositions();
    }

    public void RemoveFromAllLines(Customer customer)
    {
        var keys = lineQuDic.Keys.ToList();

        foreach (var key in keys)
        {
            Queue<Customer> oldQueue = lineQuDic[key];
            if (oldQueue.Count == 0) continue;

            Queue<Customer> newQueue = new Queue<Customer>();
            foreach (var c in oldQueue)
            {
                if (c != customer && c != null)
                    newQueue.Enqueue(c);
            }

            lineQuDic[key] = newQueue;
        }

        UpdateQueuePositions();
    }

    public Customer GetFirstCustomer(Vector3 lineStart)
    {
        if (lineQuDic[lineStart].Count > 0)
            return lineQuDic[lineStart].Peek();

        return null;
    }

    private void UpdateQueuePositions()
    {
        Vector3 backward = Vector3.left;

        foreach (var kvp in lineQuDic)
        {
            Vector3 basePos = kvp.Key;
            int i = 0;
            foreach (var c in kvp.Value)
            {
                if (c == null || !c.gameObject.activeInHierarchy) continue;

                Vector3 pos = basePos + backward * (i * 1.5f);

                Vector3? lookAt = null;

                if (kvp.Key == firstLine || kvp.Key == secondLine)
                {
                    lookAt = pos + Vector3.right;
                }

                c.SetCustomerDestination(pos, lookAt);
                i++;
            }
        }
    }

    public Vector3? GetKeyByValue(Dictionary<Vector3, GameObject> dict, GameObject value)
    {
        var pair = dict.FirstOrDefault(x => x.Value == value);
        return pair.Equals(default(KeyValuePair<Vector3, GameObject>)) ? (Vector3?)null : pair.Key;
    }
}