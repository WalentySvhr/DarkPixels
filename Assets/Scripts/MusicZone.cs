using UnityEngine;
using System.Collections.Generic;

public class MusicZone : MonoBehaviour
{
    [Header("Список треків ДЛЯ ЦІЄЇ ЗОНИ")]
    public List<AudioClip> zonePlaylist = new List<AudioClip>();

    [Header("Чи перемішувати треки в цій зоні?")]
    public bool shuffleThisZone = false;

    [Header("Музика при ВИХОДІ з цієї зони (Опціонально)")]
    [Tooltip("Якщо це маленька зона (місто), сюди можна скинути треки великої зони (опенворлду)")]
    public List<AudioClip> previousZonePlaylist = new List<AudioClip>();
    public bool shufflePreviousZone = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zonePlaylist.Count > 0)
        {
            Debug.Log($"[MusicZone] Гравець увійшов у зону '{gameObject.name}'. Вмикаю її плейлист.");
            ContinuousPlaylist.instance?.ChangeZonePlaylist(zonePlaylist, shuffleThisZone);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Якщо гравець виходить і у нас налаштований плейлист для повернення
        if (other.CompareTag("Player") && previousZonePlaylist.Count > 0)
        {
            Debug.Log($"[MusicZone] Гравець вийшов із зони '{gameObject.name}'. Повертаю попередній плейлист.");
            ContinuousPlaylist.instance?.ChangeZonePlaylist(previousZonePlaylist, shufflePreviousZone);
        }
    }
}