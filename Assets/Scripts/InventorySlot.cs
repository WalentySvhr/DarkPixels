using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Налаштування слота")]
    public bool isWeaponEquipmentSlot = false;
    public bool isAmuletEquipmentSlot = false;
    public bool isRingEquipmentSlot = false;
    public bool isBeltEquipmentSlot = false;
    public bool isPetEquipmentSlot = false;
    public bool isHelmetEquipmentSlot = false;
    public bool isChestplateEquipmentSlot = false;

    [Tooltip("Для кілець: вкажи 1 або 2. Для інших слотів залиш 0.")]
    public int ringSlotIndex = 0;
    public bool isHotbarSlot = false;

    [Header("Візуал (Плейсхолдер)")]
    public GameObject placeholderImage;

    [Header("Елементи UI")]
    public Image icon;
    public TextMeshProUGUI stackText;

    public Item currentItem;
    public int currentAmount;

    private Canvas groupCanvas;
    private CanvasGroup canvasGroup;
    private ScrollRect parentScrollRect; // --- ПОСИЛАННЯ НА СКРОЛЕР ---
    private bool isDraggingItem = false; // --- ПРАПОРЕЦЬ ДЛЯ РОЗПІЗНАВАННЯ СКРОЛУ ---

    [Header("Налаштування затискання")]
    public float holdTime = 0.5f;
    private Coroutine holdCoroutine;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    private void Awake()
    {
        groupCanvas = GetComponentInParent<Canvas>();
        parentScrollRect = GetComponentInParent<ScrollRect>(); // Знаходимо скролер вище по ієрархії

        if (icon != null)
        {
            canvasGroup = icon.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentItem != null) holdCoroutine = StartCoroutine(HoldTimer());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHoldTimer();
        if (ItemInfoManager.Instance != null) ItemInfoManager.Instance.HideInfo();
    }

    private IEnumerator HoldTimer()
    {
        yield return new WaitForSeconds(holdTime);
        if (currentItem != null && ItemInfoManager.Instance != null)
        {
            ItemInfoManager.Instance.UpdateInfo(currentItem);
        }
    }

    private void StopHoldTimer()
    {
        if (holdCoroutine != null) { StopCoroutine(holdCoroutine); holdCoroutine = null; }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        if (isHotbarSlot)
        {
            HandleAction();
        }
        else
        {
            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick <= doubleClickThreshold)
            {
                StopHoldTimer();
                HandleAction();
            }
            lastClickTime = Time.time;
        }
    }

    private void HandleAction()
    {
        if (ItemInfoManager.Instance != null) ItemInfoManager.Instance.HideInfo();

        if (isWeaponEquipmentSlot || isAmuletEquipmentSlot || isRingEquipmentSlot ||
            isBeltEquipmentSlot || isPetEquipmentSlot || isHelmetEquipmentSlot || isChestplateEquipmentSlot) return;

        Item itemToEquip = currentItem;
        string slotType = "";
        int targetSlotIndex = 0;

        if (itemToEquip is AmuletData) slotType = "Amulet";
        else if (itemToEquip is WeaponData) slotType = "Weapon";
        else if (itemToEquip is RingData) { slotType = "Ring"; targetSlotIndex = 1; }
        else if (itemToEquip is BeltData) slotType = "Belt";
        else if (itemToEquip is PetData) slotType = "Pet";
        else if (itemToEquip is HelmetData) slotType = "Helmet";
        else if (itemToEquip is ChestplateData) slotType = "Chestplate";
        else
        {
            InventoryManager.Instance.UseItem(itemToEquip);
            return;
        }

        string slotKey = targetSlotIndex > 0 ? $"{slotType}_{targetSlotIndex}" : slotType;
        Item replaceItem = null;
        if (InventoryManager.Instance.equippedItems.ContainsKey(slotKey))
        {
            replaceItem = InventoryManager.Instance.equippedItems[slotKey];
        }

        InventoryManager.Instance.Remove(itemToEquip);

        if (replaceItem != null)
        {
            if (replaceItem is PetData)
            {
                if (!InventoryManager.Instance.Contains(replaceItem))
                    InventoryManager.Instance.Add(replaceItem);
            }
            else
            {
                InventoryManager.Instance.Add(replaceItem);
            }
        }

        InventorySlot[] allSlots = FindObjectsByType<InventorySlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var slot in allSlots)
        {
            if (slotType == "Weapon" && slot.isWeaponEquipmentSlot) slot.AddItem(itemToEquip, 1);
            else if (slotType == "Amulet" && slot.isAmuletEquipmentSlot) slot.AddItem(itemToEquip, 1);
            else if (slotType == "Belt" && slot.isBeltEquipmentSlot) slot.AddItem(itemToEquip, 1);
            else if (slotType == "Ring" && slot.isRingEquipmentSlot && slot.ringSlotIndex == targetSlotIndex) slot.AddItem(itemToEquip, 1);
            else if (slotType == "Pet" && slot.isPetEquipmentSlot) slot.AddItem(itemToEquip, 1);
            else if (slotType == "Helmet" && slot.isHelmetEquipmentSlot) slot.AddItem(itemToEquip, 1);
            else if (slotType == "Chestplate" && slot.isChestplateEquipmentSlot) slot.AddItem(itemToEquip, 1);
        }

        InventoryManager.Instance.UpdateUI();
    }

    // === ОНОВЛЕНІ МЕТОДИ DRAG & DROP З ПІДТРИМКОЮ СКРОЛУ ===

    // === ОНОВЛЕНІ МЕТОДИ DRAG & DROP З ПІДТРИМКОЮ СКРОЛУ ДЛЯ ПОРОЖНІХ СЛОТІВ ===

    public void OnBeginDrag(PointerEventData eventData)
    {
        StopHoldTimer();

        // 1. Спочатку перевіряємо, чи це рух для скролінгу (вертикальний рух)
        if (parentScrollRect != null && Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x))
        {
            isDraggingItem = false;
            parentScrollRect.OnBeginDrag(eventData); // Передаємо скрол батьку, навіть якщо слот порожній!
            return;
        }

        // 2. Якщо це НЕ скрол, а спроба перетягнути предмет, ось ТЕПЕР перевіряємо чи є що перетягувати
        if (currentItem == null) return;

        // 3. Якщо предмет є — вмикаємо звичайний Drag
        isDraggingItem = true;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        icon.transform.SetParent(groupCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Якщо ми зараз скролимо екран (неважливо, порожній слот чи ні)
        if (!isDraggingItem && parentScrollRect != null)
        {
            parentScrollRect.OnDrag(eventData);
            return;
        }

        // Якщо ми тягнемо сам предмет і він існує
        if (isDraggingItem && currentItem != null)
        {
            icon.rectTransform.anchoredPosition += eventData.delta / groupCanvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Якщо завершився скрол екрана
        if (!isDraggingItem && parentScrollRect != null)
        {
            parentScrollRect.OnEndDrag(eventData);
        }
        // Якщо завершилося перетягування іконки предмета
        else if (isDraggingItem)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            icon.transform.SetParent(this.transform);
            icon.rectTransform.anchoredPosition = Vector2.zero;
        }

        isDraggingItem = false;
    }

    public void AddItem(Item newItem, int amount)
    {
        if (newItem == null) { ClearSlot(); return; }

        currentItem = newItem;
        currentAmount = amount;
        icon.sprite = newItem.icon;
        icon.enabled = true;
        icon.color = Color.white;

        if (placeholderImage != null) placeholderImage.SetActive(false);
        if (stackText != null)
        {
            if (newItem.isStackable)
            {
                stackText.text = amount.ToString();
                stackText.gameObject.SetActive(true);
            }
            else stackText.gameObject.SetActive(false);
        }

        if (isWeaponEquipmentSlot) InventoryManager.Instance.EquipItem(newItem, "Weapon");
        else if (isAmuletEquipmentSlot) InventoryManager.Instance.EquipItem(newItem, "Amulet");
        else if (isRingEquipmentSlot) InventoryManager.Instance.EquipItem(newItem, "Ring", ringSlotIndex);
        else if (isBeltEquipmentSlot) InventoryManager.Instance.EquipItem(newItem, "Belt");
        else if (isPetEquipmentSlot) InventoryManager.Instance.EquipItem(newItem, "Pet");
        else if (isHelmetEquipmentSlot) InventoryManager.Instance.EquipItem(newItem, "Helmet");
        else if (isChestplateEquipmentSlot) InventoryManager.Instance.EquipItem(newItem, "Chestplate");
    }

    public void ClearSlot()
    {
        if (isWeaponEquipmentSlot) InventoryManager.Instance.UnequipItem("Weapon");
        else if (isAmuletEquipmentSlot) InventoryManager.Instance.UnequipItem("Amulet");
        else if (isRingEquipmentSlot) InventoryManager.Instance.UnequipItem("Ring", ringSlotIndex);
        else if (isBeltEquipmentSlot) InventoryManager.Instance.UnequipItem("Belt");
        else if (isPetEquipmentSlot) InventoryManager.Instance.UnequipItem("Pet");
        else if (isHelmetEquipmentSlot) InventoryManager.Instance.UnequipItem("Helmet");
        else if (isChestplateEquipmentSlot) InventoryManager.Instance.UnequipItem("Chestplate");

        currentItem = null;
        currentAmount = 0;
        icon.sprite = null;
        icon.enabled = false;

        if (placeholderImage != null) placeholderImage.SetActive(true);
        if (stackText != null) stackText.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot == null || sourceSlot == this) return;

        Item dragItem = sourceSlot.currentItem;
        if (dragItem == null) return;

        if (this.isWeaponEquipmentSlot && !(dragItem is WeaponData)) return;
        if (this.isAmuletEquipmentSlot && !(dragItem is AmuletData)) return;
        if (this.isRingEquipmentSlot && !(dragItem is RingData)) return;
        if (this.isBeltEquipmentSlot && !(dragItem is BeltData)) return;
        if (this.isPetEquipmentSlot && !(dragItem is PetData)) return;
        if (this.isHelmetEquipmentSlot && !(dragItem is HelmetData)) return;
        if (this.isChestplateEquipmentSlot && !(dragItem is ChestplateData)) return;

        bool isThisEquip = this.isWeaponEquipmentSlot || this.isAmuletEquipmentSlot || this.isRingEquipmentSlot ||
                           this.isBeltEquipmentSlot || this.isPetEquipmentSlot || this.isHelmetEquipmentSlot || this.isChestplateEquipmentSlot;

        bool isSourceEquip = sourceSlot.isWeaponEquipmentSlot || sourceSlot.isAmuletEquipmentSlot || sourceSlot.isRingEquipmentSlot ||
                             sourceSlot.isBeltEquipmentSlot || sourceSlot.isPetEquipmentSlot || sourceSlot.isHelmetEquipmentSlot || sourceSlot.isChestplateEquipmentSlot;

        Item replaceItem = this.currentItem;

        if (isThisEquip && !isSourceEquip)
        {
            InventoryManager.Instance.Remove(dragItem);

            if (replaceItem != null)
            {
                if (replaceItem is PetData)
                {
                    if (!InventoryManager.Instance.Contains(replaceItem))
                        InventoryManager.Instance.Add(replaceItem);
                }
                else
                {
                    InventoryManager.Instance.Add(replaceItem);
                }
            }

            this.AddItem(dragItem, 1);
        }
        else if (!isThisEquip && isSourceEquip)
        {
            InventoryManager.Instance.Add(dragItem);
            sourceSlot.ClearSlot();
        }

        InventoryManager.Instance.UpdateUI();
    }
}