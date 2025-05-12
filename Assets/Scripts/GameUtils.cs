using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public static class GameUtils
{
    public static Color32 lightYellow = new(254, 231, 97, 255);
    public static Color32 lightBlue = new(0, 149, 233, 255);
    public static Color32 skyBlue = new(44, 232, 245, 255);
    public static Color32 darkBlue = new(18, 78, 137, 255);
    public static Color32 nightBlue = new(58, 68, 102, 255);
    public static Color32 darkish = new(38, 43, 68, 251);
    public static Color32 darker = new(24, 20, 37, 251);
    public static Color32 lightRed = new(247, 118, 34, 255);
    public static Color32 gray = new(139, 155, 180, 255);
    public static Color32 lightGreen = new(99, 199, 77, 255);
    public static Color32 green = new(62, 137, 72, 255);
    public static Color32 darkGreen = new(38, 92, 66, 255);
    public static Color32 lightCyan = new(192, 203, 220, 255);
    public static Color32 pressedWhite = new(255, 255, 255, 10);
    public static Color32 white = new(255, 255, 255, 255);
    public static Color32 blackish = new(24, 20, 37, 255);
    public static Color32 hoofish = new(234, 212, 170, 255);
    public static Color32 brownie = new(116, 63, 57, 255);
    
    private static readonly Random rng = new();
    
    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
    public static void DestroyTagRecursively(this Transform trans, string tag)
    {
        foreach (Transform child in trans)
        {
            if (child.CompareTag(tag))
            {
                Object.Destroy(child.gameObject);
            }
            else if (child.childCount > 0)
            {
                DestroyTagRecursively(child, tag);
            }
        }
    }
}