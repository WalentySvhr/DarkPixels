using UnityEngine;
using UnityEngine.Rendering.Universal; // Обов'язково для роботи з 2D світлом!

public class DayNightCycle : MonoBehaviour
{
    // Робимо цей скрипт Синглтоном, щоб усі ліхтарі та факели могли його знайти
    public static DayNightCycle Instance;

    [Header("Налаштування часу")]
    [Tooltip("Скільки реальних секунд триває одна ігрова доба")]
    public float dayDuration = 60f;

    [Range(0f, 1f)]
    [Tooltip("Поточний час доби (від 0 до 1)")]
    public float timeProgress = 0.5f; // Починаємо з середини дня

    [Header("Освітлення")]
    public Light2D globalLight;
    public Gradient lightColor; // Палітра кольорів протягом дня

    void Awake()
    {
        // Надійна ініціалізація Синглтона із захистом від дублікатів
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Рухаємо час вперед (додаємо частку часу відносно тривалості дня)
        timeProgress += Time.deltaTime / dayDuration;

        // Якщо день закінчився (значення перевалило за 1), скидаємо на 0
        if (timeProgress >= 1f)
        {
            timeProgress = 0f;
        }

        // Застосовуємо колір з Градієнта залежно від поточного часу
        if (globalLight != null)
        {
            globalLight.color = lightColor.Evaluate(timeProgress);
        }
    }
}