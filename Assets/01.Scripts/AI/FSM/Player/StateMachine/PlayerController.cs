using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Player unit { get; private set; }
    public Transform pivotTransform;

    [HideInInspector]
    public Vector3 moveVector;
    public Vector2 moveInput;

    public FloatingJoystick joystick;

    public Rigidbody rb;

    public float speed;

    private void Awake()
    {
        if (unit == null)
        unit = GetComponent<Player>();
    }

    public void IdleUpdate()
    {
        if (unit.curStack > 0)
        {
            unit.animator.SetBool("Stack", true);
            unit.animator.SetBool("Move", false);
        }
        else
        {
            unit.animator.SetBool("Stack", false);
            unit.animator.SetBool("Move", false);
        }

        rb.velocity = Vector3.zero;
    }

    public void MoveUpdate()
    {
        Debug.Log("¿òÁ÷ÀÓ");
        if (unit.curStack > 0)
        {
            unit.animator.SetBool("Stack", true);
            unit.animator.SetBool("Move", true);
        }
        else
        {
            unit.animator.SetBool("Stack", false);
            unit.animator.SetBool("Move", true);
        }

        float x = -joystick.Vertical;
        float z = joystick.Horizontal;

        Debug.Log($"x:{x}, z:{z}, speed:{speed}, dt:{Time.deltaTime}");

        moveVector = new Vector3(x, 0, z) * speed * Time.deltaTime;

        Debug.Log($"moveVector:{moveVector}, sqr:{moveVector.sqrMagnitude}");
        rb.MovePosition(rb.position + moveVector);

        if (moveVector.sqrMagnitude == 0)
            return;

        Quaternion dirQuat = Quaternion.LookRotation(moveVector);
        pivotTransform.localRotation = Quaternion.Slerp(pivotTransform.rotation, dirQuat, 0.3f);
    }

    public  void ReadInput()
    {
        moveInput.x = -joystick.Vertical;
        moveInput.y = joystick.Horizontal;
    }
}
