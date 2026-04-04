using UnityEngine;
using System.Collections;

public class NPCPatrol : MonoBehaviour
{
    [Header("Зона руху")]
    public BoxCollider2D patrolZone;

    [Header("Налаштування руху")]
    public float moveSpeed = 2f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("Анімація")]
    public string speedParameterName = "Speed";

    private Vector2 targetPosition;
    private bool isTalking = false;
    private bool isBlocked = false; // Прапорець, що ми вперлися в стіну
    private Animator anim;
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (patrolZone == null)
        {
            Debug.LogError($"На об'єкті {gameObject.name} не призначена зона патрулювання!");
            return;
        }

        StartCoroutine(PatrolRoutine());
    }

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (!isTalking)
            {
                isBlocked = false; // Скидаємо блокування перед новим рухом
                targetPosition = GetRandomPointInBounds();

                if (anim != null) anim.SetFloat(speedParameterName, 1f);

                // Рухаємося до цілі, поки не дійдемо АБО поки нас не заблокують
                while (Vector2.Distance(transform.position, targetPosition) > 0.2f && !isBlocked)
                {
                    if (isTalking)
                    {
                        if (anim != null) anim.SetFloat(speedParameterName, 0f);
                        yield return new WaitUntil(() => !isTalking);
                        if (anim != null) anim.SetFloat(speedParameterName, 1f);
                    }

                    // Використовуємо MovePosition для Rigidbody, це краще для фізики ніж transform.position
                    Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
                    rb.MovePosition(newPos);

                    FlipSprite(targetPosition.x);
                    yield return null;
                }

                // Якщо ми вийшли з циклу (дійшли або вдарилися) — зупиняємось
                if (anim != null) anim.SetFloat(speedParameterName, 0f);

                // Якщо була колізія, можна почекати менше, щоб швидше змінити маршрут
                float wait = isBlocked ? 1f : Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(wait);
            }
            yield return null;
        }
    }

    // Цей метод викликається автоматично Unity, коли NPC врізається в щось (стіну)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Якщо це не гравець (можна додати перевірку тегів, наприклад стін)
        if (!collision.gameObject.CompareTag("Player"))
        {
            isBlocked = true; // Кажемо корутині, що ми застрягли
        }
    }

    void FlipSprite(float targetX)
    {
        if (targetX > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    Vector2 GetRandomPointInBounds()
    {
        Bounds bounds = patrolZone.bounds;
        return new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );
    }

    public void StartInteraction() => isTalking = true;
    public void StopInteraction() => isTalking = false;
}