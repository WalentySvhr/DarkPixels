using UnityEngine;

public class WeaponAnimationController : MonoBehaviour
{
    private Animator anim;
    private float lastClickTime;
    public float baseSpeed = 1f;
    public float maxSpeed = 3f; // Максимальне прискорення

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayAttack()
    {
        if (anim != null)
        {
            // Рахуємо час з моменту останнього кліку
            float timeSinceLastClick = Time.time - lastClickTime;
            lastClickTime = Time.time;

            // Якщо клікнули швидко (наприклад, швидше ніж за 0.5 сек)
            // Швидкість буде вищою. Чим менше timeSinceLastClick, тим вище швидкість.
            float targetSpeed = Mathf.Clamp(1.0f / timeSinceLastClick, baseSpeed, maxSpeed);

            // Передаємо швидкість в Animator
            anim.SetFloat("AnimSpeed", targetSpeed);

            // Запускаємо анімацію
            anim.Play("Sword_Anim", 0, 0f);
        }
    }

    void Update()
    {
        // Поступово повертаємо швидкість до 1, якщо ми перестали клікати
        if (anim != null)
        {
            float currentSpeed = anim.GetFloat("AnimSpeed");
            anim.SetFloat("AnimSpeed", Mathf.Lerp(currentSpeed, baseSpeed, Time.deltaTime * 2f));
        }
    }
}