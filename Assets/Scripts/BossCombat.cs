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
    private bool isStunned = false; // НОВЕ: Прапорець оглушення

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
        // Якщо немає гравця, бос атакує, АБО БОС ОГЛУШЕНИЙ — нічого не робимо
        if (player == null || isAttacking || isStunned) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 1. Перевірка Агро
        if (distance > detectRange && !isAggroedByDamage)
        {
            StopMovement();
            return;
        }

        LookAtPlayer();

        // 2. Логіка: Атакувати чи Наздоганяти
        if (distance <= attackRange)
        {
            StopMovement();
            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
        else if (distance > stopDistance)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMovement();
        }
    }

    // ОНОВЛЕНО: Тепер викликає корутину оглушення
    public void OnDamageReceived()
    {
        if (!isAggroedByDamage)
        {
            Debug.Log("<color=red>Боса спровоковано шкодою!</color>");
            isAggroedByDamage = true;
        }

        // Запускаємо оглушення, тільки якщо бос ще не оглушений
        if (!isStunned)
        {
            StartCoroutine(HitStunRoutine());
        }
    }

    // НОВЕ: Корутина оглушення
    private IEnumerator HitStunRoutine()
    {
        isStunned = true;

        // Зупиняємо рух і передаємо нульову швидкість в Animator
        StopMovement();

        // Перериваємо поточну атаку (якщо бос якраз замахувався)
        // У цьому випадку скидаємо прапорець, щоб після оглушення він міг атакувати знову
        if (isAttacking)
        {
            CancelInvoke(nameof(ApplyMeleeDamage));
            CancelInvoke(nameof(ShootProjectile));
            CancelInvoke(nameof(ResetAttackFlag));
            isAttacking = false;
        }

        // Чекаємо, поки програється анімація TakeDamage
        yield return new WaitForSeconds(hitStunDuration);

        // Повертаємо боса до нормального стану
        isStunned = false;
    }

    void Attack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        // ВИКЛИК АНІМАЦІЇ АТАКИ
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

    void MoveTowardsPlayer()
    {
        Vector2 targetPosition;
        if (gameType == GameType.Platformer)
            targetPosition = new Vector2(player.position.x, rb.position.y);
        else
            targetPosition = player.position;

        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPos);

        // КЕРУВАННЯ АНІМАЦІЄЮ ХОДЬБИ
        if (anim != null)
        {
            anim.SetFloat(speedHash, 1f);
        }
    }

    void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero; // Змінено linearVelocity на velocity (стандарт для Rigidbody2D)

        // ЗУПИНКА АНІМАЦІЇ ХОДЬБИ
        if (anim != null)
        {
            anim.SetFloat(speedHash, 0f);
        }
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