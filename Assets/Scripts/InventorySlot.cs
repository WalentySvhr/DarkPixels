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
    public bool isBracersEquipmentSlot = false; // --- НОВИЙ ТИП: СЛОТ ДЛЯ НАРУЧІВ ---

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
    private ScrollRect parentScrollRect;
    private bool isDraggingItem = false;

    [Header("Налаштування затискання")]
    public float holdTime = 0.5f;
    private Coroutine holdCoroutine;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    private void Awake()
    {
        groupCanvas = GetComponentInParent<Canvas>();
        parentScrollRect = GetComponentInParent<ScrollRect>();

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

        // Додано перевірку isBracersEquipmentSlot, щоб не можна було кліком зняти наручі у нікуди
        if (isWeaponEquipmentSlot || isAmuletEquipmentSlot || isRingEquipmentSlot ||
            isBeltEquipmentSlot || isPetEquipmentSlot || isHelmetEquipmentSlot ||
            isChestplateEquipmentSlot || isBracersEquipmentSlot) return;

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
        else if (itemToEquip is BracersData) slotType = "Bracers"; // --- ПЕРЕВІРКА КЛАСУ ДЛЯ НАРУЧІВ ---
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
            else if (slotType == "Bracers" && slot.isBracersEquipmentSlot) slot.AddItem(itemToEquip, 1); // --- ЕКІПІРУВАННЯ В СЛОТ НАРУЧІВ ---
        }

        InventoryManager.Instance.UpdateUI();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StopHoldTimer();

        if (parentScrollRect != null && Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x))
        {
            isDraggingItem = false;
            parentScrollRect.OnBeginDrag(eventData);
            return;
        }

        if (currentItem == null) return;

        isDraggingItem = true;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        icon.transform.SetParent(groupCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggingItem && parentScrollRect != null)
        {
            parentScrollRect.OnDrag(eventData);
            return;
        }

        if (isDraggingItem && currentItem != null)
        {
            icon.rectTransform.anchoredPosition += eventData.delta / groupCanvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggingItem && parentScrollRect != null)
        {
            parentScrollRect.OnEndDrag(eventData);
        }
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
        else if (isBracersEquipmentSlot) InventoryManager.Instance.EquipItem(newItem, "Bracers"); // --- ПЕРЕДАЧА В ІНВЕНТАРНИЙ МЕНЕДЖЕР ---
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
        else if (isBracersEquipmentSlot) InventoryManager.Instance.UnequipItem("Bracers"); // --- ЗНЯТТЯ НАРУЧІВ ---

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

        // Перевірки валідності типу предмета для слотів (включаючи наручі)
        if (this.isWeaponEquipmentSlot && !(dragItem is WeaponData)) return;
        if (this.isAmuletEquipmentSlot && !(dragItem is AmuletData)) return;
        if (this.isRingEquipmentSlot && !(dragItem is RingData)) return;
        if (this.isBeltEquipmentSlot && !(dragItem is BeltData)) return;
        if (this.isPetEquipmentSlot && !(dragItem is PetData)) return;
        if (this.isHelmetEquipmentSlot && !(dragItem is HelmetData)) return;
        if (this.isChestplateEquipmentSlot && !(dragItem is ChestplateData)) return;
        if (this.isBracersEquipmentSlot && !(dragItem is BracersData)) return; // --- ЗАБОРОНА КЛАСТИ ІНШІ ПРЕДМЕТИ В СЛОТ НАРУЧІВ ---

        bool isThisEquip = this.isWeaponEquipmentSlot || this.isAmuletEquipmentSlot || this.isRingEquipmentSlot ||
                           this.isBeltEquipmentSlot || this.isPetEquipmentSlot || this.isHelmetEquipmentSlot ||
                           this.isChestplateEquipmentSlot || this.isBracersEquipmentSlot; // --- ДОДАНО ТУТ ---

        bool isSourceEquip = sourceSlot.isWeaponEquipmentSlot || sourceSlot.isAmuletEquipmentSlot || sourceSlot.isRingEquipmentSlot ||
                             sourceSlot.isBeltEquipmentSlot || sourceSlot.isPetEquipmentSlot || sourceSlot.isHelmetEquipmentSlot ||
                             sourceSlot.isChestplateEquipmentSlot || sourceSlot.isBracersEquipmentSlot; // --- ДОДАНО ТУТ ---

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