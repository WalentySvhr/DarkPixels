using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;

public class FontReplacer : EditorWindow
{
    public TMP_FontAsset newFont;

    [MenuItem("Tools/Replace All Fonts")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacer>("Replace Fonts");
    }

    private void OnGUI()
    {
        GUILayout.Label("Заміна шрифтів TMP всюди", EditorStyles.boldLabel);

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Новий шрифт TMP", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Замінити на Сцені та в Префабах"))
        {
            if (newFont == null)
            {
                Debug.LogError("Спершу перетягни новий шрифт у поле!");
                return;
            }

            // 1. Заміна на поточній сцені
            TextMeshProUGUI[] sceneTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int sceneCount = 0;
            foreach (var textComp in sceneTexts)
            {
                Undo.RecordObject(textComp, "Replace Font");
                textComp.font = newFont;
                EditorUtility.SetDirty(textComp);
                sceneCount++;
            }

            // 2. Заміна в усіх префабах у папці Assets
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            int prefabCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    TextMeshProUGUI[] prefabTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                    if (prefabTexts.Length > 0)
                    {
                        foreach (var textComp in prefabTexts)
                        {
                            textComp.font = newFont;
                        }
                        EditorUtility.SetDirty(prefab);
                        prefabCount += prefabTexts.Length;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Успішно! Замінено на сцені: {sceneCount} об'єктів. В префабах: {prefabCount} об'єктів.");
        }
    }
}
#endif