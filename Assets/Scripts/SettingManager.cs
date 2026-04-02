using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public Toggle musicToggle; // Сюди перетягни Тогл музики з ієрархії
    public Toggle sfxToggle;   // Сюди перетягни Тогл звуків з ієрархії

    void Start()
    {
        // 1. Завантажуємо збережені значення (0 - вимкнено, 1 - увімкнено)
        // Якщо раніше нічого не зберігали, за замовчуванням ставимо 1 (увімкнено)
        int musicStatus = PlayerPrefs.GetInt("MusicEnabled", 1);
        int sfxStatus = PlayerPrefs.GetInt("SFXEnabled", 1);

        // 2. Застосовуємо ці значення до AudioSource (інвертуємо, бо mute працює навпаки)
        musicSource.mute = (musicStatus == 0);
        sfxSource.mute = (sfxStatus == 0);

        // 3. Оновлюємо візуал самих Тоглів (щоб галочки стояли правильно)
        // Ми використовуємо onValueChanged.RemoveAllListeners, щоб при старті 
        // скрипт не викликав сам себе і не крутив логіку по колу
        musicToggle.isOn = (musicStatus == 0); // Якщо mute, то Toggle On (перекреслений динамік)
        sfxToggle.isOn = (sfxStatus == 0);
    }

    // Метод для музики (прив'язаний до On Value Changed)
    public void SetMusicMute(bool isMuted)
    {
        musicSource.mute = isMuted;
        // Зберігаємо: 0 якщо замучено, 1 якщо грає
        PlayerPrefs.SetInt("MusicEnabled", isMuted ? 0 : 1);
        PlayerPrefs.Save(); // Обов'язково для Android, щоб дані записалися в пам'ять
    }

    // Метод для звуків (прив'язаний до On Value Changed)
    public void SetSFXMute(bool isMuted)
    {
        sfxSource.mute = isMuted;
        PlayerPrefs.SetInt("SFXEnabled", isMuted ? 0 : 1);
        PlayerPrefs.Save();
    }
}