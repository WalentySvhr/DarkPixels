using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 10;          // Шкода гравцеві
    public float attackSpeed = 1.0f; // Пауза між ударами (секунди)
    
    private float nextAttackTime = 0f;

    // Спрацьовує, коли ворог торкається іншого об'єкта з колайдером
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Перевіряємо, чи об'єкт, якого ми торкнулися — Гравець
        if (collision.gameObject.CompareTag("Player"))
        {
            // Перевіряємо, чи пройшло достатньо часу для наступного удару
            if (Time.time >= nextAttackTime)
            {
                AttackPlayer(collision.gameObject);
                nextAttackTime = Time.time + attackSpeed;
            }
        }
    }

    void AttackPlayer(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            Debug.Log("Ворог атакує гравця!");
            playerHealth.TakeDamage(damage);
            
            // Тут можна додати звук удару або анімацію "випаду" ворога
        }
    }
}