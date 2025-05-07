using System;
using Model;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI warriorAp;
    [SerializeField] private TextMeshProUGUI warriorCards;
    [SerializeField] private TextMeshProUGUI warriorDiscarded;

    [SerializeField] private TextMeshProUGUI priestAp;
    [SerializeField] private TextMeshProUGUI priestCards;
    [SerializeField] private TextMeshProUGUI priestDiscarded;
    
    [SerializeField] private TextMeshProUGUI mageAp;
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
                warriorAp.text = "AP: " + player.GetCurrentAP();
                warriorCards.text = "Cards: " + player.drawPile.Count;
                warriorDiscarded.text = "Discarded: " + player.discarded.Count;
                break;
            case PlayerClass.MAGE:
                mageAp.text = "AP: " + player.GetCurrentAP();
                mageCards.text = "Cards: " + player.drawPile.Count;
                mageDiscarded.text = "Discarded: " + player.discarded.Count;
                break;
            case PlayerClass.PRIEST:
                priestAp.text = "AP: " + player.GetCurrentAP();
                priestCards.text = "Cards: " + player.drawPile.Count;
                priestDiscarded.text = "Discarded: " + player.discarded.Count;
                break;
        }
    }
}
