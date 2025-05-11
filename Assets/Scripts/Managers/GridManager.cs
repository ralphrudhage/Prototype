using System.Collections.Generic;
using Model;
using UnityEngine;

namespace Managers
{
    public class GridManager : MonoBehaviour
    {
        private readonly Dictionary<Vector2Int, GridCell> gridCells = new();
        private readonly Dictionary<Vector2Int, DynamicTile> dynamicTiles = new();

        public static GridManager Instance { get; private set; }
        private readonly HashSet<Vector2Int> validMoveTiles = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            gridCells.Clear();

            foreach (var tile in FindObjectsByType<DynamicTile>(FindObjectsSortMode.None))
            {
                Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.y));

                if (!gridCells.ContainsKey(gridPos))
                {
                    // standard tileType means walkable
                    gridCells[gridPos] = new GridCell(gridPos, walkable: tile.tileType == TileType.Standard);
                    dynamicTiles[gridPos] = tile;
                }
                else
                {
                    Debug.LogWarning($"Duplicate tile at grid position {gridPos}, skipping.");
                }
            }
            
            Debug.Log($"Grid initialized with {gridCells.Count} walkable tiles.");
        }
        
        public bool IsValidMoveTile(Vector2Int pos)
        {
            return validMoveTiles.Contains(pos);
        }

        public bool IsWithinBounds(Vector2Int gridPos)
        {
            return gridCells.ContainsKey(gridPos);
        }

        public Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x, gridPos.y, 0f);
        }

        public Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
        {
            return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        }
        
        public void ShowMoveCircles(Vector2Int from, int range)
        {
            ClearHighlights();
            validMoveTiles.Clear();

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
                new(1, 1), new(-1, 1),
                new(1, -1), new(-1, -1)
            };

            foreach (var dir in directions)
            {
                for (int step = 1; step <= range; step++)
                {
                    Vector2Int target = from + dir * step;

                    if (!gridCells.ContainsKey(target) || !IsWalkable(target))
                        break; // no tile OR not walkable -> stop this direction

                    bool isDiagonal = Mathf.Abs(dir.x) + Mathf.Abs(dir.y) == 2;

                    if (isDiagonal)
                    {
                        Vector2Int horizontal = new(from.x + dir.x, from.y);
                        Vector2Int vertical = new(from.x, from.y + dir.y);

                        // Only block if BOTH adjacent sides are blocked
                        if (!IsWalkable(horizontal) && !IsWalkable(vertical))
                            break;
                    }
                    if (IsOccupied(target))
                        break;
                    
                    validMoveTiles.Add(target);

                    if (dynamicTiles.TryGetValue(target, out var tile))
                        tile.SetHighlight(true);
                }
            }
        }
        
        public void ClearHighlights()
        {
            foreach (var tile in dynamicTiles.Values)
                tile.SetHighlight(false);
        }
        
        public bool HasLineOfSight(Vector2Int from, Vector2Int to)
        {
            Vector2 direction = (to - from);
            int steps = Mathf.CeilToInt(direction.magnitude);

            for (int i = 1; i < steps; i++)
            {
                float t = i / (float)steps;
                Vector2 interpolated = Vector2.Lerp(from, to, t);
                Vector2Int check = new Vector2Int(Mathf.RoundToInt(interpolated.x), Mathf.RoundToInt(interpolated.y));

                if (!gridCells.ContainsKey(check))
                {
                    return false;
                }
            }

            return true;
        }
        
        public void OnHighlightTileClicked(Vector2Int gridPos)
        {
            if (CardManager.Instance.PerformAction())
            {
                PartyManager.Instance.currentPlayer.SetGridPosition(gridPos);
                ClearHighlights();
            }
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            var openSet = new List<PathNode>();
            var visited = new HashSet<Vector2Int>();
            openSet.Add(new PathNode(start, 0, Heuristic(start, goal), new List<Vector2Int> { start }));

            while (openSet.Count > 0)
            {
                openSet.Sort(); // sort by lowest f-cost
                var current = openSet[0];
                openSet.RemoveAt(0);

                if (visited.Contains(current.pos)) continue;
                visited.Add(current.pos);

                if (current.pos == goal)
                    return current.path.GetRange(1, current.path.Count - 1); // skip current pos

                foreach (var neighbor in GetNeighbors(current.pos))
                {
                    if (visited.Contains(neighbor)) continue;
                    if (!gridCells.ContainsKey(neighbor)) continue; // block unwalkable

                    var newPath = new List<Vector2Int>(current.path) { neighbor };
                    int g = current.g + 1;
                    int f = g + Heuristic(neighbor, goal);

                    openSet.Add(new PathNode(neighbor, g, f, newPath));
                }
            }

            return new List<Vector2Int>();
        }

        private int Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }
        
        public List<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            var directions = new List<Vector2Int>
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
                new(1, 1),
                new(-1, 1),
                new(1, -1),
                new(-1, -1)
            };

            var results = new List<Vector2Int>();
            foreach (var dir in directions)
            {
                Vector2Int next = pos + dir;

                if (!gridCells.ContainsKey(next))
                    continue;

                bool isDiagonal = Mathf.Abs(dir.x) + Mathf.Abs(dir.y) == 2;

                if (isDiagonal)
                {
                    Vector2Int horizontal = new Vector2Int(pos.x + dir.x, pos.y);
                    Vector2Int vertical = new Vector2Int(pos.x, pos.y + dir.y);

                    // Only allow diagonal if both adjacent cardinal directions are walkable
                    if (gridCells.ContainsKey(horizontal) && gridCells.ContainsKey(vertical))
                    {
                        results.Add(next);
                    }
                }
                else
                {
                    results.Add(next);
                }
            }

            return results;
        }
        
        public bool IsWalkable(Vector2Int pos)
        {
            bool result = gridCells.TryGetValue(pos, out var cell) && cell.isWalkable;
            // if (!result) Debug.Log($"Tile {pos} is not walkable");
            return result;
        }
        
        private bool IsOccupied(Vector2Int gridPos)
        {
            Vector3 worldPos = GetWorldPosition(gridPos);

            // Check if anything (enemy/player) is standing at that grid position
            var hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null && (hit.GetComponent<Enemy>() != null || hit.GetComponent<Player>() != null))
                return true;

            return false;
        }
    }
}