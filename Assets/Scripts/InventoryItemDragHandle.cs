using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // Для TextMeshPro

// Скрипт вішається на префаб іконки ПРЕДМЕТА, а не на слот
public class InventoryItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent; // Слот, з якого ми почали тягнути

    // Посилання на дані про предмет (за бажанням)
    // public ItemScriptableObject itemData; 
    // public TextMeshProUGUI stackText;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // Додаємо CanvasGroup програмно, якщо його немає
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; // Запам'ятовуємо старий слот

        canvasGroup.alpha = 0.7f; // Напівпрозорість при тягненні
        canvasGroup.blocksRaycasts = false; // "Промінь" пальця проходить крізь предмет

        // Тимчасово переносимо іконку на самий верх Canvas, щоб вона була над усім UI
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Рух за пальцем (eventData.delta автоматично враховує рух по екрану)
        // scaleFactor важливий для коректної роботи на різних екранах
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Якщо ми не потрапили в новий слот (це перевірить InventorySlot),
        // повертаємо предмет назад у старий слот.
        if (transform.parent == canvas.transform)
        {
            ReturnToOriginalSlot();
        }
    }

    public void ReturnToOriginalSlot()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero; // Центруємо в слоті
    }
}