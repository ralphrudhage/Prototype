using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] GameObject circlePrefab;
        private List<GameObject> activeCircles = new();

        private PlayerController player;

        private int width = 20;
        private int height = 7;
        
        public Vector2 cellSize = Vector2.one;

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
        }

        public Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            return transform.position + new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0);
        }

        public bool IsWithinBounds(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < width && gridPos.y < height;
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 cellCenter = transform.position +
                                         new Vector3((x + 0.5f) * cellSize.x, (y + 0.5f) * cellSize.y, 0);
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize.x, cellSize.y, 0));
                }
            }
        }

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