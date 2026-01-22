using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class LockArea : MonoBehaviour, IEventObject
{
    public Vector3 waitPoint = new Vector3(-3.5f, 0.5f, 5.5f);
    public TMP_Text costText;
    public GameObject lockArea;
    public GameObject eatingArea;
    public GameObject chair;
    public GameObject bread;
    public GameObject trash;
    public GameObject cleanEffect;
    public GameObject upgradeMark;
    public Transform moneyPool;
    public List<Transform> effectObjList;

    private int curCost = 30;
    private int costForUnLock = 30;
    private bool isAreaOpen;
    private bool isNeedClean;

    // 이 LockArea가 현재 어떤 Customer에게 예약/사용 중인지
    private bool isProcessingCustomer = false;
    private Customer eatingCustomer;

    // 언락 비용만큼만 Money 이펙트를 스폰하기 위한 카운트
    private int remainingSpawnCount;

    private Coroutine costPayCoroutine;
    private Coroutine eatingCoroutine;

    private List<GameObject> moneys = new List<GameObject>();
    public List<GameObject> moneyList => moneys;

    private Vector3 exitPoint = new Vector3(-5.5f, 0.5f, 6.25f);
    private Vector3 sitPoint = new Vector3(-6, 1.1f, 7.5f);

    // CustomerLineManager에서 사용할 수 있는 “예약 가능 여부”
    public bool IsAvailable => isAreaOpen && !isProcessingCustomer && !isNeedClean;

    void Awake()
    {
        GameManager.Instance.AddLockAreas(this);
        isAreaOpen = GameManager.Instance.areaOpenBool["Floor_Lock"];

        if (isAreaOpen)
        {
            // 이미 언락된 상태로 시작하는 경우
            lockArea.SetActive(false);
            eatingArea.SetActive(true);
            upgradeMark.SetActive(true);

            curCost = 0;
            costText.text = "0";
            remainingSpawnCount = 0;
            isProcessingCustomer = false;
            eatingCustomer = null;
        }
        else
        {
            curCost = costForUnLock;
            costText.text = curCost.ToString();
            remainingSpawnCount = costForUnLock;
            isProcessingCustomer = false;
            eatingCustomer = null;
        }
    }

    // 외부에서 강제로 지정하고 싶을 때 쓸 수 있도록 남겨둔 함수
    public void SetEatingCustomer(Customer c)
    {
        eatingCustomer = c;
    }

    // CustomerLineManager에서 “다음 손님 예약”할 때 호출
    public bool ReserveCustomer(Customer c)
    {
        // 이미 누군가 사용 중이거나 청소 중이면 예약 불가
        if (!isAreaOpen || isProcessingCustomer || isNeedClean || c == null)
            return false;

        isProcessingCustomer = true;
        eatingCustomer = c;
        return true;
    }

    public void OnUnitEnter(Collider Unit)
    {
        if (isAreaOpen)
        {
            // 손님이 들어온 경우
            if (Unit.CompareTag("Customer"))
            {
                Customer customer = Unit.GetComponent<Customer>();
                if (customer == null)
                    return;

                // 이미 어떤 손님이 예약/사용 중인데, 그 손님이 아니라면 무시
                if (isProcessingCustomer && eatingCustomer != null && customer != eatingCustomer)
                    return;

                // 예외적으로 예약 없이 직접 들어온 경우를 방지하기 위한 방어 로직
                if (!isProcessingCustomer)
                {
                    isProcessingCustomer = true;
                    eatingCustomer = customer;
                }

                // 여기서부터는 eatingCustomer(예약된 손님)을 처리
                var agent = eatingCustomer.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.velocity = Vector3.zero;
                }

                eatingCustomer.SetCustomerDestination(sitPoint);
                eatingCustomer.transform.position = sitPoint;
                eatingCustomer.transform.rotation = Quaternion.Euler(0, 90, 0);

                if (eatingCustomer.HasBread())
                    eatingCustomer.RemoveBreadToStack();

                eatingCustomer.Anim.SetBool("Sit", true);
                eatingCustomer.Anim.SetBool("Stack", false);
                eatingCustomer.Anim.SetBool("Move", false);
                eatingCustomer.balloon.SetActive(false);
                bread.SetActive(true);

                if (eatingCoroutine != null)
                {
                    StopCoroutine(eatingCoroutine);
                    eatingCoroutine = null;
                }
                eatingCoroutine = StartCoroutine(DelayForEat(eatingCustomer));
            }
            // 청소가 필요한 상태에서 플레이어가 들어온 경우
            else if (isNeedClean && Unit.CompareTag("Player"))
            {
                cleanEffect.SetActive(true);
                SoundManager.Instance.PlaySFXOneShot("Unlock");
                AnimateParabola(trash.transform, Unit.transform.position);
            }
        }
        else
        {
            // 아직 언락되지 않은 상태: 플레이어가 비용 지불
            if (Unit.CompareTag("Player"))
            {
                if (remainingSpawnCount > 0 && GameManager.Instance.money > 0)
                {
                    int maxByCost = remainingSpawnCount;
                    int maxByMoney = GameManager.Instance.money;
                    int spawnCount = Mathf.Min(maxByCost, maxByMoney);

                    if (spawnCount > 0)
                    {
                        if (costPayCoroutine != null)
                        {
                            StopCoroutine(costPayCoroutine);
                            costPayCoroutine = null;
                        }

                        SoundManager.Instance.PlaySFXOneShot("UseMoney");
                        costPayCoroutine = StartCoroutine(SpawnMoneyStream(spawnCount, Unit.transform, transform));
                    }
                }
            }
        }
    }

    IEnumerator SpawnMoneyStream(int count, Transform start, Transform target)
    {
        for (int i = 0; i < count; i++)
        {
            if (remainingSpawnCount <= 0)
                yield break;

            var ob = ObjectPoolManager.Instance.SpawnFromPool("Money", start.position, Quaternion.identity);

            var moneyComp = ob.GetComponent<Money>();
            if (moneyComp != null && moneyComp.enabled == true)
                moneyComp.enabled = false;

            // 전체 언락 비용을 넘어서 스폰되지 않도록 스폰 시점에 차감
            remainingSpawnCount--;

            AnimateMoney(ob.transform, start.position, target.position);

            yield return new WaitForSeconds(0.05f);
        }
    }

    void AnimateMoney(Transform obj, Vector3 startPos, Vector3 target)
    {
        Vector3 start = startPos;
        Vector3 mid = (start + target) / 2f + Vector3.up * 2f;

        obj.DOPath(
            new Vector3[] { start, mid, target },
            0.3f,
            PathType.CatmullRom
        ).SetEase(Ease.OutQuad)
         .OnComplete(() =>
         {
             ObjectPoolManager.Instance.ReturnToPool("Money", obj.gameObject);
             GameManager.Instance.money--;
             curCost = Mathf.Max(curCost - 1, 0);
             costText.text = curCost.ToString();
             GameManager.Instance.MoneyUIUpdate();

             if (curCost == 0 && !isAreaOpen)
             {
                 SoundManager.Instance.PlaySFXOneShot("Unlock");

                 if (costPayCoroutine != null)
                 {
                     StopCoroutine(costPayCoroutine);
                     costPayCoroutine = null;
                 }

                 OpenEatingArea();
             }
         });
    }

    private void OpenEatingArea()
    {
        upgradeMark.SetActive(true);
        GameManager.Instance.areaOpenBool["Floor_Lock"] = true;
        isAreaOpen = true;
        lockArea.SetActive(false);
        eatingArea.SetActive(true);
        SquashStretchWave(effectObjList);
    }

    public void SquashStretchWave(List<Transform> objects, float duration = 0.2f)
    {
        if (objects == null || objects.Count == 0) return;

        Sequence seq = DOTween.Sequence();

        foreach (var obj in objects)
        {
            Vector3 originalScale = obj.localScale;

            seq.Join(
                obj.DOScale(originalScale * 0.85f, duration * 0.5f)
                   .SetEase(Ease.InSine)
                   .OnComplete(() =>
                   {
                       obj.DOScale(originalScale * Random.Range(1.2f, 1.4f), duration)
                          .SetEase(Ease.OutElastic)
                          .OnComplete(() =>
                          {
                              obj.DOScale(originalScale, duration * 0.5f)
                                 .SetEase(Ease.OutSine)
                                 .OnComplete(() =>
                                 {
                                     GameManager.Instance.eventCamera.ShowEvent(1);
                                 });
                          });
                   })
            );
        }
    }

    IEnumerator DelayForEat(Customer customer)
    {
        yield return new WaitForSeconds(5f);

        customer.SetCustomerDestination(exitPoint);
        customer.transform.position = exitPoint;
        customer.satisfaction.SetActive(true);
        customer.Anim.SetBool("Sit", false);
        customer.Anim.SetBool("Stack", false);
        customer.Anim.SetBool("Move", false);

        yield return new WaitForSeconds(1f);

        customer.SetCustomerDestination(CustomerLineManager.Instance.EndPoint.position);
        trash.SetActive(true);
        bread.SetActive(false);

        // 월드 좌표 기준으로 의자 위치/회전 설정
        chair.transform.position = new Vector3(-6.15f, 0.45f, 7.85f);
        chair.transform.localRotation = Quaternion.Euler(0, 140, 0);

        SoundManager.Instance.PlaySFXOneShot("GetMoney");
        SpawnMoney(GameManager.Instance.breadCost);
        isNeedClean = true;
        GameManager.Instance.ChangeTarget(Arrows.lockArea);
    }

    private void SpawnMoney(int amount)
    {
        int num = moneys.Count > 0 ? moneys.Count : 0;

        for (int i = num; i < (num + amount) * 2; i++)
        {
            GameObject money = ObjectPoolManager.Instance.SpawnFromPool("Money", Vector3.zero, Quaternion.identity, "LockArea");
            moneys.Add(money);

            if (money == null) continue;

            var moneyComp = money.GetComponent<Money>();
            if (moneyComp != null && !moneyComp.enabled)
                moneyComp.enabled = true;

            money.GetComponent<BoxCollider>().isTrigger = true;
            money.transform.SetParent(moneyPool);

            int layer = i / 9;
            int row = (i % 9) / 3;
            int col = i % 3;

            float x = moneyPool.position.x + (row * 0.5f);
            float y = moneyPool.position.y + (layer * 0.25f);
            float z = moneyPool.position.z + (col * 0.8f);

            money.transform.position = new Vector3(x, y, z);
        }
    }

    void AnimateParabola(Transform obj, Vector3 target, float jumpPower = 2f, float duration = 0.5f)
    {
        obj.DOJump(
            target,
            jumpPower,
            1,
            duration
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            obj.gameObject.SetActive(false);
            obj.localPosition = new Vector3(0f, 1.3f, 0f);
            chair.transform.localPosition = new Vector3(-1.4f, 0, 0);
            chair.transform.localRotation = Quaternion.Euler(0, 90, 0);
            GameManager.Instance.ChangeTarget(Arrows.bakeBread);
            isNeedClean = false;

            // 청소가 끝났으니 다음 손님 받을 준비 완료
            isProcessingCustomer = false;
            eatingCustomer = null;
        });
    }

    public void OnUnitExit(Collider Unit)
    {
        if (Unit.CompareTag("Player"))
        {
            if (costPayCoroutine != null)
            {
                StopCoroutine(costPayCoroutine);
                costPayCoroutine = null;
            }
        }
    }
}