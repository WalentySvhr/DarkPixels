using UnityEngine;
using System.Collections;

public class BossCombat : MonoBehaviour
{
    public enum GameType { Platformer, TopDown }
    public enum BossType { Melee, Ranged }

    [Header("General Settings")]
    public GameType gameType = GameType.Platformer;
    public BossType bossType = BossType.Melee;
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;
    public float detectRange = 12f;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public int attackDamage = 20;
    public float attackCooldown = 2f;

    [Header("Hit Settings")]
    [Tooltip("Час оглушення боса при отриманні удару (щоб програлася анімація)")]
    public float hitStunDuration = 0.3f;

    [Header("Ranged Settings (Тільки для Ranged)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;

    [Header("References")]
    public Transform player;
    public LayerMask playerLayer;

    private float nextAttackTime;
    private Animator anim;
    private Rigidbody2D rb;
    private bool facingRight = true;
    private bool isAttacking = false;
    private bool isAggroedByDamage = false;
    private bool isStunned = false;

    // === НОВЕ: Зберігає напрямок для плавного руху у FixedUpdate ===
    private Vector2 moveDirection;

    // Хеш-коди параметрів для оптимізації
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int attackHash = Animator.StringToHash("Attack");

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null || isAttacking || isStunned)
        {
            moveDirection = Vector2.zero; // Якщо бос не може йти, обнуляємо напрямок
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // 1. Перевірка Агро
        if (distance > detectRange && !isAggroedByDamage)
        {
            SetMovementState(Vector2.zero);
            return;
        }

        LookAtPlayer();

        // 2. Логіка: Атакувати чи Наздоганяти
        if (distance <= attackRange)
        {
            SetMovementState(Vector2.zero);
            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
        else if (distance > stopDistance)
        {
            CalculateMovementDirection();
        }
        else
        {
            SetMovementState(Vector2.zero);
        }
    }

    void FixedUpdate()
    {
        // === ВИПРАВЛЕННЯ: Рух через швидкість (velocity) ===
        // Виконуємо рух тільки якщо бос не оглушений і не атакує
        if (!isStunned && !isAttacking)
        {
            if (gameType == GameType.Platformer)
            {
                // Для платформера керуємо лише X, зберігаючи гравітацію (швидкість падіння по Y)
                rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                // Для TopDown рухаємося по обох осях
                rb.linearVelocity = moveDirection * moveSpeed;
            }
        }
    }

    public void OnDamageReceived()
    {
        if (!isAggroedByDamage)
        {
            Debug.Log("<color=red>Боса спровоковано шкодою!</color>");
            isAggroedByDamage = true;
        }

        if (!isStunned)
        {
            StartCoroutine(HitStunRoutine());
        }
    }

    private IEnumerator HitStunRoutine()
    {
        isStunned = true;

        // Вимикаємо анімацію бігу
        if (anim != null) anim.SetFloat(speedHash, 0f);

        if (isAttacking)
        {
            CancelInvoke(nameof(ApplyMeleeDamage));
            CancelInvoke(nameof(ShootProjectile));
            CancelInvoke(nameof(ResetAttackFlag));
            isAttacking = false;
        }

        // Чекаємо завершення оглушення (у цей час бос летить від фізичного імпульсу відкидання)
        yield return new WaitForSeconds(hitStunDuration);

        // === ВИПРАВЛЕННЯ: Зупиняємо ковзання після відкидання ===
        if (rb != null)
        {
            if (gameType == GameType.Platformer)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Не чіпаємо гравітацію
            else
                rb.linearVelocity = Vector2.zero;
        }

        isStunned = false;
    }

    void Attack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        moveDirection = Vector2.zero;

        // Повна зупинка боса під час атаки (з урахуванням типу гри)
        if (rb != null)
        {
            if (gameType == GameType.Platformer) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            else rb.linearVelocity = Vector2.zero;
        }

        if (anim != null) anim.SetTrigger(attackHash);

        if (bossType == BossType.Melee)
        {
            Invoke(nameof(ApplyMeleeDamage), 0.5f);
        }
        else
        {
            Invoke(nameof(ShootProjectile), 0.5f);
        }

        Invoke(nameof(ResetAttackFlag), 1.0f);
    }

    void ApplyMeleeDamage()
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hitPlayer != null)
        {
            var health = hitPlayer.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(attackDamage);
        }
    }

    void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projScript = projectileObj.GetComponent<Projectile>();

        if (projScript != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            projScript.Launch(direction, projectileSpeed);
        }
    }

    // Допоміжний метод: вираховує напрямок руху
    void CalculateMovementDirection()
    {
        Vector2 targetPosition;
        if (gameType == GameType.Platformer)
            targetPosition = new Vector2(player.position.x, rb.position.y);
        else
            targetPosition = player.position;

        moveDirection = (targetPosition - (Vector2)transform.position).normalized;

        if (anim != null) anim.SetFloat(speedHash, 1f);
    }

    // Допоміжний метод: встановлює нульовий напрямок
    void SetMovementState(Vector2 dir)
    {
        moveDirection = dir;
        if (anim != null) anim.SetFloat(speedHash, 0f);
    }

    void LookAtPlayer()
    {
        float diff = player.position.x - transform.position.x;
        if (diff > 0.1f && !facingRight) Flip();
        else if (diff < -0.1f && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void ResetAttackFlag() => isAttacking = false;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}