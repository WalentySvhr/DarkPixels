using UnityEngine;
using Cinemachine; // Для вашої версії Cinemachine

public class LocalTeleport : MonoBehaviour
{
    [Header("Налаштування телепорту")]
    public Transform targetLocation;
    public bool isActive = false;
    public string locationName = "Відкритий світ";

    [Header("Налаштування Башти")]
    public bool isEntranceToTower = false;
    public bool resetTowerOnExit = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive && collision.CompareTag("Player"))
        {
            Vector3 oldPos = collision.transform.position;

            // 1. Скидаємо швидкість
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // 2. Переміщуємо гравця
            if (targetLocation != null)
            {
                collision.transform.position = targetLocation.position;

                // 3. АВТОМАТИЧНИЙ ПОШУК КАМЕРИ
                CinemachineVirtualCamera vcam = FindFirstObjectByType<CinemachineVirtualCamera>();

                if (vcam != null)
                {
                    Vector3 delta = targetLocation.position - oldPos;
                    vcam.OnTargetObjectWarped(collision.transform, delta);
                }
            }

            // 4. Твій інший код (Announcer, TowerManager...)
            if (LocationAnnouncer.Instance != null)
                LocationAnnouncer.Instance.ShowLocation(locationName);

            if (TowerManager.Instance != null)
            {
                if (isEntranceToTower)
                {
                    TowerManager.Instance.IsPlayerInTower = true; // Блокуємо відкриття мапи
                    TowerManager.Instance.StartTowerRun();
                }
                else if (resetTowerOnExit)
                {
                    TowerManager.Instance.IsPlayerInTower = false; // Розблоковуємо мапу
                    TowerManager.Instance.ResetTowerProgress();
                }
            }
        }
    }

    public void OpenDoor()
    {
        isActive = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;
    }
}