using UnityEngine;

public class CurrencyExchangeNPC : MonoBehaviour
{
    [Header("Налаштування взаємодії")]
    public float interactRange = 2.5f;

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void OnMouseDown()
    {
        // Перевірку EventSystem видалено для надійності кліку

        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= interactRange)
        {
            OpenExchangeWindow();
        }
        else
        {
            Debug.Log("Підійдіть ближче до Міняйла!");
        }
    }

    public void OpenExchangeWindow()
    {
        if (CurrencyExchangeUI.Instance == null) return;

        // Перевірка на активність вікна залишається, щоб не відкривати одне й те саме двічі
        if (CurrencyExchangeUI.Instance.windowPanel != null && CurrencyExchangeUI.Instance.windowPanel.activeInHierarchy) return;

        CurrencyExchangeUI.Instance.Open();

        SendMessage("StartInteraction", SendMessageOptions.DontRequireReceiver);
    }
}