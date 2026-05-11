using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Joystick joystick;

    [Header("Бонуси від екіпіровки")]
    [HideInInspector]
    public float extraSpeedMultiplier = 0f;     // Бонус від амулета
    [HideInInspector]
    public float extraRingSpeedMultiplier = 0f; // Бонус від кільця

    // --- НОВЕ: Прапорець оглушення ---
    [HideInInspector]
    public bool isStunned = false;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // --- НОВЕ: Якщо гравець оглушений або мертвий, повністю зупиняємо його ---
        if (isStunned)
        {
            rb.linearVelocity = Vector2.zero; // Гасимо інерцію
            if (anim != null) anim.SetFloat("Speed", 0f); // Зупиняємо "місячну ходу"
            return; // Перериваємо виконання коду, щоб джойстик не працював
        }

        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);

        if (input.magnitude > 1f)
            input = input.normalized;

        // Рахуємо фінальну швидкість
        float totalSpeedBonus = extraSpeedMultiplier + extraRingSpeedMultiplier;
        float finalSpeed = moveSpeed * (1f + totalSpeedBonus);

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