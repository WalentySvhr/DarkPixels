using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;

    [Header("Main Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Detailed SFX Sliders")]
    public Slider stepsSlider;   // Слайдер для кроків
    public Slider combatSlider;  // Слайдер для ударів/пострілів
    public Slider lootCoinSlider;    // Слайдер для монет/луту

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    // Ключі для збереження в PlayerPrefs
    private const string MusicKey = "MusicVol";
    private const string SfxKey = "SfxVol";
    private const string StepsKey = "StepsVol";
    private const string CombatKey = "CombatVol";
    private const string LootKey = "LootCoinVol";

    void Start()
    {
        if (mainMixer == null)
        {
            Debug.LogError("Менеджер налаштувань: Не забудь перетягнути MainMixer в Інспекторі!");
            return;
        }

        // Ініціалізуємо кожен слайдер окремо
        // Передаємо: (Слайдер, Ключ збереження, НАЗВА ЕКСПОНОВАНОГО ПАРАМЕТРА в мікшері)
        InitSlider(musicSlider, MusicKey, "MusicVol");
        InitSlider(sfxSlider, SfxKey, "SFXVol");

        // Додаткові підгрупи ефектів
        InitSlider(stepsSlider, StepsKey, "StepsVol");
        InitSlider(combatSlider, CombatKey, "CombatVol");
        InitSlider(lootCoinSlider, LootKey, "LootCoinVol");

        // Ховаємо панель при старті гри, якщо забули сховати в редакторі
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // Універсальний метод ініціалізації слайдерів
    private void InitSlider(Slider slider, string saveKey, string mixerParam)
    {
        // Якщо слайдер не призначений на цій сцені (наприклад, загальний SFXSlider в меню порожній) — просто пропускаємо
        if (slider == null) return;

        // 1. Завантажуємо збережене значення (якщо гра запускається вперше — ставимо 0.75f, тобто 75% гучності)
        float savedValue = PlayerPrefs.GetFloat(saveKey, 0.75f);

        // 2. Встановлюємо візуальне положення бігунка
        slider.value = savedValue;

        // 3. Відразу застосовуємо звук до мікшера при завантаженні сцени
        ApplyVolume(mixerParam, savedValue);

        // 4. Динамічно відстежуємо рух повзунка користувачем
        slider.onValueChanged.AddListener((value) =>
        {
            ApplyVolume(mixerParam, value);
            PlayerPrefs.SetFloat(saveKey, value);
        });
    }

    // Переведення значень слайдера (0...1) у децибели мікшера (-80...0)
    private void ApplyVolume(string mixerParam, float value)
    {
        float dB = (value > 0.0001f) ? Mathf.Log10(value) * 20 : -80f;
        mainMixer.SetFloat(mixerParam, dB);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Time.timeScale = 0f; // Пауза гри для геймплейної сцени
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Time.timeScale = 1f; // Знімаємо паузу
            PlayerPrefs.Save();  // Жорстко записуємо налаштування на диск при закритті вікна
        }
    }
}