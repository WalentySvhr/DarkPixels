using UnityEngine;

public class BossCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 3f;      // Радіус удару
    public int attackDamage = 20;       // Скільки HP зніме
    public float attackCooldown = 2f;   // Пауза між ударами

    [Header("References")]
    public Transform player;
    public LayerMask playerLayer;       // Шар гравця (Player)
    private float nextAttackTime;
    private Animator anim;
    private bool facingRight = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        // 1. Поворот до гравця
        LookAtPlayer();

        // 2. Перевірка дистанції
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void LookAtPlayer()
    {
        float diff = player.position.x - transform.position.x;
        if (diff > 0.5f && !facingRight) Flip();
        else if (diff < -0.5f && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Attack()
    {
        // Запускаємо анімацію
        if (anim != null) anim.SetTrigger("attack");

        Debug.Log("Бос готує удар!");

        // Викликаємо нанесення шкоди через невелику затримку (або миттєво)
        Invoke("ApplyDamage", 0.5f); // 0.5с - час до моменту "контакту" в анімації
    }

    void ApplyDamage()
    {
        // Перевіряємо, чи гравець все ще в зоні ураження
        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);

        if (hitPlayer != null)
        {
            // Шукаємо скрипт здоров'я на гравці
            var health = hitPlayer.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
                Debug.Log("ГРАВЕЦЬ ОТРИМАВ ШКОДУ!");
            }
        }
    }

    // Малює коло атаки в редакторі Unity (зручно для налаштування)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}