using TMPro;
using UnityEngine;

public class TextSpawner : MonoBehaviour
{
    [SerializeField] private Transform canvasParent;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject infoTextPrefab;
    
    public void SpawnFloatingText(string param, Vector3 worldPosition)
    {
        var gameLabel = Instantiate(textPrefab, canvasParent);
        gameLabel.transform.position = Camera.main.WorldToScreenPoint(worldPosition);

        var text = gameLabel.GetComponent<TextMeshProUGUI>();
        text.text = param;
        
        Destroy(gameLabel, 1f);
    }
    
    public GameObject SpawnInfoText(string param, Vector3 worldPosition)
    {
        var gameLabel = Instantiate(infoTextPrefab, canvasParent);
        gameLabel.transform.position = Camera.main.WorldToScreenPoint(worldPosition);

        var text = gameLabel.GetComponent<TextMeshProUGUI>();
        text.text = param;
        
        return gameLabel;
    }

    public GameObject SpawnHealthBar()
    {
        return Instantiate(healthBar, canvasParent);
    }
}
