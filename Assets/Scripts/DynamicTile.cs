using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Standard,
    Wall
}

public class DynamicTile : MonoBehaviour
{
    [SerializeField] private Sprite whiteSprite;
    [SerializeField] private GameObject highLight;
    [SerializeField] private GameObject playerHighLight;
    [SerializeField] private List<Sprite> dynamicSprites;

    private SpriteRenderer highlightSpriteRenderer;
    private SpriteRenderer spriteRenderer;
    private Sprite startingSprite;
    private Transform originalPosition;
    public TileType tileType = TileType.Standard;

    private void Awake()
    {
        highlightSpriteRenderer = highLight.GetComponent<SpriteRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        startingSprite = tileType == TileType.Standard ? PickBiasedSprite() : spriteRenderer.sprite;
        spriteRenderer.sprite = startingSprite;
        if (tileType == TileType.Standard)
        {
            highLight.SetActive(false);
            playerHighLight.SetActive(false);
        }

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

    public void ActivatePlayerHighlight(bool isHighlighted)
    {
        playerHighLight.SetActive(isHighlighted);
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

    public void HighLightTileColor(Color32 color)
    {
        highlightSpriteRenderer.color = color;
    }

    public Transform GetOriginalPosition()
    {
        return originalPosition;
    }

    public void FadeAndDisableHighlight(float duration = 0.5f, float startDelay = 0.25f)
    {
        StartCoroutine(FadeHighlightCoroutine(duration, startDelay));
    }

    private IEnumerator FadeHighlightCoroutine(float duration, float startDelay)
    {
        var sr = highlightSpriteRenderer;
        Color originalColor = sr.color;

        // ⏳ Initial delay before fade starts
        yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Ensure fully transparent
        SetHighlight(false);
    }
}