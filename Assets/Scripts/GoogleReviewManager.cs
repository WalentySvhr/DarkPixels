using System.Collections;
using Google.Play.Review;
using UnityEngine;

public class GoogleReviewManager : MonoBehaviour
{
    // Патерн Singleton для зручного виклику з будь-якого іншого скрипта
    public static GoogleReviewManager Instance { get; private set; }

    private ReviewManager _reviewManager;

    private void Awake()
    {
        // Налаштування Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Об'єкт не знищиться при зміні сцен
        }
        else
        {
            Destroy(gameObject); // Знищуємо дублікати
        }
    }

    void Start()
    {
        // Ініціалізуємо менеджер відгуків від Google
        _reviewManager = new ReviewManager();
    }

    // ОНОВЛЕНО: Тепер приймає параметри перевірки з вашого JSON
    public void TryTriggerReview(bool alreadyReviewed, int victoryCount)
    {
        // 1. Якщо ми вже показували вікно — більше не турбуємо гравця
        if (alreadyReviewed)
        {
            return;
        }

        // 2. Якщо це, наприклад, 3-тя перемога (або більше) — запускаємо вікно
        // Можете змінити трійку на будь-яку кількість перемог, яка вам підходить
        if (victoryCount >= 3)
        {
            StartCoroutine(RequestAndShowReview());
        }
    }

    private IEnumerator RequestAndShowReview()
    {
        // 1. Запитуємо у Google Play дозвіл та інформацію про можливість показу
        var requestFlowOp = _reviewManager.RequestReviewFlow();
        yield return requestFlowOp;

        if (requestFlowOp.Error != ReviewErrorCode.NoError)
        {
            // Якщо немає інтернету або пристрій не підтримує Google Play
            Debug.LogError("Помилка запиту відгуку: " + requestFlowOp.Error.ToString());
            yield break;
        }

        // Отримуємо об'єкт із даними потоку відгуку
        PlayReviewInfo playReviewInfo = requestFlowOp.GetResult();

        // 2. Запускаємо саме вікно відгуку поверх гри
        var launchFlowOp = _reviewManager.LaunchReviewFlow(playReviewInfo);
        yield return launchFlowOp;

        if (launchFlowOp.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError("Помилка показу вікна відгуку: " + launchFlowOp.Error.ToString());
            yield break;
        }

        // Вікно успішно закрилося (або користувач його пропустив). Гра продовжується.
        Debug.Log("Процес оцінювання завершено успішно!");

        // ОНОВЛЕНО: Викликаємо метод вашого SaveManager для збереження прапорця в JSON
        OnReviewSuccessfullyShown();
    }

    private void OnReviewSuccessfullyShown()
    {
        if (SaveManager.Instance != null)
        {
            // Повідомляємо SaveManager, що все пройшло успішно
            SaveManager.Instance.OnReviewSuccessfullyShown();
        }
    }
}