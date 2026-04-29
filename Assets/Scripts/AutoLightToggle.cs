using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AutoLightToggle : MonoBehaviour
{
    private Light2D myLight;

    [Header("Налаштування розкладу")]
    [Tooltip("Час (від 0 до 1), коли вмикати світло. Наприклад, 0.75 - вечір")]
    public float turnOnTime = 0.75f;

    [Tooltip("Час (від 0 до 1), коли вимикати світло. Наприклад, 0.25 - ранок")]
    public float turnOffTime = 0.25f;

    void Start()
    {
        // Шукаємо світло на цьому об'єкті або на його дітях
        myLight = GetComponent<Light2D>();
        if (myLight == null) myLight = GetComponentInChildren<Light2D>();
    }

    void Update()
    {
        // Якщо менеджера часу немає на сцені - нічого не робимо
        if (DayNightCycle.Instance == null || myLight == null) return;

        // Беремо поточний час доби (від 0 до 1)
        float time = DayNightCycle.Instance.timeProgress;

        // Логіка: вважаємо, що зараз ніч, якщо час БІЛЬШЕ ніж вечір АБО МЕНШЕ ніж ранок
        bool isNight = (time >= turnOnTime || time <= turnOffTime);

        // Вмикаємо або вимикаємо світло
        if (myLight.enabled != isNight)
        {
            myLight.enabled = isNight;
        }
    }
}