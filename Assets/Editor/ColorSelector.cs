using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(ColorApplier))]
public class ColorSelector : Editor
{
    public override void OnInspectorGUI()
    {
        var gameObject = ((ColorApplier)target).gameObject;

        if (GUILayout.Button("Select Color"))
        {
            var menu = new GenericMenu();

            AddColorMenuItem(menu, "Blackish", GameUtils.blackish, gameObject);
            AddColorMenuItem(menu, "White", GameUtils.white, gameObject);
            AddColorMenuItem(menu, "Light Yellow", GameUtils.lightYellow, gameObject);
            AddColorMenuItem(menu, "Light Blue", GameUtils.lightBlue, gameObject);
            AddColorMenuItem(menu, "Dark Blue", GameUtils.darkBlue, gameObject);
            AddColorMenuItem(menu, "Darkish", GameUtils.darkish, gameObject);
            AddColorMenuItem(menu, "Darker", GameUtils.darker, gameObject);
            AddColorMenuItem(menu, "Light Red", GameUtils.lightRed, gameObject);
            AddColorMenuItem(menu, "Gray", GameUtils.gray, gameObject);
            AddColorMenuItem(menu, "Light Green", GameUtils.lightGreen, gameObject);
            AddColorMenuItem(menu, "Green", GameUtils.green, gameObject);
            AddColorMenuItem(menu, "Dark Green", GameUtils.darkGreen, gameObject);
            AddColorMenuItem(menu, "Light Cyan", GameUtils.lightCyan, gameObject);
            AddColorMenuItem(menu, "Sky Blue", GameUtils.skyBlue, gameObject);
            AddColorMenuItem(menu, "Night Blue", GameUtils.nightBlue, gameObject);
            AddColorMenuItem(menu, "Hoofish", GameUtils.hoofish, gameObject);
            
            menu.ShowAsContext();
        }
    }

    private void AddColorMenuItem(GenericMenu menu, string name, Color color, GameObject gameObject)
    {
        menu.AddItem(new GUIContent(name), false, () => ApplyColor(color, gameObject));
    }

    private void ApplyColor(Color color, GameObject gameObject)
    {
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
            return;
        }

        var image = gameObject.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
            return;
        }

        var textMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.color = color;
            return;
        }
        
        var text = gameObject.GetComponent<Text>();
        if (text != null)
        {
            text.color = color;
            return;
        }
    }
}
