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
        private PlayerController player;
        
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
            player = FindAnyObjectByType<PlayerController>();
            gridCells.Clear();

            foreach (var cellPos in gridTilemap.cellBounds.allPositionsWithin)
            {
                if (!gridTilemap.HasTile(cellPos)) continue;

                var gridPos = new Vector2Int(cellPos.x, cellPos.y);
                gridCells[gridPos] = new GridCell(gridPos, walkable: true);
            }
        }

        public bool IsWithinBounds(Vector2Int gridPos)
        {
            return gridCells.ContainsKey(gridPos);
        }

        public Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            return gridTilemap.GetCellCenterWorld((Vector3Int) gridPos);
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
                new(1, 1), // up-right
                new(-1, 1), // up-left
                new(1, -1), // down-right
                new(-1, -1) // down-left
            };

            foreach (var dir in directions)
            {
                for (int step = 1; step <= range; step++)
                {
                    Vector2Int target = from + dir * step;
                    if (!IsWithinBounds(target)) break;

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
            if (ActionManager.Instance.PerformAction())
            {
                player.SetGridPosition(gridPos);
                ClearCircles();
            }
        }
    }
}