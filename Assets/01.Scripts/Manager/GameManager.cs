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

        // Customer 풀 찾기
        Pool pool = null;
        for (int i = 0; i < objPool.pools.Count; i++)
        {
            if (objPool.pools[i].tag == "Customer")
            {
                pool = objPool.pools[i];
                break;
            }
        }

        // 풀 한계 체크(기존 로직 유지)
        if (pool != null && pool.initialSize <= customerNum)
            return;

        Vector3 lookAt = Vector3.zero;
        if (breadDisplays != null && breadDisplays.Count > 0)
        {
            lookAt = breadDisplays[0].gameObject.transform.position;
        }

        // 하이라키에 등록된 waitPoint 기반으로 빈 슬롯 찾기
        for (int i = 0; i < cuLineMana.m_WaitPoints.Count; i++)
        {
            var waitPos = cuLineMana.m_WaitPoints[i];

            // 안전장치: CanWait에 키가 없다면(초기화 누락/런타임 변경 등) 등록
            if (!cuLineMana.CanWait.ContainsKey(waitPos))
                cuLineMana.CanWait[waitPos] = null;

            if (cuLineMana.CanWait[waitPos] != null)
                continue;

            // 스폰 (EndLine 명칭은 현재 프로젝트에 맞게 유지)
            var go = objPool.SpawnFromPool("Customer", cuLineMana.EndPoint.position, Quaternion.Euler(0, 90, 0));
            customerNum++;

            cuLineMana.CanWait[waitPos] = go;
            cuLineMana.Enqueue(waitPos, go.GetComponent<Customer>(), lookAt);
            break;
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