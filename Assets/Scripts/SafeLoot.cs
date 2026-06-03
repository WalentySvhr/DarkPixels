using UnityEngine;
using System.Collections;

public class SafeLoot : MonoBehaviour
{
    [Header("Налаштування безпеки")]
    [Tooltip("Вибери тут шар перешкод (Obstacles), де лежать дерева")]
    public LayerMask obstacleLayer;

    [Tooltip("Радіус перевірки (приблизний розмір монетки)")]
    public float checkRadius = 0.2f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Запускаємо корутину, яка чекає повної зупинки предмета
        StartCoroutine(CheckAfterFlightRoutine());
    }

    IEnumerator CheckAfterFlightRoutine()
    {
        if (rb != null)
        {
            yield return new WaitForSeconds(0.2f); // Даємо старт для розльоту

            // Чекаємо, поки предмет повністю припинить рух
            while (rb.linearVelocity.magnitude > 0.05f)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Предмет на землі. Перевіряємо застрягання
        FixPosition();
    }

    private void FixPosition()
    {
        // 1. Перевіряємо, чи фінальна точка предмета перетинається з деревом
        if (Physics2D.OverlapCircle(transform.position, checkRadius, obstacleLayer))
        {
            // 2. Якщо ми всередині дерева, готуємо 8 напрямків для пошуку виходу
            Vector2[] searchDirections = {
                Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
                new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
            };

            float stepDistance = 0.15f; // Крок пошуку (на скільки відходимо далі з кожним колом)
            float maxSearchRange = 2.0f; // Максимальна відстань пошуку вільної трави

            // Циклічно розширюємо радіус пошуку
            for (float currentRange = stepDistance; currentRange <= maxSearchRange; currentRange += stepDistance)
            {
                foreach (Vector2 dir in searchDirections)
                {
                    // Рахуємо потенційну точку на травичці
                    Vector2 testPoint = (Vector2)transform.position + dir * currentRange;

                    // Перевіряємо, чи ця точка ВІЛЬНА від колайдера дерева
                    if (!Physics2D.OverlapCircle(testPoint, checkRadius, obstacleLayer))
                    {
                        // ЗНАЙШЛИ! Переносимо лут на чисте місце
                        transform.position = testPoint;

                        if (rb != null)
                        {
                            rb.linearVelocity = Vector2.zero; // Гасимо фізику
                        }

                        Debug.Log($"<color=cyan>[SafeLoot]</color> Предмет {gameObject.name} успішно виштовхнуто з тайлмепу на чисту траву!");
                        enabled = false;
                        return; // Виходимо з методу, задача виконана
                    }
                }
            }
        }

        // Якщо все добре і предмет на траві, просто вимикаємо скрипт для економії ресурсів
        enabled = false;
    }
}