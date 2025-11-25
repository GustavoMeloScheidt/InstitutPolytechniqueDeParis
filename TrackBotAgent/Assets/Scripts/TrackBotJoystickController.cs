using UnityEngine;

public class TrackBotJoystickController : MonoBehaviour
{
    public JoyStick joystick;
    public float moveSpeed = 3f;
    public float turnSpeed = 8f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector2 dir = joystick.GetDirection();

        if (dir.magnitude > 0.1f)
        {
            // Converter direção 2D → direçao do mundo
            Vector3 moveDir = new Vector3(dir.x, 0, dir.y);

            // Rotaciona suavemente para o joystick
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.fixedDeltaTime * turnSpeed);

            // Move na direção apontada
            rb.MovePosition(transform.position + moveDir.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
