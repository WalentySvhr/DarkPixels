using UnityEngine;

public class DailyQuestPoint : MonoBehaviour
{
    [Header("Daily Quest Settings")]
    [Tooltip("Тип дейліка, прогрес якого закриває ця точка (наприклад, KillElite, CatchFish тощо)")]
    public DailyQuestType pointType;

    [Tooltip("ID цілі, який має СУВОРО збігатися з Target ID, що ти пишеш в інспекторі DailyQuestManager")]
    public string targetID;

    [Tooltip("Чи вимикати об'єкт після наступання (актуально для зон квесту)")]
    public bool destroyOnComplete = true;

    private void Start()
    {
        // Реєструємо цю точку в базі даних менеджера дейліків
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.RegisterDailyPoint(this);
        }
    }

    private void OnDestroy()
    {
        // Обов'язково знімаємо реєстрацію, якщо об'єкт видаляється зі сцени
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.UnregisterDailyPoint(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Додаємо прогрес у дейлік при вході в зону
            DailyQuestManager.Instance.AddProgress(pointType, 1);

            if (destroyOnComplete)
                gameObject.SetActive(false);
        }
    }

    // Якщо це NPC (рибалка, торговець) або об'єкт для взаємодії — викликай цей метод при розмові/кліці
    public void Interact()
    {
        DailyQuestManager.Instance.AddProgress(pointType, 1);
    }

    // Якщо це Елітний моб — викликай цей метод у його скрипті в момент смерті (OnDeath)
    public void OnMobDeath()
    {
        DailyQuestManager.Instance.AddProgress(pointType, 1);
    }
}