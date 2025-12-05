using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum Arrows
{
    None,
    bakeBread,
    breadDisplay,
    counter,
    moneyArea,
    lockArea,
}

public class GameManager : Singleton<GameManager>
{
    public int money;
    public int breadCost = 5;
    public int customerNum = 0;

    public bool firstCal = true;
    public bool firstClean = true;

    public Transform target;
    public TMP_Text moneyText;

    public CameraEventController eventCamera;

    [SerializedDictionary("EnumArrows", "Arrow")]
    public SerializedDictionary<Arrows, GameObject> targetAble;
    [SerializedDictionary("AreaName", "OpenBool")]
    public SerializedDictionary<string, bool> areaOpenBool;

    public Indicator indicator;

    private List<BreadDisplay> breadDisplays = new List<BreadDisplay>();
    private List<LockArea> lockAreas = new List<LockArea>();

    public List<BreadDisplay> BreadDisplays => breadDisplays;
    public List<LockArea> LockAreas => lockAreas;

    void Awake()
    {
        money = 50;
        ChangeTarget(Arrows.bakeBread);
        areaOpenBool["Floor_Lock"] = false;
        firstCal = true;
        firstClean = true;
    }

    private Coroutine customerSpawnCoroutine;
    public float spawnInterval = 1f;

    private void Start()
    {
        customerSpawnCoroutine = StartCoroutine(CustomerSpawnLoop());
    }

    private IEnumerator CustomerSpawnLoop()
    {
        while (true)
        {
            TrySpawnCustomersToEmptySeats();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void TrySpawnCustomersToEmptySeats()
    {
        var cuLineMana = CustomerLineManager.Instance;
        var objPool = ObjectPoolManager.Instance;

        Pool pool = null;
        var coordinate = new List<Vector3>
        {
            new Vector3(-6.5f,0.5f,-3.8f),
            new Vector3(-5.0f,0.5f,-5f),
            new Vector3(-6.5f,0.5f,-6.2f)
        };

        for (int i = 0; i < objPool.pools.Count; i++)
        {
            if (objPool.pools[i].tag == "Customer")
            {
                pool = objPool.pools[i];
                break;
            }
        }

        if (pool != null && pool.initialSize <= customerNum)
        {
            return;
        }

        for (int i = 0; i < cuLineMana.canWait.Count; i++)
        {
            if (cuLineMana.canWait[coordinate[i]] == null)
            {
                var c = objPool.SpawnFromPool("Customer", cuLineMana.endPoint, Quaternion.Euler(0, 90, 0));
                customerNum++;
                cuLineMana.canWait[coordinate[i]] = c;
                cuLineMana.Enqueue(coordinate[i], c.GetComponent<Customer>(), new Vector3(-6.5f, 0.5f, -5f));
                break;
            }
        }
    }

    public void MoneyUIUpdate()
    {
        moneyText.text = money.ToString();
    }

    public void ChangeTarget(Arrows name)
    {
        if (name == Arrows.None)
        {
            if (target != null)
            {
                target.gameObject.SetActive(false);
                target = null;
            }
            if (indicator != null)
            {
                indicator.gameObject.SetActive(false);
            }
            return;
        }

        if (targetAble.Count > 0)
        {
            if (target != null)
            {
                target.gameObject.SetActive(false);
                target = null;
            }
            if (!indicator.gameObject.activeSelf)
            {
                indicator.gameObject.SetActive(true);
            }
            target = targetAble[name].transform;
            target.gameObject.SetActive(true);
            indicator.SetIndicatorTarget();
        }
    }

    public void AddBreadDisplay(BreadDisplay breadDisplay)
    {
        breadDisplays.Add(breadDisplay);
    }

    public void RemoveBreadDisplay(BreadDisplay breadDisplay)
    {
        breadDisplays.Remove(breadDisplay);
    }

    public void AddLockAreas(LockArea lockArea)
    {
        lockAreas.Add(lockArea);
    }

    public void RemoveLockAreas(LockArea lockArea)
    {
        lockAreas.Remove(lockArea);
    }
}