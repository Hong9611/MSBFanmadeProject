using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour, IEventObject
{
    public Transform moneyPool;
    public GameObject paperBag;

    public List<GameObject> moneyList => moneys;

    private string moneyPoolTag = "Money";
    private bool onPlayer = false;

    private Coroutine calculateCoroutine;
    private List<GameObject> moneys = new List<GameObject>();

    public void OnUnitEnter(Collider Unit)
    {
        onPlayer = true;
        var cusMana = CustomerLineManager.Instance;

        if (Unit.CompareTag("Player"))
        {
            Customer firstCustomer = cusMana.GetFirstCustomer(cusMana.firstLine);

            if (firstCustomer == null) return;

            if (firstCustomer.request != 1 /* && firstCustomer != null*/ )
            {
                if (calculateCoroutine == null)
                {
                    calculateCoroutine = StartCoroutine(calculateBread(firstCustomer));
                }
            }
        }
    }

    private IEnumerator calculateBread(Customer customer)
    {
        while (true)
        {
            var manager = CustomerLineManager.Instance;

            if (!onPlayer)
            {
                yield return null;
                continue;
            }
            if (!(manager.LineQuDic[manager.firstLine].Count > 0))
            {
                yield return null;
                continue;
            }
            if (customer == null)
            {
                customer = manager.GetFirstCustomer(manager.firstLine);
                if (customer == null)
                {
                    yield return null;
                    continue;
                }
            }

            if (!paperBag.activeSelf)
            {
                paperBag.SetActive(true);
                yield return new WaitForSeconds(0.4f);
            }

            customer.RemoveBreadToStack();

            if (customer.curRequest == customer.request)
            {
                var animP = paperBag.GetComponent<Animator>();

                SoundManager.Instance.PlaySFXOneShot("GetMoney");
                animP.SetBool("Package", true);
                SoundManager.Instance.PlaySFXOneShot("GetItem");

                yield return new WaitForSeconds(0.15f);

                paperBag.SetActive(false);
                animP.SetBool("Package", false);

                int cost = GameManager.Instance.breadCost * customer.request;

                SpawnMoney(cost);

                GameManager.Instance.ChangeTarget(Arrows.moneyArea);

                customer.balloon.SetActive(false);
                customer.satisfaction.SetActive(true);
                customer.paperBack.SetActive(true);
                customer.Anim.SetBool("Stack", true);

                manager.RemoveCustomerInQueue(manager.firstLine, customer);
                manager.Enqueue(manager.endPoint, customer);
                customer = null;
                if (GameManager.Instance.firstCal)
                {
                    GameManager.Instance.eventCamera.ShowEvent(0);
                    GameManager.Instance.firstCal = false;
                }

                if (manager.LineQuDic[manager.firstLine].Count > 0)
                {
                    customer = null;
                }
                else
                {
                    calculateCoroutine = null;
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SpawnMoney(int amount)
    {
        int num = moneys.Count > 0 ? moneys.Count : 0;

        for (int i = num; i < (num + amount); i++)
        {
            GameObject money = ObjectPoolManager.Instance.SpawnFromPool(moneyPoolTag, Vector3.zero, Quaternion.identity, "Counter");
            moneys.Add(money);

            if (money.GetComponent<Money>().enabled == false) { money.GetComponent<Money>().enabled = true; }

            if (money == null) continue;

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

    public void OnUnitExit(Collider Unit)
    {
        if (Unit.CompareTag("Player"))
        {
            onPlayer = false;
            if (calculateCoroutine != null)
            {
                StopCoroutine(calculateCoroutine);
                calculateCoroutine = null;
            }
        }
    }
}