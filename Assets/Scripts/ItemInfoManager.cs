using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ItemInfoManager : MonoBehaviour
{
    public static ItemInfoManager Instance;

    [Header("UI Елементи")]
    public GameObject infoPanel;
    public RectTransform infoPanelRect; // Потрібно перетягнути RectTransform панелі
    public Canvas mainCanvas;           // Потрібно перетягнути головний Canvas
    public TextMeshProUGUI nameText;

    [Space]
    public TextMeshProUGUI shortDescText;
    public TextMeshProUGUI mainStatsText;
    public TextMeshProUGUI extraStatsText;
    public TextMeshProUGUI priceText;

    private Item lastOpenedItem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (infoPanel != null) infoPanel.SetActive(false);

        // Якщо забув призначити RectTransform в інспекторі
        if (infoPanelRect == null && infoPanel != null)
            infoPanelRect = infoPanel.GetComponent<RectTransform>();
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

        nameText.text = item.itemName;

        ItemDescription description = item.GetDetailedInfo();

        SetTextAndActive(shortDescText, description.shortDesc);
        SetTextAndActive(mainStatsText, description.mainStats);
        SetTextAndActive(extraStatsText, description.extraStats);
        SetTextAndActive(priceText, description.priceText);

        // Спершу оновлюємо позицію, щоб RectTransform перерахував розміри під новий текст
        Canvas.ForceUpdateCanvases();
        UpdatePosition();
    }

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
        if (infoPanelRect == null || mainCanvas == null) return;

        // Отримуємо позицію курсора/пальця
        Vector2 mousePos = Input.mousePosition;

        float scale = mainCanvas.scaleFactor;
        float panelWidth = infoPanelRect.rect.width * scale;
        float panelHeight = infoPanelRect.rect.height * scale;

        Vector2 newPos;

        // --- ГОРИЗОНТАЛЬНЕ ПОЗИЦІОНУВАННЯ ---
        // Якщо палець у правій половині екрану — показуємо вікно зліва від пальця
        if (mousePos.x > Screen.width / 2)
        {
            newPos.x = mousePos.x - panelWidth - 40f;
        }
        else // Якщо у лівій половині — показуємо справа
        {
            newPos.x = mousePos.x + 40f;
        }

        // --- ВЕРТИКАЛЬНЕ ПОЗИЦІОНУВАННЯ ---
        // Центруємо вікно по висоті відносно пальця
        newPos.y = mousePos.y - (panelHeight / 2);

        // Захист від виходу за межі екрану (Screen Clamping)
        newPos.x = Mathf.Clamp(newPos.x, 10f, Screen.width - panelWidth - 10f);
        newPos.y = Mathf.Clamp(newPos.y, 10f, Screen.height - panelHeight - 10f);

        infoPanel.transform.position = newPos;
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