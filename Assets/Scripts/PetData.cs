using UnityEngine;

// --- Список усіх можливих здібностей петів ---
public enum PetAbilityType
{
    None,
    MagnetLoot,     // Притягує лут
    HealthRegen,    // (На майбутнє) Лікує гравця
    DamageAura      // (На майбутнє) Завдає шкоди ворогам поруч
}

[CreateAssetMenu(fileName = "New Pet", menuName = "Inventory/Pet Data")]
public class PetData : Item
{
    [Header("Pet Settings")]
    public GameObject petPrefab;

    [Header("Ціна в Діамантах (Преміум)")]
    public int diamondPrice = 100;

    [Header("Бонуси для гравця")]
    public int bonusDamage = 0;
    public float bonusHealth = 0f;

    [Header("Налаштування Здібності")]
    public PetAbilityType abilityType = PetAbilityType.MagnetLoot; // Вибір здібності
    public float abilityRadius = 4f;   // Радіус дії (для магніту, аури тощо)
    public float abilityPower = 8f;    // Сила дії (швидкість магніту, або кількість хілу)

    [Header("Tooltip Info")]
    [TextArea(2, 4)]
    public string abilityDescription = "Збирає лут навколо гравця";

    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        desc.shortDesc = "Type: Pet";
        desc.mainStats = abilityDescription;

        string extra = "";
        if (bonusDamage > 0) extra += $"Damage: +{bonusDamage}\n";
        if (bonusHealth > 0) extra += $"Health: +{bonusHealth}\n";

        // Відображаємо радіус здібності, якщо вона є
        if (abilityType != PetAbilityType.None)
        {
            extra += $"Ability Radius: {abilityRadius}m\n";
        }

        desc.extraStats = extra.Trim();
        desc.priceText = $"Price: <color=#4169E1>{diamondPrice} diamonds</color> ";

        return desc;
    }
}