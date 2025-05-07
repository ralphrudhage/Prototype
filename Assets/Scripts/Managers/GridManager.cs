using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Managers
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Tilemap gridTilemap;
        [SerializeField] private GameObject circlePrefab;

        private readonly List<GameObject> activeCircles = new();
        private readonly Dictionary<Vector2Int, GridCell> gridCells = new();

        public static GridManager Instance { get; private set; }

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

            foreach (var cellPos in gridTilemap.cellBounds.allPositionsWithin)
            {
                if (!gridTilemap.HasTile(cellPos)) continue;

                var gridPos = new Vector2Int(cellPos.x, cellPos.y);
                if (gridPos.y == 0)
                {
                    // Debug.Log($"Adding walkable tile: {gridPos}");
                }
                gridCells[gridPos] = new GridCell(gridPos, walkable: true);
            }
        }

        public bool IsWithinBounds(Vector2Int gridPos)
        {
            return gridCells.ContainsKey(gridPos);
        }

        public Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            return gridTilemap.GetCellCenterWorld((Vector3Int)gridPos);
        }

        public Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
        {
            var cellPos = gridTilemap.WorldToCell(worldPos);
            return new Vector2Int(cellPos.x, cellPos.y);
        }


        public void ShowMoveCircles(Vector2Int from, int range)
        {
            ClearCircles();

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

                    if (!gridCells.ContainsKey(target)) break;

                    // Block diagonal moves through corners
                    bool isDiagonal = Mathf.Abs(dir.x) + Mathf.Abs(dir.y) == 2;

                    if (isDiagonal)
                    {
                        Vector2Int horizontal = new Vector2Int(from.x + dir.x, from.y);
                        Vector2Int vertical = new Vector2Int(from.x, from.y + dir.y);

                        if (!gridCells.ContainsKey(horizontal) || !gridCells.ContainsKey(vertical))
                            break; // skip and stop stepping further in this diagonal
                    }

                    Vector3 worldPos = GetWorldPosition(target);
                    GameObject circle = Instantiate(circlePrefab, worldPos, Quaternion.identity);
                    circle.GetComponent<SelectCell>().Init(target);
                    activeCircles.Add(circle);
                }
            }
        }


        private void ClearCircles()
        {
            foreach (var obj in activeCircles)
            {
                Destroy(obj);
            }

            activeCircles.Clear();
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


        /*
        private void OnDrawGizmos()
        {
            if (gridTilemap == null || gridCells == null) return;

            Gizmos.color = Color.green;

            foreach (var cell in gridCells.Values)
            {
                var center = GetWorldPosition(cell.position);
                Gizmos.DrawWireCube(center, Vector3.one);
            }
        }
        */

        public void OnHighlightTileClicked(Vector2Int gridPos)
        {
            if (CardManager.Instance.PerformAction())
            {
                PartyManager.Instance.currentPlayer.SetGridPosition(gridPos);
                ClearCircles();
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

    }
}