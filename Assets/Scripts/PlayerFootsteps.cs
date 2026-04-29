using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Components")]
    public AudioSource audioSource;
    public AudioClip[] stepSounds; // Масив звуків, щоб кроки не були одноманітними

    [Header("Settings")]
    [Range(0, 1)] public float volume = 0.5f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    private void Awake()
    {
        // Якщо забув перетягнути AudioSource, скрипт спробує знайти його сам
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // Цей метод ми викликаємо через Animation Event
    public void PlayFootstep()
    {
        if (audioSource != null && stepSounds.Length > 0)
        {
            // Вибираємо випадковий звук із масиву
            int randomIndex = Random.Range(0, stepSounds.Length);
            AudioClip clip = stepSounds[randomIndex];

            // Рандомізуємо висоту звуку (pitch), щоб кроки звучали природніше
            audioSource.pitch = Random.Range(minPitch, maxPitch);

            // Відтворюємо звук
            audioSource.PlayOneShot(clip, volume);
        }
    }
}