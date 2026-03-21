using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 40f; // Бос б'є сильніше
    public float attackRange = 1.5f; // Бос має більший радіус атаки
    public float attackCooldown = 1.5f; // Бос б'є рідше, але болючіше
    
    [Header("References")]
    public Transform attackPoint; // Точка, з якої йде удар
    public LayerMask playerLayer; // Шар, на якому знаходиться гравець

    private float lastAttackTime;

    void Update()
    {
        // Якщо кулдаун минув, перевіряємо чи можна атакувати
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // Перевіряємо, чи гравець у радіусі атаки
            Collider2D playerInRange = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);

            if (playerInRange != null)
            {
                PerformAttack(playerInRange);
            }
        }
    }

    void PerformAttack(Collider2D player)
    {
        lastAttackTime = Time.time;
        
        // Знаходимо скрипт здоров'я гравця (наприклад, PlayerHealth)
        // Припускаємо, що у гравця є метод TakeDamage(float damage)
        var playerHealth = player.GetComponent<PlayerHealth>(); 
        if (playerHealth != null)
        {
            playerHealth.TakeDamage((int)attackDamage);
            Debug.Log("Бос атакував гравця!");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}