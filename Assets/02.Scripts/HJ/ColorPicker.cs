using UnityEngine;
using System.Collections;

public class ColorPicker : MonoBehaviour
{
    public static ColorPicker instance;

    public Color color;

    private void Awake()
    {
        instance = this;
    }

    public void OnClickColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out color);
    }

}
