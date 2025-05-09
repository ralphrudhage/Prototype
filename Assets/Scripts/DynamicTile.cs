using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DynamicTile : MonoBehaviour
{
    [SerializeField] private Sprite whiteSprite;
    [SerializeField] private GameObject highLight;
    [SerializeField] private List<Sprite> dynamicSprites;
    
    private SpriteRenderer spriteRenderer;
    private Sprite startingSprite;
    private Transform originalPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startingSprite = PickBiasedSprite();
        spriteRenderer.sprite = startingSprite;
        highLight.SetActive(false);
        originalPosition = transform;
    }

    private Sprite PickBiasedSprite()
    {
        float roll = Random.value;
        
        if (roll < 0.8f || dynamicSprites.Count == 1)
        {
            return dynamicSprites[0];
        }
        
        int index = Random.Range(1, dynamicSprites.Count);
        return dynamicSprites[index];
    }
    
    public void SetHighlight(bool isHighlighted)
    {
        highLight.SetActive(isHighlighted);
    }

    public void WhiteTile()
    {
        spriteRenderer.sprite = whiteSprite;
        spriteRenderer.color = Color.white;
    }

    public void StartingTile()
    {
        spriteRenderer.sprite = startingSprite;
        spriteRenderer.color = Color.white;
        transform.position = originalPosition.position;
    }
    
    public void ColorTile(Color32 color)
    {
        spriteRenderer.color = color;
    }
    
    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    public Transform GetOriginalPosition()
    {
        return originalPosition;
    }
}

