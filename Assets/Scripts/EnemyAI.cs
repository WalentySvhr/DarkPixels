using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Returning, Fleeing, Hit }
    public EnemyState currentState = EnemyState.Idle;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float fleeSpeed = 3.5f;
    public float checkRadius = 5f;
    public float attackRange = 1.2f;
    public float stopDistance = 0.8f;

    [Header("Hit Settings")]
    public float hitStunDuration = 0.3f; // Скільки секунд моб стоїть на місці при ударі

    [Header("Flee Settings")]
    public bool canFlee = false;
    public float fleeDuration = 2f;
    [Range(0, 100)]
    public float fleeChancePercent = 15f; // Шанс у відсотках (наприклад, 15)
    private float currentFleeTimer = 0f;
    private bool hasAttemptedToFlee = false; // Блокувальник повторних спроб

    [Header("Aggro Settings")]
    public float loseAggroDistance = 8f;
    public float loseAggroTime = 2f;
    private float currentLoseAggroTimer = 0f;
    private Vector2 startPosition;

    [Header("Attack Settings")]
    public int damage = 10;
    public float attackCooldown = 1.5f;
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
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    // Метод викликається при отриманні шкоди (наприклад, зі скрипта EnemyHealth)
    public void OnTakeDamage()
    {
        isAggroedByDamage = true;

        // Перериваємо поточну дію і переходимо в стан Hit (запускаємо корутину оглушення)
        if (currentState != EnemyState.Hit)
        {
            StartCoroutine(HitStunRoutine());
        }
    }

    // --- НОВЕ: Корутина для паузи під час удару ---
    private IEnumerator HitStunRoutine()
    {
        // Перемикаємось у стан Hit і зупиняємось
        currentState = EnemyState.Hit;
        moveDirection = Vector2.zero;
        if (anim != null) anim.SetFloat("Speed", 0f); // Зупиняємо анімацію бігу

        // Чекаємо, поки програється анімація отримання шкоди
        yield return new WaitForSeconds(hitStunDuration);

        // --- ПЕРЕВІРКА НА ВТЕЧУ (відбувається ПІСЛЯ того, як моб оговтався) ---
        if (canFlee && !hasAttemptedToFlee)
        {
            hasAttemptedToFlee = true; // Помічаємо, що спроба використана

            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= fleeChancePercent)
            {
                currentState = EnemyState.Fleeing;
                currentFleeTimer = fleeDuration;
                Debug.Log($"Ворог злякався ({randomRoll:F1}%) і тікає!");
                yield break; // Виходимо з корутини, бо він тепер тікає
            }
            else
            {
                Debug.Log("Ворог вирішив битися до кінця (шанс втечі не спрацював).");
            }
        }

        // Якщо моб не втік, він продовжує переслідувати гравця
        currentState = EnemyState.Chasing;
    }

    void Update()
    {
        if (target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        switch (currentState)
        {
            // --- НОВЕ: Обробка стану Hit в Update ---
            case EnemyState.Hit:
                // Моб оглушений, нічого не робимо (стоїмо на місці)
                moveDirection = Vector2.zero;
                break;

            case EnemyState.Idle:
                moveDirection = Vector2.zero;
                if (distanceToPlayer <= checkRadius || isAggroedByDamage)
                {
                    currentState = EnemyState.Chasing;
                    currentLoseAggroTimer = 0f;
                }
                break;

            case EnemyState.Fleeing:
                currentFleeTimer -= Time.deltaTime;
                if (currentFleeTimer <= 0)
                {
                    currentState = EnemyState.Chasing;
                }
                else
                {
                    moveDirection = (transform.position - target.position).normalized;
                    HandleFlip(transform.position.x + moveDirection.x);
                }
                break;

            case EnemyState.Chasing:
                // Логіка втрати агро
                if (distanceToPlayer > loseAggroDistance)
                {
                    currentLoseAggroTimer += Time.deltaTime;
                    if (currentLoseAggroTimer >= loseAggroTime)
                    {
                        ResetCombat(); // Скидаємо стан бою
                        currentState = EnemyState.Returning;
                        break;
                    }
                }
                else
                {
                    currentLoseAggroTimer = 0f;
                }

                // Рух до гравця
                if (distanceToPlayer > stopDistance)
                {
                    moveDirection = (target.position - transform.position).normalized;
                    HandleFlip(target.position.x);
                }
                else
                {
                    moveDirection = Vector2.zero;
                }

                // Атака
                if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
                {
                    TriggerAttack();
                    nextAttackTime = Time.time + attackCooldown;
                }
                break;

            case EnemyState.Returning:
                float distanceToStart = Vector2.Distance(transform.position, startPosition);
                if (distanceToStart > 0.1f)
                {
                    moveDirection = (startPosition - (Vector2)transform.position).normalized;
                    HandleFlip(startPosition.x);
                }
                else
                {
                    transform.position = startPosition;
                    currentState = EnemyState.Idle;
                    hasAttemptedToFlee = false; // Скидаємо можливість втечі, коли ворог повернувся в спокій
                }

                if (distanceToPlayer <= checkRadius || isAggroedByDamage)
                {
                    currentState = EnemyState.Chasing;
                }
                break;
        }

        // Оновлюємо параметр Speed в Animator (але не робимо це в стані Hit, бо ми його вже обнулили)
        if (anim != null && currentState != EnemyState.Hit)
        {
            anim.SetFloat("Speed", moveDirection.magnitude);
        }
    }

    void ResetCombat()
    {
        isAggroedByDamage = false;
        // Тут можна додати скидання hasAttemptedToFlee, 
        // якщо хочете, щоб він міг тікати знову після того як відійшов від гравця
    }

    void TriggerAttack()
    {
        if (anim != null) anim.SetTrigger("Attack");
        StartCoroutine(AttackDelay());
    }

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(0.3f);
        if (target != null && (currentState == EnemyState.Chasing || currentState == EnemyState.Fleeing))
        {
            float distance = Vector2.Distance(transform.position, target.position);
            if (distance <= attackRange)
            {
                // Припускаємо, що у гравця є скрипт PlayerHealth
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(damage);
                Debug.Log("Гравець отримав шкоду: " + damage);
            }
        }
    }

    void FixedUpdate()
    {
        if (moveDirection != Vector2.zero && currentState != EnemyState.Hit)
        {
            float currentSpeed = (currentState == EnemyState.Fleeing) ? fleeSpeed : speed;
            rb.MovePosition(rb.position + moveDirection * currentSpeed * Time.fixedDeltaTime);
        }
    }

    void HandleFlip(float targetPosX)
    {
        float direction = targetPosX - transform.position.x;
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