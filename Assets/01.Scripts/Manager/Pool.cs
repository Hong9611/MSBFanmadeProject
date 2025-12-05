using System;
using System.Collections.Generic;
using UnityEngine;

public enum PoolOrderType
{
    FIFO, // Queue
    LIFO  // Stack
}

[System.Serializable]
public class Pool
{
    public string tag;
    public GameObject prefab;
    public int initialSize;
    public bool canGrow = true;

    public PoolOrderType orderType = PoolOrderType.FIFO;

    [NonSerialized]
    private Queue<GameObject> m_Queue;

    [NonSerialized]
    private Stack<GameObject> m_Stack;

    public void Initialize(Transform p_Parent)
    {
        m_Queue = new Queue<GameObject>();
        m_Stack = new Stack<GameObject>();

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab, p_Parent);
            obj.SetActive(false);
            Add(obj);
        }
    }

    public int Count
    {
        get
        {
            return orderType == PoolOrderType.FIFO
                ? m_Queue.Count
                : m_Stack.Count;
        }
    }

    public void Add(GameObject p_Object)
    {
        if (orderType == PoolOrderType.FIFO)
        {
            m_Queue.Enqueue(p_Object);
        }
        else
        {
            m_Stack.Push(p_Object);
        }
    }

    public GameObject Get()
    {
        if (orderType == PoolOrderType.FIFO)
        {
            return m_Queue.Count > 0 ? m_Queue.Dequeue() : null;
        }
        else
        {
            return m_Stack.Count > 0 ? m_Stack.Pop() : null;
        }
    }
}