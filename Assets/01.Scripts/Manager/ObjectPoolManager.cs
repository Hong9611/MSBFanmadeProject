using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public List<Pool> pools;

    void Awake()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var pool in pools)
        {
            pool.Initialize(this.transform);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, string Maker = null)
    {
        Pool targetPool = pools.Find(p => p.tag == tag);

        if (targetPool == null)
        {
            Debug.LogWarning($"Tag <{tag}> Null.");
            return null;
        }

        if (targetPool.Count == 0)
        {
            if (targetPool.canGrow)
            {
                Debug.LogWarning($"Pool {tag} is empty. Growing pool by 1.");
                GameObject newObj = Instantiate(targetPool.prefab, this.transform);
                newObj.SetActive(false);
                targetPool.Add(newObj);
            }
            else
            {
                Debug.LogWarning($"Pool {tag} is empty and cannot grow.");
                return null;
            }
        }

        GameObject objToSpawn = targetPool.Get();
        if (objToSpawn == null)
        {
            Debug.LogWarning($"Pool {tag} returned null object.");
            return null;
        }

        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.transform.SetParent(null);

        Rigidbody rb = objToSpawn.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        IPooledObject pooledObj = objToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn(Maker);
        }

        return objToSpawn;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        Pool targetPool = pools.Find(p => p.tag == tag);

        if (targetPool == null)
        {
            Debug.LogWarning($"Pool <{tag}> Null for return.");
            Destroy(obj);
            return;
        }

        foreach (var mb in obj.GetComponents<MonoBehaviour>())
        {
            mb.StopAllCoroutines();
        }

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        obj.SetActive(false);
        obj.transform.SetParent(this.transform);
        targetPool.Add(obj);
    }
}

public interface IPooledObject
{
    void OnObjectSpawn(string maker = null);
}