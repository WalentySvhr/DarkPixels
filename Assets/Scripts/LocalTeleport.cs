using UnityEngine;
using Cinemachine;

public class LocalTeleport : MonoBehaviour
{
    [Header("Налаштування телепорту")]
    public Transform targetLocation;
    public bool isActive = false;
    public string locationName = "Відкритий світ";
    public bool requireConfirmation = true;

    [TextArea(2, 4)] // Робить поле зручним для введення тексту в кілька рядків
    public string confirmationMessage = "Бажаєте телепортуватись?";

    [Header("Налаштування Башти")]
    public bool isEntranceToTower = false;
    public bool resetTowerOnExit = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive && collision.CompareTag("Player"))
        {
            if (requireConfirmation && TeleportConfirmationUI.Instance != null)
            {
                // ПЕРЕДАЄМО ТЕКСТ разом з іншими даними
                TeleportConfirmationUI.Instance.ShowDialog(this, collision.gameObject, confirmationMessage);
            }
            else
            {
                PerformTeleport(collision.gameObject);
            }
        }
    }

    public void PerformTeleport(GameObject player)
    {
        Vector3 oldPos = player.transform.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (targetLocation != null)
        {
            Vector3 safeNewPosition = new Vector3(targetLocation.position.x, targetLocation.position.y, 0f);
            player.transform.position = safeNewPosition;

            CinemachineVirtualCamera vcam = FindFirstObjectByType<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                Vector3 delta = safeNewPosition - oldPos;
                vcam.OnTargetObjectWarped(player.transform, delta);
            }
        }

        if (LocationAnnouncer.Instance != null)
            LocationAnnouncer.Instance.ShowLocation(locationName);

        if (TowerManager.Instance != null)
        {
            if (isEntranceToTower)
            {
                TowerManager.Instance.IsPlayerInTower = true;
                TowerManager.Instance.StartTowerRun();
            }
            else if (resetTowerOnExit)
            {
                TowerManager.Instance.IsPlayerInTower = false;
                TowerManager.Instance.ResetTowerProgress();
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