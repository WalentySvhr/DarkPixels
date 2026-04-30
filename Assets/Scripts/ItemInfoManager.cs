using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ItemInfoManager : MonoBehaviour
{
    public static ItemInfoManager Instance;

    [Header("UI Елементи")]
    public GameObject infoPanel;
    public TextMeshProUGUI nameText;

    [Space]
    public TextMeshProUGUI shortDescText;  // НОВЕ: Текст для короткого художнього опису
    public TextMeshProUGUI mainStatsText;  // Текст для основних характеристик
    public TextMeshProUGUI extraStatsText; // Текст для стаку/ефектів
    public TextMeshProUGUI priceText;      // Текст для ціни

    private Item lastOpenedItem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (infoPanel != null) infoPanel.SetActive(false);
    }

    public void ToggleInfo(Item item)
    {
        if (item == null) return;

        if (infoPanel.activeSelf && lastOpenedItem == item)
        {
            HideInfo();
        }
        else
        {
            UpdateInfo(item);
        }
    }

    public void UpdateInfo(Item item)
    {
        if (item == null) return;

        infoPanel.SetActive(true);
        lastOpenedItem = item;

        // Встановлюємо назву предмета
        nameText.text = item.itemName;

        // Отримуємо структуру з усіма блоками тексту (включаючи shortDesc)
        ItemDescription description = item.GetDetailedInfo();

        // Заповнюємо тексти
        // Тепер shortDesc відображається у відповідному полі
        SetTextAndActive(shortDescText, description.shortDesc);
        SetTextAndActive(mainStatsText, description.mainStats);
        SetTextAndActive(extraStatsText, description.extraStats);
        SetTextAndActive(priceText, description.priceText);

        UpdatePosition();
    }

    /// <summary>
    /// Допоміжний метод: встановлює текст і вимикає об'єкт, якщо тексту немає
    /// </summary>
    private void SetTextAndActive(TextMeshProUGUI textElement, string content)
    {
        if (textElement == null) return;

        if (string.IsNullOrEmpty(content))
        {
            textElement.gameObject.SetActive(false);
        }
        else
        {
            textElement.gameObject.SetActive(true);
            textElement.text = content;
        }
    }

    private void UpdatePosition()
    {
        // Покращена логіка: використовуємо RectTransform для точнішого позиціонування
        Vector3 position = Input.mousePosition;
        position.y += 150f;
        infoPanel.transform.position = position;
    }

    public void HideInfo()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
        lastOpenedItem = null;
    }

    private void Update()
    {
        if (infoPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                HideInfo();
            }
        }
    }
}