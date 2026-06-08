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
        Debug.Log($"Об'єкт {other.name} увійшов у зону. Тег: {other.tag}"); // ДОДАЙТЕ ЦЕ

        if (other.CompareTag("Player") && zonePlaylist.Count > 0)
        {
            Debug.Log("Умова виконана, викликаю ChangeZonePlaylist"); // ДОДАЙТЕ ЦЕ
            ContinuousPlaylist.instance?.ChangeZonePlaylist(zonePlaylist, shuffleThisZone);
        }
    }
}