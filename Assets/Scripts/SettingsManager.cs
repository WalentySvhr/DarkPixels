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

    // Ключі для збереження (тепер їх більше)
    private const string MusicKey = "MusicVol";
    private const string SfxKey = "SfxVol";
    private const string StepsKey = "StepsVol";
    private const string CombatKey = "CombatVol";
    private const string LootKey = "LootCoinVol";

    void Start()
    {
        // Ініціалізуємо кожен слайдер окремо
        // Передай: (Слайдер, Ключ збереження, Назва параметра в мікшері)
        InitSlider(musicSlider, MusicKey, "MusicVol");
        InitSlider(sfxSlider, SfxKey, "SFXVol");

        // Нові підгрупи
        InitSlider(stepsSlider, StepsKey, "StepsVol");
        InitSlider(combatSlider, CombatKey, "CombatVol");
        InitSlider(lootCoinSlider, LootKey, "LootCoinVol");

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // Універсальний метод, щоб не писати копіпаст для кожного слайдера
    private void InitSlider(Slider slider, string saveKey, string mixerParam)
    {
        if (slider == null) return;

        // 1. Завантажуємо значення
        float savedValue = PlayerPrefs.GetFloat(saveKey, 0.75f);

        // 2. Встановлюємо візуал слайдера
        slider.value = savedValue;

        // 3. Відразу застосовуємо звук до мікшера при старті
        ApplyVolume(mixerParam, savedValue);

        // 4. Додаємо слухача на зміну (динамічне оновлення)
        slider.onValueChanged.AddListener((value) =>
        {
            ApplyVolume(mixerParam, value);
            PlayerPrefs.SetFloat(saveKey, value);
            // PlayerPrefs.Save() краще викликати один раз при закритті панелі, щоб не "фрізити" мобільний диск
        });
    }

    // Метод для конвертації та встановлення гучності
    private void ApplyVolume(string mixerParam, float value)
    {
        float dB = (value > 0.0001f) ? Mathf.Log10(value) * 20 : -80f;
        mainMixer.SetFloat(mixerParam, dB);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        PlayerPrefs.Save(); // Зберігаємо все разом при закритті
    }
}