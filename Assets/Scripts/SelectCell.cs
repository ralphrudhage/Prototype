using Managers;
using UnityEngine;

public class SelectCell : MonoBehaviour
{
    private Vector2Int gridPosition;

    public void Init(Vector2Int pos)
    {
        gridPosition = pos;
    }

    private void OnMouseDown()
    {
        GridManager.Instance.OnHighlightTileClicked(gridPosition);
    }
}
