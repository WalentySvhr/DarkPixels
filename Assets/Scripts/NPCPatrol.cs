using UnityEngine;
using System.Collections;

public class NPCPatrol : MonoBehaviour
{
    [Header("Зона руху")]
    [Tooltip("BoxCollider2D, який визначає межі прогулянки")]
    public BoxCollider2D patrolZone;

    [Header("Налаштування руху")]
    public float moveSpeed = 2f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("Анімація")]
    [Tooltip("Назва параметра швидкості в Animator (зазвичай Speed або isWalking)")]
    public string speedParameterName = "Speed";

    private Vector2 targetPosition;
    private bool isTalking = false;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
                // 1. Обираємо нову ціль
                targetPosition = GetRandomPointInBounds();

                // 2. Починаємо рух
                if (anim != null) anim.SetFloat(speedParameterName, 1f);

                while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
                {
                    // Якщо під час руху почалася розмова — чекаємо
                    if (isTalking)
                    {
                        if (anim != null) anim.SetFloat(speedParameterName, 0f);
                        yield return new WaitUntil(() => !isTalking);
                        if (anim != null) anim.SetFloat(speedParameterName, 1f);
                    }

                    transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    FlipSprite(targetPosition.x);
                    yield return null;
                }

                // 3. Зупинка та очікування
                if (anim != null) anim.SetFloat(speedParameterName, 0f);
                float wait = Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(wait);
            }
            yield return null;
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

    // Методи для зупинки NPC під час діалогу
    public void StartInteraction() => isTalking = true;
    public void StopInteraction() => isTalking = false;
}