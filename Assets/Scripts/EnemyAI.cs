using UnityEngine;
using System.Collections; // Обов'язково додай це на самому початку скрипту!

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float checkRadius = 5f;
    public float attackRange = 1.2f;    // Дистанція для запуску анімації
    public float stopDistance = 0.8f;   // Дистанція фізичної зупинки

    [Header("Attack Settings")]
    public int damage = 10;             // Шкода гравцеві
    public float attackCooldown = 1.5f; // Пауза між ударами
    private float nextAttackTime = 0f;

    [Header("Settings")]
    public bool isAggroedByDamage = false;
    public bool spriteFacingLeft = false;

    [Header("References")]
    public Transform hpBarTransform;
    private Animator anim;
    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);

        // 1. ЛОГІКА РУХУ
        if (distance > stopDistance && (isAggroedByDamage || distance <= checkRadius))
        {
            moveDirection = (target.position - transform.position).normalized;
            HandleFlip();
        }
        else
        {
            moveDirection = Vector2.zero;
        }

        // 2. ЛОГІКА АТАКИ (Анімація + Нанесення шкоди)
        if (distance <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                TriggerAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        // 3. ОНОВЛЕННЯ АНІМАТОРУ (Speed)
        if (anim != null)
        {
            anim.SetFloat("Speed", moveDirection.magnitude);
        }
    }

    void TriggerAttack()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attack"); // Запускаємо анімацію миттєво
        }

        // Запускаємо окремий процес для нанесення шкоди із затримкою
        StartCoroutine(AttackDelay());
    }

    IEnumerator AttackDelay()
    {
        // Підбери цей час (наприклад, 0.2 або 0.5 секунди), 
        // щоб урон знімався саме в момент візуального удару сокирою
        yield return new WaitForSeconds(0.3f);

        if (target != null)
        {
            // Перевіряємо дистанцію ще раз (гравець міг встигнути відбігти)
            float distance = Vector2.Distance(transform.position, target.position);

            if (distance <= attackRange)
            {
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    Debug.Log("Влучання після замаху! Наносимо " + damage + " шкоди!");
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }
    void FixedUpdate()
    {
        if (moveDirection != Vector2.zero)
        {
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
        }
    }

    void HandleFlip()
    {
        float direction = target.position.x - transform.position.x;
        if (Mathf.Abs(direction) > 0.1f)
        {
            float scaleX = (direction > 0) ? 1 : -1;
            if (spriteFacingLeft) scaleX *= -1;
            transform.localScale = new Vector3(scaleX, 1, 1);

            if (hpBarTransform != null)
                hpBarTransform.localScale = new Vector3(scaleX, 1, 1);
        }
    }
}