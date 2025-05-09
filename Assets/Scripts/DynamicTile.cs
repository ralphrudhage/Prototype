using System.Collections.Generic;
using UnityEngine;

public class DynamicTile : MonoBehaviour
{
    [SerializeField] private GameObject highLight;
    [SerializeField] private List<Sprite> dynamicSprites;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = PickBiasedSprite();
        highLight.SetActive(false);
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
}

