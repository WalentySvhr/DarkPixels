using UnityEngine;

// Виносимо enum за межі класу, щоб він був доступний всюди
public enum WeaponType { Melee, Ranged }

[CreateAssetMenu(fileName = "NewWeapon", menuName = "RPG/Weapon")]
public class WeaponData : Item
{
    [Header("Налаштування зброї")]
    public WeaponType weaponType; // ТЕПЕР ТУТ ПРАВИЛЬНИЙ ТИП

    [Header("Бойові характеристики")]
    public int damage;
    public float attackRange;
    public float cooldown;
    public float damageDelay;

    [Header("Для ближнього бою")]
    public float knockbackForce = 10f;

    [Header("Для дальнього бою")]
    public GameObject projectilePrefab;
    public float shootForce;

    [Header("Візуал")]
    public GameObject visualPrefab;

    public override ItemDescription GetDetailedInfo()
    {
        ItemDescription desc = new ItemDescription();

        // Блок 1: Основні бойові показники
        // Використовуємо наш WeaponType для перевірки
        string typeName = (weaponType == WeaponType.Melee) ? "Melee" : "Ranged";

        desc.mainStats = $"Type: {typeName}\n" +
                         $"Damage: {damage}\n" +
                         $"Range: {attackRange}m";

        // Блок 2: Додаткові показники
        float attacksPerSecond = (cooldown > 0) ? (1f / cooldown) : 0;
        string extra = $"AttackSpeed: {attacksPerSecond:F1} atk/s\n";

        if (weaponType == WeaponType.Melee)
        {
            if (knockbackForce > 0) extra += $"Knockback: {knockbackForce}";
        }
        else
        {
            if (shootForce > 0) extra += $"ShootForce: {shootForce} m/s";
        }

        desc.extraStats = extra.TrimEnd();

        // Блок 3: Ціна
        if (price > 0)
            desc.priceText = $"Price: {price} gold";

        return desc;
    }

    public void UpdateAnimationSpeed(Animator anim)
    {
        if (anim != null && cooldown > 0)
        {
            float speedMultiplier = 1f / cooldown;
            anim.SetFloat("AttackSpeed", speedMultiplier);
        }
    }
}