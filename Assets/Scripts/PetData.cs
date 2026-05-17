using UnityEngine;

[CreateAssetMenu(fileName = "New Pet", menuName = "Inventory/Pet Data")]
public class PetData : Item // Наслідуємо твій базовий клас предмета
{
    [Header("Pet Settings")]
    public GameObject petPrefab;

    [Header("Ціна в Діамантах (Преміум)")]
    public int diamondPrice = 100;

    [Header("Tooltip Info")]
    [TextArea(2, 4)]
    public string abilityDescription = "Збирає лут навколо гравця";

    public int bonusDamage = 0;
    public float bonusHealth = 0f;

    // Перевизначаємо метод, щоб тултип підхопив саме ці дані
    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // 1. Короткий опис (виводимо тип "Pet")
        desc.shortDesc = "Type: Pet\n";

        // 2. Головні статі (Опис здібності)
        desc.mainStats = abilityDescription;

        // 3. Додаткові статі (якщо пет дає бонуси)
        string extra = "";
        if (bonusDamage > 0) extra += $"Damage: +{bonusDamage}\n";
        if (bonusHealth > 0) extra += $"Health: +{bonusHealth}\n";
        desc.extraStats = extra;

        // 4. Ціна в ДІАМАНТАХ (Форматуємо колір)
        // Замість жовтого gold, пишемо синім diamonds
        desc.priceText = $"Price: <color=#00BFFF>{diamondPrice} diamonds</color>";

        return desc;
    }
}