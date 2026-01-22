using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerLineManager : Singleton<CustomerLineManager>
{
    public List<Transform> m_StartPoints = new(3);
    public List<Transform> m_WaitPoints = new(3);

    public Transform FirstLinePoint => m_StartPoints[0];
    public Transform SecondLinePoint => m_StartPoints[1];
    public Transform EndPoint => m_StartPoints[2];

    private Dictionary<Transform, Queue<Customer>> m_LineQueues = new();
    private Dictionary<Transform, GameObject> m_CanWait = new();

    public Dictionary<Transform, Queue<Customer>> LineQueues => m_LineQueues;
    public Dictionary<Transform, GameObject> CanWait => m_CanWait;

    private void Awake()
    {
        if (!ValidatePoints())
        {
            enabled = false;
            return;
        }

        m_LineQueues.Clear();
        for (int i = 0; i < m_StartPoints.Count; i++)
        {
            Transform point = m_StartPoints[i];
            m_LineQueues[point] = new Queue<Customer>();
        }

        m_CanWait.Clear();
        for (int i = 0; i < m_WaitPoints.Count; i++)
        {
            Transform point = m_WaitPoints[i];
            m_CanWait[point] = null;
        }
    }

    private bool ValidatePoints()
    {
        if (m_StartPoints == null || m_StartPoints.Count < 3)
        {
            Debug.LogError($"{nameof(CustomerLineManager)}: StartPoints가 3개 미만입니다.");
            return false;
        }

        for (int i = 0; i < m_StartPoints.Count; i++)
        {
            if (m_StartPoints[i] == null)
            {
                Debug.LogError($"{nameof(CustomerLineManager)}: StartPoints[{i}]가 비어 있습니다.");
                return false;
            }
        }

        if (m_WaitPoints == null)
        {
            Debug.LogError($"{nameof(CustomerLineManager)}: WaitPoints가 null입니다.");
            return false;
        }

        for (int i = 0; i < m_WaitPoints.Count; i++)
        {
            if (m_WaitPoints[i] == null)
            {
                Debug.LogError($"{nameof(CustomerLineManager)}: WaitPoints[{i}]가 비어 있습니다.");
                return false;
            }
        }

        return true;
    }

    private void Update()
    {
        if (!m_LineQueues.TryGetValue(SecondLinePoint, out Queue<Customer> queue))
            return;

        var game = GameManager.Instance;

        if (queue.Count <= 0)
            return;

        if (!game.areaOpenBool.TryGetValue("Floor_Lock", out bool open) || !open)
            return;

        var lockArea = game.LockAreas[0];

        if (!lockArea.IsAvailable)
            return;

        var customer = GetFirstCustomer(SecondLinePoint);
        if (customer == null)
            return;

        RemoveCustomerInQueue(SecondLinePoint, customer);

        if (!lockArea.ReserveCustomer(customer))
        {
            Enqueue(SecondLinePoint, customer);
            return;
        }

        customer.SetCustomerDestination(lockArea.waitPoint);
    }

    public void Enqueue(Transform p_Point, Customer p_Customer, Vector3? p_LookAt = null)
    {
        if (p_Point == null || p_Customer == null)
            return;

        if (m_LineQueues.TryGetValue(p_Point, out Queue<Customer> lineQueue))
        {
            lineQueue.Enqueue(p_Customer);
            UpdateQueuePositions();
            return;
        }

        if (m_CanWait.ContainsKey(p_Point))
            m_CanWait[p_Point] = p_Customer.gameObject;

        p_Customer.SetCustomerDestination(p_Point.position, p_LookAt);
    }

    public void RemoveCustomerInQueue(Transform p_Point, Customer p_Customer)
    {
        if (p_Point == null || p_Customer == null)
            return;

        if (!m_LineQueues.TryGetValue(p_Point, out Queue<Customer> oldQueue))
            return;

        Queue<Customer> newQueue = new Queue<Customer>();
        foreach (var cust in oldQueue)
        {
            if (cust != p_Customer)
                newQueue.Enqueue(cust);
        }

        m_LineQueues[p_Point] = newQueue;
        UpdateQueuePositions();
    }

    public void RemoveFromAllLines(Customer p_Customer)
    {
        if (p_Customer == null)
            return;

        var keys = m_LineQueues.Keys.ToList();

        foreach (var key in keys)
        {
            Queue<Customer> oldQueue = m_LineQueues[key];
            if (oldQueue.Count == 0)
                continue;

            Queue<Customer> newQueue = new Queue<Customer>();
            foreach (var c in oldQueue)
            {
                if (c != null && c != p_Customer)
                    newQueue.Enqueue(c);
            }

            m_LineQueues[key] = newQueue;
        }

        UpdateQueuePositions();
    }

    public Customer GetFirstCustomer(Transform p_Point)
    {
        if (p_Point == null)
            return null;

        if (!m_LineQueues.TryGetValue(p_Point, out Queue<Customer> queue))
            return null;

        return queue.Count > 0 ? queue.Peek() : null;
    }

    private void UpdateQueuePositions()
    {
        Vector3 backward = Vector3.left;

        foreach (var kvp in m_LineQueues)
        {
            Vector3 basePos = kvp.Key.position;
            int i = 0;

            foreach (var c in kvp.Value)
            {
                if (c == null || !c.gameObject.activeInHierarchy)
                    continue;

                Vector3 pos = basePos + backward * (i * 1.5f);

                Vector3? lookAt = null;
                if (kvp.Key == FirstLinePoint || kvp.Key == SecondLinePoint)
                    lookAt = pos + Vector3.right;

                c.SetCustomerDestination(pos, lookAt);
                i++;
            }
        }
    }

    public Transform GetKeyByValue(Dictionary<Transform, GameObject> p_Dict, GameObject p_Value)
    {
        var pair = p_Dict.FirstOrDefault(x => x.Value == p_Value);
        return pair.Equals(default(KeyValuePair<Transform, GameObject>)) ? null : pair.Key;
    }
}
