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
    public float hitStunDuration = 0.3f;

    [Header("Flee Settings")]
    public bool canFlee = false;
    public float fleeDuration = 2f;
    [Range(0, 100)]
    public float fleeChancePercent = 15f;
    private float currentFleeTimer = 0f;
    private bool hasAttemptedToFlee = false;

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

    [Header("Mobile Optimization")]
    [SerializeField] private float cullDistance = 15f;
    [SerializeField] private float cullCheckInterval = 0.5f;
    private float cullTimer = 0f;
    private bool isCulled = false;

    [Header("References")]
    public Transform hpBarTransform;
    private Animator anim;
    private Rigidbody2D rb;
    private Transform target;
    private PlayerHealth targetHealth; // Закешований скрипт здоров'я гравця
    private Vector2 moveDirection;
    private Collider2D myCollider;
    private SpriteRenderer myRenderer;

    // === ЗМІННІ ДЛЯ ОПТИМІЗАЦІЇ ПРЕДСТАВЛЕННЯ ТА ОБЧИСЛЕНЬ ===
    private float checkRadiusSqr;
    private float attackRangeSqr;
    private float stopDistanceSqr;
    private float loseAggroDistanceSqr;
    private float cullDistanceSqr;

    private WaitForSeconds hitStunWait;
    private WaitForSeconds attackDelayWait;

    private bool hasAnimator;
    private bool hasHpBar;
    private bool hasRigidbody;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();
        myRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        // Попередній підрахунок квадратів відстаней (sqrMagnitude працює без квадратного кореня!)
        checkRadiusSqr = checkRadius * checkRadius;
        attackRangeSqr = attackRange * attackRange;
        stopDistanceSqr = stopDistance * stopDistance;
        loseAggroDistanceSqr = loseAggroDistance * loseAggroDistance;
        cullDistanceSqr = cullDistance * cullDistance;

        // Кешуємо WaitForSeconds для корутин
        hitStunWait = new WaitForSeconds(hitStunDuration);
        attackDelayWait = new WaitForSeconds(0.3f);

        // Прапорці для швидкої перевірки на null
        hasAnimator = anim != null;
        hasHpBar = hpBarTransform != null;
        hasRigidbody = rb != null;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            targetHealth = playerObj.GetComponent<PlayerHealth>(); // Кешуємо здоров'я ОДИН раз
        }

        cullTimer = Random.Range(0f, cullCheckInterval);

        if (TowerManager.Instance != null && TowerManager.Instance.IsTowerRunActive)
        {
            float damageMultiplier = TowerManager.Instance.IsBossFloor()
                ? TowerManager.Instance.GetBossDamageMultiplier()
                : TowerManager.Instance.GetEnemyDamageMultiplier();

            damage = Mathf.RoundToInt(damage * damageMultiplier);
        }
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
        if (hasAnimator) anim.SetFloat("Speed", 0f);

        yield return hitStunWait; // Оптимізовано: нуль сміття

        if (hasRigidbody) rb.linearVelocity = Vector2.zero;

        if (canFlee && !hasAttemptedToFlee)
        {
            hasAttemptedToFlee = true;
            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= fleeChancePercent)
            {
                currentState = EnemyState.Fleeing;
                currentFleeTimer = fleeDuration;
                yield break;
            }
        }

        currentState = EnemyState.Chasing;
    }

    void Update()
    {
        if (target == null) return;

        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.position;

        // --- БЛОК ОПТИМІЗАЦІЇ CULLING ---
        cullTimer += Time.deltaTime;
        if (cullTimer >= cullCheckInterval)
        {
            cullTimer = 0f;
            // Рахуємо квадрат відстані (це набагато швидше за Vector2.Distance)
            float sqrDistanceToPlayer = (currentPos - targetPos).sqrMagnitude;

            bool newCullState = (sqrDistanceToPlayer > cullDistanceSqr) && !isAggroedByDamage;

            if (newCullState != isCulled)
            {
                isCulled = newCullState;

                if (myCollider != null) myCollider.enabled = !isCulled;
                if (myRenderer != null) myRenderer.enabled = !isCulled;
                if (hasHpBar) hpBarTransform.gameObject.SetActive(!isCulled);

                if (isCulled && hasRigidbody)
                {
                    rb.linearVelocity = Vector2.zero;
                    moveDirection = Vector2.zero;
                    if (hasAnimator) anim.SetFloat("Speed", 0f);
                }
            }
        }

        if (isCulled) return;
        // --------------------------------

        // Вектор напрямку до гравця та квадрат фактичної відстані
        Vector2 toTarget = targetPos - currentPos;
        float sqrDistanceToPlayerActual = toTarget.sqrMagnitude;

        switch (currentState)
        {
            case EnemyState.Hit:
                moveDirection = Vector2.zero;
                break;

            case EnemyState.Idle:
                moveDirection = Vector2.zero;
                if (sqrDistanceToPlayerActual <= checkRadiusSqr || isAggroedByDamage)
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
                    moveDirection = (-toTarget).normalized;
                    HandleFlip(currentPos.x + moveDirection.x, currentPos.x);
                }
                break;

            case EnemyState.Chasing:
                if (sqrDistanceToPlayerActual > loseAggroDistanceSqr)
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

                if (sqrDistanceToPlayerActual > stopDistanceSqr)
                {
                    moveDirection = toTarget.normalized;
                    HandleFlip(targetPos.x, currentPos.x);
                }
                else
                {
                    moveDirection = Vector2.zero;
                }

                if (sqrDistanceToPlayerActual <= attackRangeSqr && Time.time >= nextAttackTime)
                {
                    TriggerAttack();
                    nextAttackTime = Time.time + attackCooldown;
                }
                break;

            case EnemyState.Returning:
                Vector2 toStart = startPosition - currentPos;
                float sqrDistanceToStart = toStart.sqrMagnitude;

                if (sqrDistanceToStart > 0.01f) // 0.1f у квадраті це 0.01f
                {
                    moveDirection = toStart.normalized;
                    HandleFlip(startPosition.x, currentPos.x);
                }
                else
                {
                    transform.position = startPosition;
                    currentState = EnemyState.Idle;
                    hasAttemptedToFlee = false;
                }

                if (sqrDistanceToPlayerActual <= checkRadiusSqr || isAggroedByDamage)
                {
                    currentState = EnemyState.Chasing;
                }
                break;
        }

        if (hasAnimator && currentState != EnemyState.Hit)
        {
            anim.SetFloat("Speed", moveDirection.sqrMagnitude); // Швидше, ніж .magnitude
        }
    }

    void ResetCombat()
    {
        isAggroedByDamage = false;
    }

    void TriggerAttack()
    {
        if (hasAnimator) anim.SetTrigger("Attack");
        StartCoroutine(AttackDelay());
    }

    IEnumerator AttackDelay()
    {
        yield return attackDelayWait; // Оптимізовано: нуль сміття

        if (target != null && (currentState == EnemyState.Chasing || currentState == EnemyState.Fleeing))
        {
            float sqrDistance = (target.position - transform.position).sqrMagnitude;
            if (sqrDistance <= attackRangeSqr)
            {
                // Використовуємо закешоване посилання на здоров'я гравця безGetComponent
                if (targetHealth != null) targetHealth.TakeDamage(damage);
            }
        }
    }

    void FixedUpdate()
    {
        if (isCulled) return;

        if (currentState != EnemyState.Hit)
        {
            float currentSpeed = (currentState == EnemyState.Fleeing) ? fleeSpeed : speed;
            if (hasRigidbody) rb.linearVelocity = moveDirection * currentSpeed;
        }
    }

    // Передаємо також поточний X моба, щоб не викликати повторно transform.position.x всередині методу
    void HandleFlip(float targetPosX, float currentPosX)
    {
        float direction = targetPosX - currentPosX;
        if (Mathf.Abs(direction) > 0.1f)
        {
            float scaleX = (direction > 0) ? 1 : -1;
            if (spriteFacingLeft) scaleX *= -1;

            // На телефонах зміна localScale щокадру може бути важкою, тому міняємо тільки якщо вона реально інша
            Vector3 currentScale = transform.localScale;
            if (Mathf.Abs(currentScale.x - scaleX) > 0.01f)
            {
                transform.localScale = new Vector3(scaleX, 1, 1);
                if (hasHpBar) hpBarTransform.localScale = new Vector3(scaleX, 1, 1);
            }
        }
    }
}