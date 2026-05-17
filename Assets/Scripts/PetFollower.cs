using UnityEngine;

public class PetFollower : MonoBehaviour
{
    [Header("Налаштування руху")]
    public Transform playerTarget;
    public float followSpeed = 4f;
    public float stopDistance = 1.5f;

    [Tooltip("Якщо гравець далі ніж ця відстань, пет миттєво телепортується до нього")]
    public float teleportDistance = 15f; // --- ДОДАНО ---

    [Header("Візуал")]
    public float hoverAmplitude = 0.2f;
    public float hoverSpeed = 3f;

    [Header("Здібності: Магніт луту")]
    public bool canMagnetLoot = true;
    public float magnetRadius = 5f;
    public float magnetSpeed = 8f;
    public string lootTag = "Loot"; // Тег предметів, які треба тягнути

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        float distance = Vector2.Distance(transform.position, playerTarget.position);

        // --- НОВЕ: Ривок повідця (Телепортація пета) ---
        if (distance > teleportDistance)
        {
            // Миттєво переміщуємо пета трохи збоку від гравця
            transform.position = playerTarget.position + new Vector3(-1f, 1f, 0f);
            return; // Зупиняємо виконання іншої логіки в цьому кадрі, щоб уникнути ривків
        }
        // ----------------------------------------------

        // 1. Рух за гравцем
        if (distance > stopDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, followSpeed * Time.deltaTime);
        }

        // 2. Розворот
        if (playerTarget.position.x > transform.position.x) spriteRenderer.flipX = false;
        else if (playerTarget.position.x < transform.position.x) spriteRenderer.flipX = true;

        // 3. Левітація (постав hoverAmplitude = 0 для наземних)
        float hoverY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude * Time.deltaTime;
        transform.position += new Vector3(0, hoverY, 0);

        // 4. Збір луту
        if (canMagnetLoot) PullLoot();
    }

    private void PullLoot()
    {
        // Шукаємо всі колайдери в радіусі
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, magnetRadius);

        foreach (Collider2D col in colliders)
        {
            // Якщо у предмета є наш тег, тягнемо його до ГРАВЦЯ
            if (col.CompareTag(lootTag))
            {
                col.transform.position = Vector2.MoveTowards(col.transform.position, playerTarget.position, magnetSpeed * Time.deltaTime);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Малює жовте коло в редакторі для зручного налаштування радіусу
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}