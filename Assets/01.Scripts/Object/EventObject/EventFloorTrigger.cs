using UnityEngine;

public class EventFloorTrigger : MonoBehaviour
{
    public IEventObject parentObject;

    public string playerTag = "Player";
    public string CustomerTag = "Customer";

    void OnEnable()
    {
        parentObject = transform.parent.GetComponent<IEventObject>();

        Collider eventCollider = GetComponent<Collider>();
        if (eventCollider != null)
        {
            eventCollider.isTrigger = true;
        }
        else
        {
            enabled = false;
            return;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
    }

    void OnTriggerEnter(Collider Unit)
    {
        parentObject?.OnUnitEnter(Unit);
    }
    void OnTriggerExit(Collider Unit)
    {
        parentObject?.OnUnitExit(Unit);
    }
}