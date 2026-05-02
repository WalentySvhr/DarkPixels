using UnityEngine;
using System.Collections;

public class DungeonAdUI : MonoBehaviour
{
    public static DungeonAdUI Instance;

    [Header("Налаштування часу")]
    public float activeTime = 30f; // Скільки секунд кнопка висить після старту поверху

    [Header("Кнопка")]
    public GameObject boostButtonObject;

    // Зберігаємо посилання на таймер, щоб могти його зупинити
    private Coroutine hideRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Завжди ховаємо на старті
        HideBoostButton();
    }

    // Викликається з TowerManager на початку поверху
    public void ShowBoostButton()
    {
        if (boostButtonObject == null) return;

        // 1. Якщо старий таймер ще йшов - скидаємо його
        if (hideRoutine != null) StopCoroutine(hideRoutine);

        // 2. Показуємо кнопку
        boostButtonObject.SetActive(true);
        Debug.Log($"Данж: З'явилася кнопка х3! У гравця є {activeTime} сек.");

        // 3. Запускаємо таймер на зникнення
        hideRoutine = StartCoroutine(HideAfterTimeRoutine());
    }

    // Корутина таймера
    private IEnumerator HideAfterTimeRoutine()
    {
        // Чекаємо заданий час
        yield return new WaitForSeconds(activeTime);

        // Якщо час вийшов, а кнопка все ще активна (її не натиснули) - ховаємо
        if (boostButtonObject != null && boostButtonObject.activeSelf)
        {
            boostButtonObject.SetActive(false);
            Debug.Log("Данж: Пропозиція х3 зникла (час вийшов).");
        }
    }

    // Викликається з TowerManager (коли зачистили поверх) або коли забрали нагороду
    public void HideBoostButton()
    {
        if (boostButtonObject != null)
        {
            boostButtonObject.SetActive(false);
        }

        // Зупиняємо таймер, якщо він ще працює
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
    }
}