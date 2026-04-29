using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Joystick joystick;

    [HideInInspector]
    public float extraSpeedMultiplier = 0f; // Сюди скрипт екіпіровки передаватиме бонус (наприклад, 0.2 для +20%)

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

        // --- МАГІЯ ТУТ: Рахуємо фінальну швидкість ---
        // Якщо бонус 0, то finalSpeed буде такою ж, як moveSpeed.
        // Якщо бонус 0.5 (амулет на 50%), то finalSpeed буде в 1.5 рази більшою.
        float finalSpeed = moveSpeed * (1f + extraSpeedMultiplier);
        if (extraSpeedMultiplier != 0) Debug.Log("Швидкість змінена! Множник: " + extraSpeedMultiplier);

        // Замість moveSpeed тепер використовуємо finalSpeed
        rb.linearVelocity = new Vector2(input.x * finalSpeed, input.y * finalSpeed);

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