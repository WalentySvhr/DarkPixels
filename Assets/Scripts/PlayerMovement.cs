using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Joystick joystick;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);

        if (input.magnitude > 1f)
            input = input.normalized;

        // Замість прямого множення спробуй так:
        rb.linearVelocity = new Vector2(input.x * moveSpeed, input.y * moveSpeed);

        if (anim != null)
        {
            anim.SetFloat("Speed", input.magnitude);
        }

        // Логіка повороту персонажа без зміни розміру
        Flip(input.x);
    }

    void Flip(float horizontalInput)
    {
        // Отримуємо поточний масштаб з інспектора
        Vector3 currentScale = transform.localScale;

        // Якщо йдемо вправо і scale від'ємний, АБО йдемо вліво і scale додатній — розвертаємо
        if (horizontalInput > 0.1f && currentScale.x < 0 || horizontalInput < -0.1f && currentScale.x > 0)
        {
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }
    }
}