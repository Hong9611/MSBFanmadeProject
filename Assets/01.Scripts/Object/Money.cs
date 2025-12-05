using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour, IPooledObject
{
    private Counter counter;
    private LockArea lockArea;

    private static float lastSoundTime = 0f;
    private static float soundCooldown = 0.1f;

    public void OnObjectSpawn(string maker = null)
    {
        if (maker != null)
        {
            switch (maker)
            {
                case "LockArea":
                    lockArea = GameObject.FindGameObjectWithTag("LockArea").GetComponent<LockArea>();
                    break;
                case "Counter":
                    counter = GameObject.FindGameObjectWithTag("Counter").GetComponent<Counter>();
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (lockArea != null)
            {
                AnimateParabola(transform, other.transform.position, lockArea.moneyList);

            }
            else if (counter != null)
            {
                AnimateParabola(transform, other.transform.position, counter.moneyList);
            }
        }
    }

    void AnimateParabola(Transform obj, Vector3 target, List<GameObject> moneyList, float jumpPower = 2f, float duration = 0.5f)
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
            if (Time.time - lastSoundTime >= soundCooldown)
            {
                SoundManager.Instance.PlaySFXOneShot("GetMoney");
                lastSoundTime = Time.time;
            }

            if (GameManager.Instance.target != null && GameManager.Instance.target == GameManager.Instance.targetAble[Arrows.moneyArea])
            {
                GameManager.Instance.ChangeTarget(Arrows.None);
            }

            ObjectPoolManager.Instance.ReturnToPool("Money", obj.gameObject);
            moneyList.Remove(gameObject);
            GameManager.Instance.money++;
            GameManager.Instance.MoneyUIUpdate();
        });
    }
}