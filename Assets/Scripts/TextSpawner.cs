using TMPro;
using UnityEngine;

public class TextSpawner : MonoBehaviour
{
    [SerializeField] private Transform canvasParent;
    [SerializeField] private Transform canvasScreenParent;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject apMonitor;
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject infoTextPrefab;
    [SerializeField] private GameObject playerInfoPrefab;
    
    public void SpawnFloatingText(string param, Vector3 worldPosition, Color32 color)
    {
        var gameLabel = Instantiate(textPrefab, canvasParent);
        gameLabel.transform.position = worldPosition;

        var text = gameLabel.GetComponent<TextMeshProUGUI>();
        text.text = param;
        text.color = color;
        
        Destroy(gameLabel, 1f);
    }
    
    public GameObject SpawnInfoText(string param, Vector3 worldPosition)
    {
        var gameLabel = Instantiate(infoTextPrefab, canvasParent);
        gameLabel.transform.position = worldPosition;

        var text = gameLabel.GetComponent<TextMeshProUGUI>();
        text.text = param;
        
        return gameLabel;
    }

    public GameObject SpawnPlayerInfo()
    {
        return Instantiate(playerInfoPrefab, canvasScreenParent);
    }
    
    public GameObject SpawnHealthBar()
    {
        return Instantiate(healthBar, canvasParent);
    }
}
