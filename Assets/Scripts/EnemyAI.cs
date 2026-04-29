using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Returning, Fleeing }
    public EnemyState currentState = EnemyState.Idle;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float fleeSpeed = 3.5f;
    public float checkRadius = 5f;
    public float attackRange = 1.2f;
    public float stopDistance = 0.8f;

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

    // Метод викликається при отриманні шкоди
    public void OnTakeDamage()
    {
        isAggroedByDamage = true;

        // ПЕРЕВІРКА: чи може тікати, чи він вже не тікає, і чи він ще НЕ пробував тікати в цій сутичці
        if (canFlee && currentState != EnemyState.Fleeing && !hasAttemptedToFlee)
        {
            hasAttemptedToFlee = true; // Помічаємо, що спроба використана (шанс випав лише раз)

            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= fleeChancePercent)
            {
                currentState = EnemyState.Fleeing;
                currentFleeTimer = fleeDuration;
                Debug.Log($"Ворог злякався ({randomRoll:F1}%) і тікає!");
            }
            else
            {
                Debug.Log("Ворог вирішив битися до кінця (шанс втечі не спрацював).");
            }
        }
    }

    void Update()
    {
        if (target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        switch (currentState)
        {
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

        if (anim != null)
            anim.SetFloat("Speed", moveDirection.magnitude);
    }

    // Допоміжний метод для очищення параметрів бою
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
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(damage);
            }
        }
    }

    void FixedUpdate()
    {
        if (moveDirection != Vector2.zero)
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