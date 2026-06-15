using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    [System.Serializable]
    private class ActiveAbilityState
    {
        public AbilitySO data;
        public bool isActive;
        public float nextTickTime;
        public GameObject spawnedFX; // <-- ДОДАНО: зберігає посилання на створений на сцені ефект

        public ActiveAbilityState(AbilitySO data)
        {
            this.data = data;
            this.isActive = false;
            this.nextTickTime = 0f;
            this.spawnedFX = null;
        }
    }

    [Header("Налаштування Вмінь гравця")]
    [SerializeField] private List<AbilitySO> activeAbilities = new List<AbilitySO>();

    private PlayerMana playerMana;
    private Dictionary<AbilitySO, ActiveAbilityState> abilityStates = new Dictionary<AbilitySO, ActiveAbilityState>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Жорстке скидання для редактора Unity, щоб рівні не залишалися з минулого запуску
        foreach (var ability in activeAbilities)
        {
            if (ability != null)
            {
                ability.currentLevel = 0;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(ability);
#endif
            }
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log("[AbilityManager] Рівні абілок скинуто в 0 перед завантаженням JSON.");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
    }

    private void Start()
    {
        playerMana = GetComponent<PlayerMana>();
        if (playerMana == null)
        {
            Debug.LogError($"[AbilityManager] На об'єкті {gameObject.name} не знайдено PlayerMana!");
        }

        foreach (var ability in activeAbilities)
        {
            var state = abilityStates.ContainsKey(ability); // Безпечна ініціалізація у Start
            RegisterAbility(ability);
        }
    }

    public void RegisterAbility(AbilitySO newAbility)
    {
        if (newAbility == null) return;

        if (!abilityStates.ContainsKey(newAbility))
        {
            abilityStates.Add(newAbility, new ActiveAbilityState(newAbility));
            if (!activeAbilities.Contains(newAbility))
            {
                activeAbilities.Add(newAbility);
            }
        }
    }

    public void UseAbility(AbilitySO abilityData)
    {
        if (abilityData == null || playerMana == null) return;

        if (!abilityStates.ContainsKey(abilityData)) RegisterAbility(abilityData);

        if (abilityData.type == AbilityType.Instant) ExecuteInstantAbility(abilityData);
        else if (abilityData.type == AbilityType.Toggleable) ToggleAbility(abilityData);
    }

    private void ExecuteInstantAbility(AbilitySO data)
    {
        float currentManaCost = data.GetCurrentManaCost();

        if (playerMana.TrySpendMana(currentManaCost))
        {
            Debug.Log($"Активовано разовий скіл: {data.abilityName} (Витрачено мани: {currentManaCost})");
            SpawnVisualEffect(data);
        }
    }

    private void ToggleAbility(AbilitySO data)
    {
        var state = abilityStates[data];
        state.isActive = !state.isActive;

        if (state.isActive)
        {
            state.nextTickTime = 0f;

            // 🌟 ОНОВЛЕНО: Спавнить візуальний ефект та автоматично підганяє його розмір під радіус скіла
            if (data.visualEffectPrefab != null && state.spawnedFX == null)
            {
                // Створюємо ефект як дочірній об'єкт гравця (передаємо transform як параметр батька)
                state.spawnedFX = Instantiate(data.visualEffectPrefab, transform.position, Quaternion.identity, transform);
                state.spawnedFX.transform.localPosition = Vector3.zero; // Центруємо під гравцем

                // 📐 АВТО-МАСШТАБУВАННЯ:
                // Множимо радіус на 2, тому що радіус — це відстань від центра до краю (половина діаметра),
                // а компонент Transform.localScale змінює загальний габаритний розмір (діаметр) об'єкта.
                float visualScale = data.radius * 2f * 0.5f; // Якщо вогонь завеликий, зменшуємо вдвічі
                state.spawnedFX.transform.localScale = new Vector3(visualScale, visualScale, 1f);
            }
        }
        else
        {
            StopToggleableAbility(data);
        }
    }

    private void Update()
    {
        foreach (var pair in abilityStates)
        {
            AbilitySO data = pair.Key;
            ActiveAbilityState state = pair.Value;

            if (data.type == AbilityType.Toggleable && state.isActive)
            {
                state.nextTickTime -= Time.deltaTime;

                if (state.nextTickTime <= 0f)
                {
                    // Обчислюємо, скільки мани потрібно на один тік аури
                    float costPerTick = data.GetCurrentManaCost() * data.tickRate;

                    if (playerMana.TrySpendMana(costPerTick))
                    {
                        // Мани вистачило — завдаємо шкоди ворогам і скидаємо таймер
                        ApplyAoEDamage(data);
                        state.nextTickTime = data.tickRate;

                        // 🌟 ОНОВЛЕНО: Якщо ефект з якихось причин зник, створюємо його знову та масштабуємо під радіус
                        if (data.visualEffectPrefab != null && state.spawnedFX == null)
                        {
                            state.spawnedFX = Instantiate(data.visualEffectPrefab, transform.position, Quaternion.identity, transform);
                            state.spawnedFX.transform.localPosition = Vector3.zero;

                            // Авто-масштабування під радіус з AbilitySO
                            float visualScale = data.radius * 2f;
                            state.spawnedFX.transform.localScale = new Vector3(visualScale, visualScale, 1f);
                        }
                    }
                    else
                    {
                        // 🌟 ВИПРАВЛЕННЯ БАГУ СТИСКАННЯ: Якщо мани не вистачило навіть на один тік, 
                        // ми повністю гасимо ауру через StopToggleableAbility.
                        // Це акуратно видалить префаб і переведе стан абілки в isActive = false.
                        StopToggleableAbility(data);
                    }
                }
            }
        }
    }

    private void ApplyAoEDamage(AbilitySO data)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, data.radius);
        int currentDamage = data.GetCurrentDamage();

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                Vector2 knockbackDirection = data.knockbackForce > 0f ?
                    ((Vector2)(enemyCollider.transform.position - transform.position)).normalized : Vector2.zero;

                enemy.TakeDamage(currentDamage, knockbackDirection, data.knockbackForce, false);
            }
        }
    }

    private void StopToggleableAbility(AbilitySO data)
    {
        if (abilityStates.ContainsKey(data))
        {
            var state = abilityStates[data];
            state.isActive = false;

            if (state.spawnedFX != null)
            {
                Destroy(state.spawnedFX);
                state.spawnedFX = null;
            }

            // 🌟 ДОДАЙ ЦЕЙ РЯДОК СЮДИ:
            // Якщо вимкнена абілка — це та, яка зараз винесена на кнопку HUD, гасимо кнопку!
            if (CombatAbilityButton.Instance != null && CombatAbilityButton.Instance.equippedAbility == data)
            {
                CombatAbilityButton.Instance.ForceUntoggle();
            }
        }
    }

    private void SpawnVisualEffect(AbilitySO data)
    {
        // Цей метод залишається для Instant скілів (ефекти, які самі знищуються через скрипт руйнування за часом)
        if (data.visualEffectPrefab != null)
        {
            Instantiate(data.visualEffectPrefab, transform.position, Quaternion.identity, transform);
        }
    }
    private void OnDrawGizmosSelected()
    {
        // Якщо гра запущена і є активна абілка, малюємо її радіус
        if (activeAbilities != null)
        {
            foreach (var ability in activeAbilities)
            {
                if (ability != null && ability.type == AbilityType.Toggleable)
                {
                    // Малюємо червоне напівпрозоре коло навколо гравця в редакторі
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Червоний колір з альфою 0.3
                    Gizmos.DrawWireSphere(transform.position, ability.radius);
                }
            }
        }
    }
}