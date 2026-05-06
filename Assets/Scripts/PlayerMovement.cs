using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Joystick joystick;

    [Header("Бонуси від екіпіровки")]
    [HideInInspector]
    public float extraSpeedMultiplier = 0f;     // Бонус від амулета (наприклад, 0.2 для +20%)
    [HideInInspector]
    public float extraRingSpeedMultiplier = 0f; // НОВЕ: Бонус від кільця

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
        // Підсумовуємо всі бонуси швидкості
        float totalSpeedBonus = extraSpeedMultiplier + extraRingSpeedMultiplier;

        // Розрахунок: Базова швидкість * (1 + сума всіх бонусів)
        float finalSpeed = moveSpeed * (1f + totalSpeedBonus);

        // Debug лог для перевірки (можна прибрати після тестів)
        if (totalSpeedBonus != 0)
        {
            // Debug.Log($"Швидкість змінена! Сумарний бонус: {totalSpeedBonus * 100}% | Фінальна швидкість: {finalSpeed}");
        }

        // Використовуємо finalSpeed для руху фізичного тіла
        rb.linearVelocity = new Vector2(input.x * finalSpeed, input.y * finalSpeed);

        if (anim != null)
        {
            anim.SetFloat("Speed", input.magnitude);
        }

        // Логіка повороту персонажа
        Flip(input.x);
    }

    void Flip(float horizontalInput)
    {
        Vector3 currentScale = transform.localScale;

        if (horizontalInput > 0.1f && currentScale.x < 0 || horizontalInput < -0.1f && currentScale.x > 0)
        {
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }
    }
}