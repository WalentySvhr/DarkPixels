using UnityEngine;
using System.Collections;

public class NPCPatrol : MonoBehaviour
{
    [Header("Зона руху")]
    [Tooltip("Можна призначити будь-який Collider2D (Polygon, Box, Circle)")]
    public Collider2D patrolZone;

    [Header("Налаштування руху")]
    public float moveSpeed = 2f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("Налаштування застрягання")]
    [Tooltip("Скільки секунд NPC буде штовхати стіну перед тим як здатися")]
    public float timeToGiveUp = 1.5f;

    [Header("Анімація")]
    public string speedParameterName = "Speed";

    private Vector2 targetPosition;
    private bool isTalking = false;
    private bool isTouchingSomething = false;
    private float stuckTimer = 0f;

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
                isTouchingSomething = false;
                stuckTimer = 0f;
                targetPosition = GetRandomPointInBounds();

                if (anim != null) anim.SetFloat(speedParameterName, 1f);

                while (Vector2.Distance(transform.position, targetPosition) > 0.2f)
                {
                    if (isTalking)
                    {
                        if (anim != null) anim.SetFloat(speedParameterName, 0f);
                        yield return new WaitUntil(() => !isTalking);

                        if (Vector2.Distance(transform.position, targetPosition) > 0.2f)
                        {
                            if (anim != null) anim.SetFloat(speedParameterName, 1f);
                            FlipSprite(targetPosition.x);
                        }
                    }

                    if (!isTalking)
                    {
                        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
                        rb.MovePosition(newPos);
                        FlipSprite(targetPosition.x);

                        if (isTouchingSomething)
                        {
                            stuckTimer += Time.deltaTime;
                            if (stuckTimer >= timeToGiveUp) break;
                        }
                        else
                        {
                            stuckTimer = 0f;
                        }
                    }
                    yield return null;
                }

                if (anim != null) anim.SetFloat(speedParameterName, 0f);
                yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
            }
            else
            {
                yield return null;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            isTouchingSomething = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            isTouchingSomething = false;
    }

    void FlipSprite(float targetX)
    {
        float scaleX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(targetX > transform.position.x ? scaleX : -scaleX, transform.localScale.y, transform.localScale.z);
    }

    // --- ОНОВЛЕНИЙ МЕТОД ГЕНЕРАЦІЇ ТОЧКИ ---
    Vector2 GetRandomPointInBounds()
    {
        Bounds bounds = patrolZone.bounds;
        Vector2 randomPoint;
        int attempts = 0;

        do
        {
            randomPoint = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
            attempts++;

            // Якщо за 50 спроб не знайшли точку всередині (наприклад, дуже вузький полігон),
            // повертаємо поточну позицію, щоб NPC просто постояв.
            if (attempts > 50) return transform.position;

        } while (!patrolZone.OverlapPoint(randomPoint));

        return randomPoint;
    }

    public void StartInteraction()
    {
        isTalking = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetFloat(speedParameterName, 0f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) FlipSprite(player.transform.position.x);
    }

    public void StopInteraction()
    {
        isTalking = false;
    }
}