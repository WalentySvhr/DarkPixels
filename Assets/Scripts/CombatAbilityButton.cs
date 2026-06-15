using UnityEngine;
using UnityEngine.UI;

public class CombatAbilityButton : MonoBehaviour
{
    // Глобальний доступ (Синглтон)
    public static CombatAbilityButton Instance { get; private set; }

    [Header("UI Компоненти кнопки")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image activeOverlay; // Напівпрозора рамка активації аури

    [Header("Налаштування ефекту натискання")]
    [SerializeField] private float pressedScaleMultiplier = 0.9f; // На скільки стискається кнопка при кліку
    [SerializeField] private float animationSpeed = 15f;          // Швидкість повернення розміру

    // Публічна властивість для читання зі слотів магазину
    public AbilitySO equippedAbility { get; private set; }

    private bool isToggled = false;
    private Vector3 originalScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        originalScale = transform.localScale;
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
        UpdateVisuals();
    }

    private void Update()
    {
        // Плавне повернення кнопки до початкового розміру після кліку
        if (transform.localScale != originalScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * animationSpeed);
        }
    }

    // Метод для призначення магії на цю кнопку з UI магазину
    public void EquipAbility(AbilitySO newAbility)
    {
        if (newAbility == null && equippedAbility != null)
        {
            if (isToggled && equippedAbility.type == AbilityType.Toggleable && AbilityManager.Instance != null)
            {
                AbilityManager.Instance.UseAbility(equippedAbility);
                Debug.Log($"[HUD Button] Активну ауру {equippedAbility.abilityName} вимкнено перед видаленням.");
            }
        }

        equippedAbility = newAbility;
        isToggled = false;

        UpdateVisuals();
    }

    private void OnButtonClicked()
    {
        if (equippedAbility == null || AbilityManager.Instance == null) return;

        // Ефект фізичного відгуку (зменшуємо кнопку в момент кліку)
        transform.localScale = originalScale * pressedScaleMultiplier;

        // Передаємо команду активації в менеджер вмінь
        AbilityManager.Instance.UseAbility(equippedAbility);

        // Якщо це аура, перемикаємо стан підсвітки кнопки
        if (equippedAbility.type == AbilityType.Toggleable)
        {
            isToggled = !isToggled;
        }
        else
        {
            isToggled = false;
        }

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        Button mainButton = GetComponent<Button>();

        if (equippedAbility == null)
        {
            if (iconImage != null) iconImage.gameObject.SetActive(false);
            if (activeOverlay != null) activeOverlay.gameObject.SetActive(false);

            if (mainButton != null)
            {
                mainButton.interactable = false;
                if (mainButton.targetGraphic != null) mainButton.targetGraphic.color = Color.white;
            }
        }
        else
        {
            if (iconImage != null)
            {
                if (equippedAbility.abilityIcon != null)
                {
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = equippedAbility.abilityIcon;

                    // 🌟 ОНОВЛЕНО: Зміна кольору самої іконки (спрайту) скіла.
                    // Якщо аура увімкнена (isToggled) — робимо картинку сірішою/темнішою (сірий колір 0.6f).
                    // Якщо вимкнена — повертаємо повну яскравість (білий колір).
                    iconImage.color = isToggled ? new Color(0.55f, 0.55f, 0.55f, 1f) : Color.white;
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                    Debug.LogWarning($"[HUD Button] У вміння {equippedAbility.abilityName} відсутня іконка в SO!");
                }
            }

            // Керуємо рамкою підсвітки
            if (activeOverlay != null)
            {
                activeOverlay.gameObject.SetActive(isToggled);
            }

            if (mainButton != null)
            {
                mainButton.interactable = true;

                // Скидаємо колір заднього фону кнопки в дефолт, щоб він не заважав іконці
                if (mainButton.targetGraphic != null) mainButton.targetGraphic.color = Color.white;
            }
        }
    }

    // Метод примусового вимкнення підсвітки (наприклад, якщо закінчилася мана)
    public void ForceUntoggle()
    {
        isToggled = false;

        // Повертаємо іконці нормальний яскравий колір
        if (iconImage != null) iconImage.color = Color.white;
        if (activeOverlay != null) activeOverlay.gameObject.SetActive(false);

        Button mainButton = GetComponent<Button>();
        if (mainButton != null && mainButton.targetGraphic != null)
        {
            mainButton.targetGraphic.color = Color.white;
        }
    }
}