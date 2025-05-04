using UnityEngine;

namespace Model
{
    public class GridCell
    {
        public Vector2Int position;
        public bool isWalkable;

        public GridCell(Vector2Int pos, bool walkable = true)
        {
            position = pos;
            isWalkable = walkable;
        }
    }
}