using UnityEngine;
using System.Collections;

public class TopDownLoot : MonoBehaviour
{
    [Header("Налаштування розльоту")]
    public float minJumpHeight = 1.0f;
    public float maxJumpHeight = 2.0f;
    public float jumpDuration = 0.5f;
    public float spreadRadius = 1.5f;

    [Header("Налаштування безпеки (Проти дерев)")]
    [Tooltip("Вибери тут шар перешкод (Obstacles), де лежать твої дерева")]
    public LayerMask obstacleLayer;
    [Tooltip("Радіус перевірки (приблизний розмір монетки)")]
    public float checkRadius = 0.15f;

    [Header("Налаштування анімації (після падіння)")]
    public bool useAnimation = true;
    public float rotationSpeed = 50f;      // Швидкість обертання (градуси в сек)
    public float floatAmplitude = 0.2f;    // Висота коливання (вгору-вниз)
    public float floatSpeed = 2f;        // Швидкість коливання

    [Header("Посилання")]
    public Vector3 targetPosition;
    public Transform visualChild;

    private bool isFlying = true;

    void Start()
    {
        if (visualChild == null && transform.childCount > 0)
            visualChild = transform.GetChild(0);

        float height = Random.Range(minJumpHeight, maxJumpHeight);

        Vector3 randomDirection = Random.insideUnitCircle * spreadRadius;
        Vector3 potentialTarget = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0);

        // === МАНЕВР БЕЗПЕКИ ===
        // Перевіряємо точку приземлення ДО початку польоту і коригуємо її, якщо там дерево
        Vector3 finalSafeTarget = GetSafePosition(potentialTarget);

        StartCoroutine(SimulateLootDrop(finalSafeTarget, height));
    }

    void Update()
    {
        // Анімація виконується тільки тоді, коли предмет закінчив розліт
        if (!isFlying && useAnimation && visualChild != null)
        {
            // 1. Обертання навколо осі Y
            visualChild.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

            // 2. Коливання вгору-вниз (синусоїда)
            float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            visualChild.localPosition = new Vector3(0, newY, 0);
        }
    }

    IEnumerator SimulateLootDrop(Vector3 targetPos, float height)
    {
        isFlying = true;
        float timer = 0;
        Vector3 startPos = transform.position;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / jumpDuration;

            // Рух по землі
            transform.position = Vector3.Lerp(startPos, targetPos, progress);

            // Стрибок (парабола)
            float yOffset = 4 * height * progress * (1 - progress);
            if (visualChild != null)
                visualChild.localPosition = new Vector3(0, yOffset, 0);

            yield return null;
        }

        transform.position = targetPos;
        isFlying = false; // Політ закінчено, вмикаємо анімацію в Update
    }

    // Метод, який сканує простір навколо і знаходить найближчу чисту траву
    private Vector3 GetSafePosition(Vector3 potentialTarget)
    {
        // === ДЕБАГ-СКАНЕР (Тимчасовий) ===
        // Шукаємо взагалі БУДЬ-ЯКИЙ колайдер у точці, ігноруючи маску шарів (obstacleLayer)
        Collider2D anyCollider = Physics2D.OverlapCircle(potentialTarget, checkRadius);

        if (anyCollider != null)
        {
            Debug.Log($"<color=yellow>[SafeLoot Дебаг]</color> Точка намацала колайдер об'єкта: <b>{anyCollider.name}</b>. Його шар в Unity: <b>{LayerMask.LayerToName(anyCollider.gameObject.layer)}</b>");
        }
        else
        {
            Debug.Log($"<color=red>[SafeLoot Дебаг]</color> Точка взагалі ПУСТА для фізики. Навіть без фільтру шарів тут колайдера немає.");
        }
        // =================================

        // Наша звичайна логіка пошуку вільного місця
        if (!Physics2D.OverlapCircle(potentialTarget, checkRadius, obstacleLayer))
        {
            return potentialTarget;
        }

        Vector2[] searchDirections = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
        new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
    };

        float stepDistance = 0.15f;
        float maxSearchRange = 2.5f;

        for (float currentRange = stepDistance; currentRange <= maxSearchRange; currentRange += stepDistance)
        {
            foreach (Vector2 dir in searchDirections)
            {
                Vector2 testPoint = (Vector2)potentialTarget + dir * currentRange;

                if (!Physics2D.OverlapCircle(testPoint, checkRadius, obstacleLayer))
                {
                    return new Vector3(testPoint.x, testPoint.y, 0);
                }
            }
        }

        return potentialTarget;
    }
}