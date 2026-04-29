using UnityEngine;
using System.Collections;

public class ArcherEnemy : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Returning }
    public EnemyState currentState = EnemyState.Idle;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float checkRadius = 8f;
    public float stopDistance = 5f;
    public float retreatDistance = 3f;

    [Header("Aggro Settings")]
    public float loseAggroDistance = 12f;
    public float loseAggroTime = 2f;
    private float currentLoseAggroTimer = 0f;
    private Vector2 startPosition;

    [Header("Shooting Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float attackCooldown = 2f;
    public float delayBeforeArrow = 0.5f; // Час затримки до вильоту стріли (підлаштуй під анімацію)
    private float nextAttackTime = 0f;
    private bool isAttacking = false; // Чи перебуває ворог у процесі атаки

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

        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // Якщо ворог атакує (чекає вильоту стріли), ми не міняємо стан руху
        if (isAttacking) return;

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

            case EnemyState.Chasing:
                UpdateChasingState(distanceToPlayer);
                break;

            case EnemyState.Returning:
                UpdateReturningState(distanceToPlayer);
                break;
        }

        if (anim != null)
            anim.SetFloat("Speed", moveDirection.magnitude);
    }

    void FixedUpdate()
    {
        if (moveDirection != Vector2.zero && !isAttacking)
        {
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void UpdateChasingState(float distanceToPlayer)
    {
        if (distanceToPlayer > loseAggroDistance)
        {
            currentLoseAggroTimer += Time.deltaTime;
            if (currentLoseAggroTimer >= loseAggroTime)
            {
                isAggroedByDamage = false;
                currentState = EnemyState.Returning;
                return;
            }
        }
        else { currentLoseAggroTimer = 0f; }

        if (distanceToPlayer > stopDistance)
            moveDirection = (target.position - transform.position).normalized;
        else if (distanceToPlayer < retreatDistance)
            moveDirection = (transform.position - target.position).normalized;
        else
            moveDirection = Vector2.zero;

        HandleFlip(target.position.x);

        if (distanceToPlayer <= stopDistance + 1f && Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void UpdateReturningState(float distanceToPlayer)
    {
        float distanceToStart = Vector2.Distance(transform.position, startPosition);

        if (distanceToStart > 0.2f)
        {
            moveDirection = ((Vector3)startPosition - transform.position).normalized;
            HandleFlip(startPosition.x);
        }
        else
        {
            transform.position = startPosition;
            currentState = EnemyState.Idle;
            moveDirection = Vector2.zero;
        }

        if (distanceToPlayer <= checkRadius || isAggroedByDamage)
            currentState = EnemyState.Chasing;
    }

    // --- ЛОГІКА КОРУТИНИ ---
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        moveDirection = Vector2.zero; // Зупиняємо рух під час пострілу

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // Чекаємо потрібний момент анімації
        yield return new WaitForSeconds(delayBeforeArrow);

        // Викликаємо метод створення стріли
        LaunchArrow();

        // Невелика пауза після пострілу, щоб ворог не миттєво побіг
        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
    }

    public void LaunchArrow()
    {
        if (target == null || arrowPrefab == null || firePoint == null) return;

        Vector2 direction = (target.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Instantiate(arrowPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
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
            {
                Vector3 hpScale = hpBarTransform.localScale;
                hpScale.x = Mathf.Abs(hpScale.x) * (scaleX > 0 ? 1 : -1);
                hpBarTransform.localScale = hpScale;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}