using UnityEngine;

public class LootPhysics : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float minForce = 4f;
    public float maxForce = 7f;
    public float linearDamping = 3f; // Щоб плавно зупинялися

    public void ApplyExplosion(GameObject item)
    {
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 1. Налаштування фізики для Top-Down
            rb.gravityScale = 0f;
            rb.linearDamping = linearDamping;

            // 2. Випадковий напрямок (360 градусів)
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // 3. Випадкова сила
            float force = Random.Range(minForce, maxForce);

            // 4. Імпульс та випадкове крутіння
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-20f, 20f), ForceMode2D.Impulse);
        }
    }
}