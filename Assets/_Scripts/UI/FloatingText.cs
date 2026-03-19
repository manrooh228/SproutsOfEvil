using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public void SetText(string text, Color color)
    {
        var tmp = GetComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        Destroy(gameObject, 1.1f);
    }
}
