using UnityEngine;

public enum WeaponType { Melee, Ranged }

[CreateAssetMenu(fileName = "NewWeapon", menuName = "RPG/Weapon")]
public class WeaponData : Item
{
    // Ми ПРИБРАЛИ public int price; бо він вже є у батьківському класі Item
    // Ми ПЕРЕЙМЕНУВАЛИ type на weaponType, щоб не було конфлікту з Item.type

    [Header("Налаштування зброї")]
    public WeaponType weaponType;



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

    public void UpdateAnimationSpeed(Animator anim)
    {
        if (anim != null)
        {
            // Якщо кулдаун 0.5с, швидкість має бути 1 / 0.5 = 2 (вдвічі швидше)
            float speedMultiplier = 1f / cooldown;
            anim.SetFloat("AttackSpeed", speedMultiplier);
        }
    }
}