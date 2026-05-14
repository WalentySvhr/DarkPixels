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

    [Header("Налаштування затискання")]
    public float holdTime = 0.5f;
    private Coroutine holdCoroutine;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    private void Awake()
    {
        groupCanvas = GetComponentInParent<Canvas>();
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

    // === ІДЕАЛЬНИЙ КЛІК (З ПОВЕРНЕННЯМ СТАРОГО ПРЕДМЕТА) ===
    private void HandleAction()
    {
        if (ItemInfoManager.Instance != null) ItemInfoManager.Instance.HideInfo();
        if (isWeaponEquipmentSlot || isAmuletEquipmentSlot || isRingEquipmentSlot || isBeltEquipmentSlot) return;

        Item itemToEquip = currentItem;
        string slotType = "";
        int targetSlotIndex = 0;

        if (itemToEquip is AmuletData) slotType = "Amulet";
        else if (itemToEquip is WeaponData) slotType = "Weapon";
        else if (itemToEquip is RingData) { slotType = "Ring"; targetSlotIndex = 1; }
        else if (itemToEquip is BeltData) slotType = "Belt";
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

        // Забираємо новий предмет і повертаємо старий
        InventoryManager.Instance.Remove(itemToEquip);
        if (replaceItem != null) InventoryManager.Instance.Add(replaceItem);

        // Візуально одягаємо
        InventorySlot[] allSlots = FindObjectsByType<InventorySlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var slot in allSlots)
        {
            if (slotType == "Weapon" && slot.isWeaponEquipmentSlot) slot.AddItem(itemToEquip, 1);
            else if (slotType == "Amulet" && slot.isAmuletEquipmentSlot) slot.AddItem(itemToEquip, 1);
            else if (slotType == "Belt" && slot.isBeltEquipmentSlot) slot.AddItem(itemToEquip, 1);
            else if (slotType == "Ring" && slot.isRingEquipmentSlot && slot.ringSlotIndex == targetSlotIndex) slot.AddItem(itemToEquip, 1);
        }

        InventoryManager.Instance.UpdateUI();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StopHoldTimer();
        if (currentItem == null) return;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        icon.transform.SetParent(groupCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        icon.rectTransform.anchoredPosition += eventData.delta / groupCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        icon.transform.SetParent(this.transform);
        icon.rectTransform.anchoredPosition = Vector2.zero;
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

        if (isWeaponEquipmentSlot && newItem is WeaponData weaponData)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(weaponData);
            InventoryManager.Instance.EquipItem(newItem, "Weapon");
        }
        else if (isAmuletEquipmentSlot && newItem is AmuletData amulet)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.EquipAmulet(amulet);
            InventoryManager.Instance.EquipItem(newItem, "Amulet");
        }
        else if (isRingEquipmentSlot && newItem is RingData ring)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.EquipRing(ring, ringSlotIndex);
            InventoryManager.Instance.EquipItem(newItem, "Ring", ringSlotIndex);
        }
        else if (isBeltEquipmentSlot && newItem is BeltData belt)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.EquipBelt(belt);
            InventoryManager.Instance.EquipItem(newItem, "Belt");
        }
    }

    public void ClearSlot()
    {
        if (isWeaponEquipmentSlot)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(null);
            InventoryManager.Instance.UnequipItem("Weapon");
        }
        else if (isAmuletEquipmentSlot)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.UnequipAmulet();
            InventoryManager.Instance.UnequipItem("Amulet");
        }
        else if (isRingEquipmentSlot)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.UnequipRing(ringSlotIndex);
            InventoryManager.Instance.UnequipItem("Ring", ringSlotIndex);
        }
        else if (isBeltEquipmentSlot)
        {
            PlayerEquipment equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment != null) equipment.UnequipBelt();
            InventoryManager.Instance.UnequipItem("Belt");
        }

        currentItem = null;
        currentAmount = 0;
        icon.sprite = null;
        icon.enabled = false;

        if (placeholderImage != null) placeholderImage.SetActive(true);
        if (stackText != null) stackText.gameObject.SetActive(false);
    }

    // === ІДЕАЛЬНИЙ DRAG & DROP ===
    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot == null || sourceSlot == this) return;

        Item dragItem = sourceSlot.currentItem;
        if (dragItem == null) return;

        if (this.isWeaponEquipmentSlot && !(dragItem is WeaponData)) return;
        if (this.isAmuletEquipmentSlot && !(dragItem is AmuletData)) return;
        if (this.isRingEquipmentSlot && !(dragItem is RingData)) return;
        if (this.isBeltEquipmentSlot && !(dragItem is BeltData)) return;

        bool isThisEquip = this.isWeaponEquipmentSlot || this.isAmuletEquipmentSlot || this.isRingEquipmentSlot || this.isBeltEquipmentSlot;
        bool isSourceEquip = sourceSlot.isWeaponEquipmentSlot || sourceSlot.isAmuletEquipmentSlot || sourceSlot.isRingEquipmentSlot || sourceSlot.isBeltEquipmentSlot;

        Item replaceItem = this.currentItem;

        // З ІНВЕНТАРЮ В ЕКІПІРОВКУ
        if (isThisEquip && !isSourceEquip)
        {
            InventoryManager.Instance.Remove(dragItem);
            if (replaceItem != null) InventoryManager.Instance.Add(replaceItem);
            this.AddItem(dragItem, 1);
        }
        // З ЕКІПІРОВКИ В ІНВЕНТАР
        else if (!isThisEquip && isSourceEquip)
        {
            InventoryManager.Instance.Add(dragItem);
            sourceSlot.ClearSlot();
        }

        InventoryManager.Instance.UpdateUI();
    }
}