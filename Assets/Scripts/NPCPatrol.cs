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
                // Скидаємо все перед новим маршрутом
                isTouchingSomething = false;
                stuckTimer = 0f;
                targetPosition = GetRandomPointInBounds();

                if (anim != null) anim.SetFloat(speedParameterName, 1f);

                // Головний цикл руху
                while (Vector2.Distance(transform.position, targetPosition) > 0.2f)
                {
                    // Якщо почали говорити - ставимо корутину на паузу
                    if (isTalking)
                    {
                        if (anim != null) anim.SetFloat(speedParameterName, 0f);
                        yield return new WaitUntil(() => !isTalking);

                        // Після діалогу знову вмикаємо ходьбу (якщо ще не дійшли до точки)
                        if (Vector2.Distance(transform.position, targetPosition) > 0.2f)
                        {
                            if (anim != null) anim.SetFloat(speedParameterName, 1f);
                            FlipSprite(targetPosition.x); // Повертаємо погляд назад на маршрут
                        }
                    }

                    // Рух (виконується тільки якщо не говоримо)
                    if (!isTalking)
                    {
                        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
                        rb.MovePosition(newPos);
                        FlipSprite(targetPosition.x);

                        // --- ЛОГІКА ЗАСТРЯГАННЯ ---
                        if (isTouchingSomething)
                        {
                            stuckTimer += Time.deltaTime;
                            if (stuckTimer >= timeToGiveUp)
                            {
                                // Debug.Log($"{gameObject.name}: Не можу пройти, йду в інше місце.");
                                break;
                            }
                        }
                        else
                        {
                            stuckTimer = 0f;
                        }
                    }

                    yield return null;
                }

                // Зупинка після завершення шляху або застрягання
                if (anim != null) anim.SetFloat(speedParameterName, 0f);

                float wait = Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(wait);
            }
            else
            {
                // Якщо діалог почався під час паузи між маршрутами
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

    // --- ОСЬ ТУТ ГОЛОВНІ ЗМІНИ ДЛЯ ДІАЛОГУ ---
    public void StartInteraction()
    {
        isTalking = true;

        // 1. Жорстко гасимо інерцію фізики, щоб він не ковзав
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Зупиняємо анімацію ходьби
        if (anim != null) anim.SetFloat(speedParameterName, 0f);

        // 3. Знаходимо гравця і повертаємо NPC обличчям до нього!
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            FlipSprite(player.transform.position.x);
        }
    }

    public void StopInteraction()
    {
        isTalking = false;
    }
}