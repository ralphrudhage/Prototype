using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FxManager : MonoBehaviour
{
    private int currentEffectIndex = 0;

    private delegate IEnumerator FxRoutine();

    private List<FxRoutine> effects;

    private void Awake()
    {
        effects = new List<FxRoutine>
        {
            ExplosionEffectCoroutine,
            WhiteOutCoroutine,
            WhiteInCoroutine,
            RandomFlickerWithColorsCoroutine,
            RedWaveCoroutine,
            SnakeFillCoroutine,
        };
    }

    public void Explode()
    {
        StartCoroutine(ExplosionEffectCoroutine());
    }

    public void PlayNextEffect()
    {
        StartCoroutine(effects[currentEffectIndex]());
        currentEffectIndex = (currentEffectIndex + 1) % effects.Count;
    }

    public void ResetToStartingState()
    {
        var tiles = FindObjectsByType<DynamicTile>(FindObjectsSortMode.None);

        foreach (var tile in tiles)
        {
            tile.StartingTile();
        }
    }

    private IEnumerator WhiteOutCoroutine()
    {
        var tiles = FindObjectsByType<DynamicTile>(FindObjectsSortMode.None)
            .OrderByDescending(t => t.transform.position.y)
            .ThenBy(t => t.transform.position.x)
            .ToList();

        foreach (var tile in tiles)
        {
            tile.WhiteTile();
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator WhiteInCoroutine()
    {
        var tiles = FindObjectsByType<DynamicTile>(FindObjectsSortMode.None)
            .OrderBy(t => t.transform.position.y)
            .ThenByDescending(t => t.transform.position.x)
            .ToList();

        foreach (var tile in tiles)
        {
            tile.StartingTile();
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator RandomFlickerWithColorsCoroutine()
    {
        var tiles = FindObjectsByType<DynamicTile>(FindObjectsSortMode.None).ToList();
        var shuffled = tiles.OrderBy(_ => Random.value).ToList();

        foreach (var tile in shuffled)
        {
            tile.WhiteTile();

            var randomColor = new Color32(
                (byte)Random.Range(80, 255),
                (byte)Random.Range(80, 255),
                (byte)Random.Range(80, 255),
                255
            );

            tile.ColorTile(randomColor);
            yield return new WaitForSeconds(0.02f);
            tile.StartingTile();
        }
    }

    private IEnumerator RedWaveCoroutine()
    {
        var tiles = FindObjectsByType<DynamicTile>(FindObjectsSortMode.None)
            .OrderByDescending(t => t.transform.position.y)
            .ThenBy(t => t.transform.position.x)
            .ToList();

        foreach (var tile in tiles)
        {
            tile.ColorTile(GameUtils.lightRed);
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator SnakeFillCoroutine()
    {
        var tiles = FindObjectsByType<DynamicTile>(FindObjectsSortMode.None)
            .GroupBy(t => Mathf.RoundToInt(t.transform.position.y)) // group by row (Y)
            .OrderBy(g => g.Key) // bottom to top
            .SelectMany((rowGroup, index) =>
                    index % 2 == 0
                        ? rowGroup.OrderBy(t => t.transform.position.x) // even row: left to right
                        : rowGroup.OrderByDescending(t => t.transform.position.x) // odd row: right to left
            )
            .ToList();

        foreach (var tile in tiles)
        {
            tile.WhiteTile();
            yield return new WaitForSeconds(0.015f);
        }
    }
    
    private IEnumerator ExplosionEffectCoroutine()
    {
        var tiles = FindObjectsByType<DynamicTile>(FindObjectsSortMode.None).ToList();

        // Center = average tile position
        Vector2 avgPos = tiles.Select(t => (Vector2)t.transform.position).Aggregate(Vector2.zero, (sum, pos) => sum + pos) / tiles.Count;
        DynamicTile centerTile = tiles.OrderBy(t => Vector2.Distance(t.transform.position, avgPos)).First();
        Vector2Int center = Vector2Int.RoundToInt(centerTile.transform.position);


        var localTiles = tiles
            .Where(t =>
            {
                Vector2Int pos = Vector2Int.RoundToInt(t.transform.position);
                float dist = Vector2.Distance(new Vector2(pos.x, pos.y), new Vector2(center.x, center.y));
                return dist <= 2.5f; // 2.5 gives a good approximation of a "round" 5x5 area
            })
            .ToList();


        var groups = localTiles
            .GroupBy(t => Mathf.RoundToInt(Vector2.Distance(t.transform.position, center)))
            .OrderBy(g => g.Key);

        float rippleDistance = 0.2f;
        float waitBetweenRings = 0.05f;

        foreach (var group in groups)
        {
            foreach (var tile in group)
            {
                // fire colors
                /* 
                Color32 color = group.Key switch
                {
                    0 => new Color32(255, 100, 30, 255),
                    1 => new Color32(255, 180, 60, 255),
                    2 => new Color32(255, 240, 100, 255),
                    _ => new Color32(255, 255, 150, 255)
                };*/
                
                // plasma
                Color32 color = group.Key switch
                {
                    0 => new Color32(80, 150, 255, 255),   // electric blue core
                    1 => new Color32(60, 120, 240, 255),   // cooler mid ring
                    2 => new Color32(40, 90, 200, 255),    // deep plasma blue
                    _ => new Color32(30, 60, 160, 255)     // fading outer rim
                };
                
                tile.SetHighlight(true);
                tile.HighLightTileColor(color);
                tile.FadeAndDisableHighlight();

                // Push outward
                Vector3 dir = tile.transform.position - new Vector3(center.x, center.y, 0f);
                Vector3 targetPos = tile.GetOriginalPosition().position + dir * rippleDistance;

                StartCoroutine(RippleReturn(tile, targetPos, tile.GetOriginalPosition().position, color));
            }

            yield return new WaitForSeconds(waitBetweenRings);
        }
        
        // After explosion, apply ripple to nearby surrounding tiles (but not colored ones)
        foreach (var tile in tiles.Except(localTiles))
        {
            Vector2Int pos = Vector2Int.RoundToInt(tile.transform.position);
            float dist = Vector2.Distance(pos, center);
            
            if (dist > 2.5f && dist <= 3.5f) // just outside explosion
            {
                float delay = (dist - 2.5f) * waitBetweenRings;
                StartCoroutine(ApplyRippleOnly(tile, delay, rippleDistance * 0.5f, center));
            }
        }
    }

    
    private IEnumerator RippleReturn(DynamicTile tile, Vector3 pushedPos, Vector3 originalPos, Color32 color)
    {
        // Move to pushed position
        tile.transform.position = pushedPos;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            tile.transform.position = Vector3.Lerp(pushedPos, originalPos, t);
            yield return null;
        }

        tile.StartingTile();
        tile.transform.position = originalPos;
    }
    
    private IEnumerator ApplyRippleOnly(DynamicTile tile, float delay, float pushAmount, Vector2 center)
    {
        yield return new WaitForSeconds(delay);

        Vector3 direction = (tile.transform.position - new Vector3(center.x, center.y)).normalized;
        Vector3 original = tile.GetOriginalPosition().position;
        Vector3 pushed = original + direction * pushAmount;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin((elapsed / duration) * Mathf.PI); // spring in/out
            tile.transform.position = Vector3.Lerp(original, pushed, t);
            yield return null;
        }

        tile.transform.position = original;
    }
}