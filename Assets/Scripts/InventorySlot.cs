using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Обов'язково для Drag & Drop

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public TextMeshProUGUI stackText;

    // Зберігаємо посилання на дані, щоб знати, ЩО ми переносимо
    public Item currentItem;
    public int currentAmount;

    private Canvas groupCanvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        groupCanvas = GetComponentInParent<Canvas>();
        // Додаємо CanvasGroup, щоб приглушати іконку при тягненні
        canvasGroup = icon.gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
    }

    public void AddItem(Item newItem, int amount)
    {
        if (newItem == null) return;

        currentItem = newItem;
        currentAmount = amount;

        icon.sprite = newItem.icon;
        icon.enabled = true;
        icon.color = Color.white;

        if (stackText != null)
        {
            stackText.text = (newItem.isStackable && amount > 1) ? amount.ToString() : "";
            stackText.gameObject.SetActive(newItem.isStackable && amount > 1);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentAmount = 0;
        icon.sprite = null;
        icon.enabled = false;
        if (stackText != null) stackText.gameObject.SetActive(false);
    }

    // --- ЛОГІКА ПЕРЕТЯГУВАННЯ ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return; // Не тягнемо порожній слот

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // Дозволяємо "бачити" слот під іконкою

        // Виносимо іконку на передній план, щоб не перекривалася іншими слотами
        icon.transform.SetParent(groupCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        // Рух за пальцем/мишкою
        icon.rectTransform.anchoredPosition += eventData.delta / groupCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Повертаємо іконку назад у цей слот (якщо Drop не відбувся в іншому місці)
        icon.transform.SetParent(this.transform);
        icon.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Отримуємо слот, З ЯКОГО прийшов предмет
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();

        if (sourceSlot != null && sourceSlot != this)
        {
            // Запам'ятовуємо наші дані (якщо в нас щось було)
            Item tempItem = currentItem;
            int tempAmount = currentAmount;

            // Забираємо дані з того слота
            AddItem(sourceSlot.currentItem, sourceSlot.currentAmount);

            // Якщо в нас щось було — віддаємо тому слоту (Своп/Обмін), інакше чистимо той слот
            if (tempItem != null)
            {
                sourceSlot.AddItem(tempItem, tempAmount);
            }
            else
            {
                sourceSlot.ClearSlot();
            }
        }
    }
}