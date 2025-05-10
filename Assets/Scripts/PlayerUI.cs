using Model;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI warriorCards;
    [SerializeField] private TextMeshProUGUI warriorDiscarded;
    
    [SerializeField] private TextMeshProUGUI priestCards;
    [SerializeField] private TextMeshProUGUI priestDiscarded;
    
    [SerializeField] private TextMeshProUGUI mageCards;
    [SerializeField] private TextMeshProUGUI mageDiscarded;
    
    public static PlayerUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    public void UpdatePlayerUI(Player player)
    {
        switch (player.playerClass)
        {
            case PlayerClass.WARRIOR:
                warriorCards.text = "Cards: " + player.drawPile.Count;
                warriorDiscarded.text = "Discarded: " + player.discarded.Count;
                break;
            case PlayerClass.MAGE:
                mageCards.text = "Cards: " + player.drawPile.Count;
                mageDiscarded.text = "Discarded: " + player.discarded.Count;
                break;
            case PlayerClass.PRIEST:
                priestCards.text = "Cards: " + player.drawPile.Count;
                priestDiscarded.text = "Discarded: " + player.discarded.Count;
                break;
        }
    }
}
