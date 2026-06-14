using TMPro;
using UnityEngine;

public class GlobalFontSetter : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;

    void Awake()
    {
        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (TMP_Text t in texts)
            t.font = font;
    }
}