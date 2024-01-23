using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColouredString
{
    public static string Colorize(string text, string color, bool bold = false)
    {
        return
        "<color=#" + color + ">" +
        (bold ? "<b>" : "") +
        text +
        (bold ? "</b>" : "") +
        "</color>";
    }
}
