using UnityEngine;
using UnityEngine.UI;
using TMPro; // Якщо ви використовуєте TextMeshPro для тексту

public class TeleportConfirmationUI : MonoBehaviour
{
    public static TeleportConfirmationUI Instance { get; private set; }

    [Header("UI Елементи")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TMP_Text dialogText; // Сюди перетягуємо наш Text або TMP_Text компонент підказки
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private LocalTeleport pendingTeleport;
    private GameObject playerRef;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (dialogPanel != null) dialogPanel.SetActive(false);

        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
    }

    // Додали третій параметр - message
    public void ShowDialog(LocalTeleport teleport, GameObject player, string message)
    {
        pendingTeleport = teleport;
        playerRef = player;

        // Оновлюємо текст у вікні перед його показом
        if (dialogText != null)
        {
            dialogText.text = message;
        }

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void OnConfirm()
    {
        Time.timeScale = 1f;
        if (dialogPanel != null) dialogPanel.SetActive(false);

        if (pendingTeleport != null && playerRef != null)
        {
            pendingTeleport.PerformTeleport(playerRef);
        }

        ClearReferences();
    }

    private void OnCancel()
    {
        Time.timeScale = 1f;
        if (dialogPanel != null) dialogPanel.SetActive(false);
        ClearReferences();
    }

    private void ClearReferences()
    {
        pendingTeleport = null;
        playerRef = null;
    }
}