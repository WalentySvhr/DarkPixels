using UnityEngine;


// Цей клас є ScriptableObject, який зберігає дані для діалогів з NPC. Він містить ім'я NPC, його портрет (опціонально) та масив реплік, які NPC може сказати. Ці дані можна створювати та редагувати в Інспекторі Unity, а потім використовувати в DialogManager для відображення діалогів під час гри.
// Щоб створити новий діалог, потрібно клікнути правою кнопкою миші в папці Assets, вибрати Create -> RPG -> Dialog, і заповнити поля в Інспекторі. Потім цей DialogData можна передати в метод StartDialog класу DialogManager для початку діалогу з NPC.
[CreateAssetMenu(fileName = "NewDialog", menuName = "RPG/Dialog")]
public class DialogData : ScriptableObject
{
    [Header("Інформація про NPC")]
    public string npcName;
    public Sprite npcPortrait; // Іконка обличчя (за бажанням)

    [Header("Репліки")]
    [TextArea(3, 10)] // Робить віконце для тексту в Інспекторі більшим
    public string[] sentences;
}