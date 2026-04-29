using UnityEngine;
using TMPro;
using System.Collections;

public class TowerUIManager : MonoBehaviour
{
    public static TowerUIManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI notificationText;
    public float displayDuration = 3f;

    private Coroutine currentRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Підготовка тексту при старті
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(true);
            notificationText.text = "";
            notificationText.canvasRenderer.SetAlpha(0); // Ховаємо через прозорість
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationText == null) return;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(DisplayRoutine(message));
    }

    IEnumerator DisplayRoutine(string message)
    {
        notificationText.text = message;
        notificationText.canvasRenderer.SetAlpha(1); // Показуємо

        yield return new WaitForSeconds(displayDuration);

        notificationText.canvasRenderer.SetAlpha(0); // Ховаємо
        notificationText.text = "";
        currentRoutine = null;
    }
}