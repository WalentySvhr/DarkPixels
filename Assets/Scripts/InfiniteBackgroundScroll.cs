using UnityEngine;

public class CloudFlyEffect : MonoBehaviour
{
    [Header("Налаштування руху")]
    [SerializeField] private float minSpeed = 30f;  // Мінімальна швидкість
    [SerializeField] private float maxSpeed = 80f;  // Максимальна швидкість

    [Header("Межі появи по висоті (Y)")]
    [SerializeField] private float minY = 100f;     // Найнижча точка на екрані
    [SerializeField] private float maxY = 400f;     // Найвища точка на екрані

    private RectTransform rectTransform;
    private float currentSpeed;
    private float screenWidth;
    private float cloudWidth;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Отримуємо ширину Canvas/екрана
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            screenWidth = canvasRect.rect.width;
        }
        else
        {
            screenWidth = Screen.width; // Фолбек, якщо не знайшли Canvas
        }

        cloudWidth = rectTransform.rect.width;

        // Задаємо початкову випадкову швидкість та позицію
        ResetCloudPosition(true); // true означає, що на старті гри хмара може з'явитися посеред екрана
    }

    private void Update()
    {
        // Рухаємо хмару праворуч (Vector2.right)
        rectTransform.anchoredPosition += Vector2.right * currentSpeed * Time.deltaTime;

        // Рахуємо крайню праву межу (екран + уся ширина хмари, щоб вона зникла повністю)
        float rightBoundary = (screenWidth / 2f) + (cloudWidth / 2f);

        // Якщо хмара повністю вилетіла за правий край
        if (rectTransform.anchoredPosition.x > rightBoundary)
        {
            ResetCloudPosition(false); // Переносимо наліво за екран
        }
    }

    private void ResetCloudPosition(bool startOfGame)
    {
        // Вибираємо випадкову швидкість для цього забігу
        currentSpeed = Random.Range(minSpeed, maxSpeed);

        float spawnX;

        if (startOfGame)
        {
            // На самому старті гри випадково розкидаємо хмари по екрану, щоб вони не летіли купою
            spawnX = Random.Range(-(screenWidth / 2f), (screenWidth / 2f));
        }
        else
        {
            // Під час гри спавним ЧІТКО за лівим краєм екрана
            spawnX = -(screenWidth / 2f) - (cloudWidth / 2f);
        }

        // Випадкова висота для різноманітності
        float spawnY = Random.Range(minY, maxY);

        rectTransform.anchoredPosition = new Vector2(spawnX, spawnY);
    }
}