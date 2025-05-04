using Managers;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI energyText;
    
    public Vector2Int currentGridPos;
    
    private int currentEnergy;
    
    void Start()
    {
        energyText.text = "";
        currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
    }
    
    public void SetGridPosition(Vector2Int gridPos)
    {
        currentGridPos = gridPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPos);
    }

    public void UseEnergy(int energy)
    {
        currentEnergy -= energy;
        energyText.text = currentEnergy.ToString();
    }

    public int GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public void ResetEnergy()
    {
        currentEnergy = 300;
        energyText.text = currentEnergy.ToString();
    }
}