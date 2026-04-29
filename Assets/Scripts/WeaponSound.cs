using UnityEngine;

public class WeaponSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip hitSound; // Це поле у тебе вже заповнене на скріншоті

    // Метод для виклику звуку удару
    public void PlayHit()
    {
        if (audioSource != null && hitSound != null)
        {
            // Рандомізуємо Pitch, щоб кожен удар звучав унікально
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.PlayOneShot(hitSound);
        }
    }
}