using UnityEngine;

public class BossCombat : MonoBehaviour
{
    public enum GameType { Platformer, TopDown }
    public enum BossType { Melee, Ranged } // Новий вибір типу боса

    [Header("General Settings")]
    public GameType gameType = GameType.Platformer;
    public BossType bossType = BossType.Melee;
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;    // Дистанція зупинки
    public float detectRange = 12f;      // Дистанція зору

    [Header("Attack Settings")]
    public float attackRange = 2.5f;     // Радіус для удару або дистанція пострілу
    public int attackDamage = 20;
    public float attackCooldown = 2f;

    [Header("Ranged Settings (Тільки для Ranged)")]
    public GameObject projectilePrefab;  // Префаб стріли/магії
    public Transform firePoint;          // Точка вильоту снаряда
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
        if (player == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 1. Перевірка Агро (зір або провокація шкодою)
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

    // Викликається зі скрипта здоров'я (TakeDamage)
    public void OnDamageReceived()
    {
        if (!isAggroedByDamage)
        {
            Debug.Log("<color=red>Боса спровоковано шкодою здалеку!</color>");
            isAggroedByDamage = true;
        }
    }

    void Attack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        if (anim != null) anim.SetTrigger("attack");

        // Викликаємо метод атаки залежно від типу
        if (bossType == BossType.Melee)
        {
            Invoke("ApplyMeleeDamage", 0.5f); // Ближній бій
        }
        else
        {
            Invoke("ShootProjectile", 0.5f); // Дальній бій
        }

        Invoke("ResetAttackFlag", 1.0f);
    }

    // --- ЛОГІКА БЛИЖНЬОГО БОЮ ---
    void ApplyMeleeDamage()
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hitPlayer != null)
        {
            var health = hitPlayer.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(attackDamage);
        }
    }

    // --- ЛОГІКА ДАЛЬНЬОГО БОЮ ---
    void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // Створюємо снаряд
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Отримуємо скрипт снаряда
        Projectile projScript = projectileObj.GetComponent<Projectile>();

        if (projScript != null)
        {
            // Розраховуємо напрямок до гравця в момент пострілу
            Vector2 direction = (player.position - firePoint.position).normalized;

            // Запускаємо його!
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

        if (anim != null) anim.SetBool("isRunning", true);
    }

    void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("isRunning", false);
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