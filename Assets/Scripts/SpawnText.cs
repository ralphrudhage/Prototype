using TMPro;
using UnityEngine;

public class SpawnText : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab;
    
    public void SpawnFloatingText(string param, Vector3 worldPosition)
    {
        var gameLabel = Instantiate(textPrefab, transform);
        gameLabel.transform.position = Camera.main.WorldToScreenPoint(worldPosition);

        var text = gameLabel.GetComponent<TextMeshProUGUI>();
        text.text = param;
        
        Destroy(gameLabel, 1f);
    }
}
