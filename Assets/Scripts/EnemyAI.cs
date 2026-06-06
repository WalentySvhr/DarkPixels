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

    // === НАЛАШТУВАННЯ ОПТИМІЗАЦІЇ ДЛЯ ТЕЛЕФОНУ ===
    [Header("Mobile Optimization")]
    [SerializeField] private float cullDistance = 15f;      // Дистанція, на якій моб "засинає"
    [SerializeField] private float cullCheckInterval = 0.5f; // Як часто перевіряти відстань (сек)
    private float cullTimer = 0f;
    private bool isCulled = false;

    [Header("References")]
    public Transform hpBarTransform;
    private Animator anim;
    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;
    private Collider2D myCollider;
    private SpriteRenderer myRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();
        myRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;

        // Рандомізуємо старт таймера, щоб моби не робили перевірку в один і той самий кадр
        cullTimer = Random.Range(0f, cullCheckInterval);
    }

    public void OnTakeDamage()
    {
        isAggroedByDamage = true;

        if (currentState != EnemyState.Hit)
        {
            StartCoroutine(HitStunRoutine());
        }
    }

    private IEnumerator HitStunRoutine()
    {
        currentState = EnemyState.Hit;
        moveDirection = Vector2.zero;
        if (anim != null) anim.SetFloat("Speed", 0f);

        // Чекаємо, поки моб оглушений і летить від відкидання
        yield return new WaitForSeconds(hitStunDuration);

        // === ВИПРАВЛЕННЯ: Скидаємо залишкову фізичну швидкість від відкидання, 
        // щоб моб не продовжував ковзати або дьоргатись ===
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (canFlee && !hasAttemptedToFlee)
        {
            hasAttemptedToFlee = true;

            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= fleeChancePercent)
            {
                currentState = EnemyState.Fleeing;
                currentFleeTimer = fleeDuration;
                Debug.Log($"Ворог злякався ({randomRoll:F1}%) і тікає!");
                yield break;
            }
        }

        currentState = EnemyState.Chasing;
    }

    void Update()
    {
        if (target == null) return;

        // --- БЛОК ОПТИМІЗАЦІЇ ---
        cullTimer += Time.deltaTime;
        if (cullTimer >= cullCheckInterval)
        {
            cullTimer = 0f;
            float distanceToPlayer = Vector2.Distance(transform.position, target.transform.position);

            // Якщо ворог заагрений по шкоді, ми його НЕ ховаємо, поки він не заспокоїться
            bool newCullState = (distanceToPlayer > cullDistance) && !isAggroedByDamage;

            if (newCullState != isCulled)
            {
                isCulled = newCullState;

                // Вимикаємо візуалізацію та колайдер
                if (myCollider != null) myCollider.enabled = !isCulled;
                if (myRenderer != null) myRenderer.enabled = !isCulled;
                if (hpBarTransform != null) hpBarTransform.gameObject.SetActive(!isCulled);

                // Якщо моб заснув, зупиняємо його фізично
                if (isCulled && rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    moveDirection = Vector2.zero;
                    if (anim != null) anim.SetFloat("Speed", 0f);
                }
            }
        }

        // Якщо моб оптимізований (заснув) — повністю виходимо з Update, нічого не рахуємо!
        if (isCulled) return;
        // ------------------------

        float distanceToPlayerActual = Vector2.Distance(transform.position, target.position);

        switch (currentState)
        {
            case EnemyState.Hit:
                moveDirection = Vector2.zero;
                break;

            case EnemyState.Idle:
                moveDirection = Vector2.zero;
                if (distanceToPlayerActual <= checkRadius || isAggroedByDamage)
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
                if (distanceToPlayerActual > loseAggroDistance)
                {
                    currentLoseAggroTimer += Time.deltaTime;
                    if (currentLoseAggroTimer >= loseAggroTime)
                    {
                        ResetCombat();
                        currentState = EnemyState.Returning;
                        break;
                    }
                }
                else
                {
                    currentLoseAggroTimer = 0f;
                }

                if (distanceToPlayerActual > stopDistance)
                {
                    moveDirection = (target.position - transform.position).normalized;
                    HandleFlip(target.position.x);
                }
                else
                {
                    moveDirection = Vector2.zero;
                }

                if (distanceToPlayerActual <= attackRange && Time.time >= nextAttackTime)
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
                    hasAttemptedToFlee = false;
                }

                if (distanceToPlayerActual <= checkRadius || isAggroedByDamage)
                {
                    currentState = EnemyState.Chasing;
                }
                break;
        }

        if (anim != null && currentState != EnemyState.Hit)
        {
            anim.SetFloat("Speed", moveDirection.magnitude);
        }
    }

    void ResetCombat()
    {
        isAggroedByDamage = false;
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
                Debug.Log("Гравець отримав шкоду: " + damage);
            }
        }
    }

    void FixedUpdate()
    {
        // === Якщо моб за межами екрана — фізику руху взагалі не прораховуємо ===
        if (isCulled) return;

        if (currentState != EnemyState.Hit)
        {
            float currentSpeed = (currentState == EnemyState.Fleeing) ? fleeSpeed : speed;
            rb.linearVelocity = moveDirection * currentSpeed;
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