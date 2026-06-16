using UnityEngine;

public class LightCullingUnity6 : MonoBehaviour
{
    private Transform playerTransform;

    [Header("Optimization Settings")]
    [SerializeField] private float activationDistance = 12f; // Радіус активації
    [SerializeField] private float checkInterval = 0.4f;     // Як часто перевіряти (сек)

    [Header("Target To Toggle")]
    [SerializeField] private GameObject lightHolder;         // Об'єкт, який будемо вимикати

    private float timer;
    private bool isLightOn = true;

    // === ЗМІННІ ОПТИМІЗАЦІЇ ===
    private float activationDistanceSqr; // Квадрат радіусу активації
    private bool hasPlayer;
    private bool hasLightHolder;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            hasPlayer = true;
        }

        if (lightHolder == null && transform.childCount > 0)
        {
            lightHolder = transform.GetChild(0).gameObject;
        }

        hasLightHolder = lightHolder != null;

        // Попередньо рахуємо квадрат відстані (радіус * радіус)
        activationDistanceSqr = activationDistance * activationDistance;

        // Рандомізуємо старт таймера, щоб усі лампи не робили перевірку в один і той самий кадр
        timer = Random.Range(0f, checkInterval);
    }

    void Update()
    {
        // Швидка перевірка через булеві змінні (набагато швидша за null-check об'єктів Unity)
        if (!hasPlayer || !hasLightHolder) return;

        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckDistance();
        }
    }

    void CheckDistance()
    {
        // Рахуємо квадрат відстані без обчислення квадратного кореня
        float sqrDistance = ((Vector2)transform.position - (Vector2)playerTransform.position).sqrMagnitude;

        // Порівнюємо квадрати відстаней
        bool shouldBeOn = sqrDistance <= activationDistanceSqr;

        if (shouldBeOn != isLightOn)
        {
            isLightOn = shouldBeOn;

            // Повністю деактивуємо об'єкт зі світлом.
            // В Unity 6 це примусово видаляє джерело з системи освітлення.
            lightHolder.SetActive(isLightOn);
        }
    }
}