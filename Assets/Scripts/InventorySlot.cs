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
    [Tooltip("Для кілець: вкажи 1 або 2. Для інших слотів залиш 0.")]
    public int ringSlotIndex = 0;
    public bool isHotbarSlot = false;

    [Header("Візуал (Плейсхолдер)")]
    [Tooltip("Сюди перетягни сіру фонову іконку (ImageWeapon, ImageAmulet тощо)")]
    public GameObject placeholderImage; // НОВЕ ПОЛЕ ДЛЯ СІРОЇ ІКОНКИ

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

    private void HandleAction()
    {
        if (ItemInfoManager.Instance != null) ItemInfoManager.Instance.HideInfo();

        if (isWeaponEquipmentSlot || isAmuletEquipmentSlot || isRingEquipmentSlot) return;

        if (currentItem is AmuletData amulet)
        {
            PlayerEquipment eq = FindFirstObjectByType<PlayerEquipment>();
            if (eq != null) eq.EquipAmulet(amulet);
        }
        else if (currentItem is WeaponData weapon)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(weapon);
        }
        else if (currentItem is RingData ring)
        {
            PlayerEquipment eq = FindFirstObjectByType<PlayerEquipment>();
            if (eq != null) eq.EquipRing(ring, 1); // Одягаємо в 1-й слот по дефолту при кліку
        }
        else
        {
            InventoryManager.Instance.UseItem(currentItem);
        }
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

        // --- ХОВАЄМО ПЛЕЙСХОЛДЕР ---
        if (placeholderImage != null) placeholderImage.SetActive(false);

        if (stackText != null)
        {
            if (newItem.isStackable)
            {
                stackText.text = amount.ToString();
                stackText.gameObject.SetActive(true);
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }
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

        currentItem = null;
        currentAmount = 0;
        icon.sprite = null;
        icon.enabled = false;

        // --- ПОКАЗУЄМО ПЛЕЙСХОЛДЕР ЗНОВУ ---
        if (placeholderImage != null) placeholderImage.SetActive(true);

        if (stackText != null) stackText.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot != this)
        {
            if (this.isWeaponEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is WeaponData)) return;
            if (this.isAmuletEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is AmuletData)) return;
            if (this.isRingEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is RingData)) return;

            Item itemToMove = sourceSlot.currentItem;
            int amountToMove = sourceSlot.currentAmount;
            Item itemToReplace = this.currentItem;
            int amountToReplace = this.currentAmount;

            this.AddItem(itemToMove, amountToMove);

            if (itemToReplace != null)
                sourceSlot.AddItem(itemToReplace, amountToReplace);
            else
                sourceSlot.ClearSlot();

            InventoryManager.Instance.UpdateUI();
        }
    }
}