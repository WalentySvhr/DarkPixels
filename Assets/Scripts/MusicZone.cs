using UnityEngine;
using System.Collections.Generic;

public class MusicZone : MonoBehaviour
{
    [Header("Список треків ДЛЯ ЦІЄЇ ЗОНИ")]
    public List<AudioClip> zonePlaylist = new List<AudioClip>();

    [Header("Чи перемішувати треки в цій зоні?")]
    public bool shuffleThisZone = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Переконайся, що на твоєму персонажі в Інспекторі стоїть Tag "Player"
        if (other.CompareTag("Player") && zonePlaylist.Count > 0)
        {
            ContinuousPlaylist.instance?.ChangeZonePlaylist(zonePlaylist, shuffleThisZone);
        }
    }
}