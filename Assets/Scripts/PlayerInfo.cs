using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private GameObject defenseIcon;
    [SerializeField] private List<GameObject> icons;
    [SerializeField] private Image hpFiller;
    [SerializeField] private TextMeshProUGUI hpText;

    private void Start() {
        hpFiller.fillAmount = 1f;
        defenseIcon.SetActive(false);
    }
    
    public void UpdatePlayerInfo(float currentHp, float maxHp, int ap, int defense)
    {
        UpdateHp(currentHp, maxHp);
        UpdateAP(ap);
        UpdateDefense(defense);
    }
    
    private void UpdateHp(float currentHp, float maxHp) {
        hpText.text = currentHp + " / " + maxHp;
        hpFiller.fillAmount = Mathf.Clamp(currentHp / maxHp, 0f, 1f);
    }
    
    private void UpdateAP(int ap)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].SetActive(i < ap);
        }
    }

    private void UpdateDefense(int defense)
    {
        defenseIcon.SetActive(defense > 0);
        if (defense > 0)
        {
            defenseIcon.GetComponentInChildren<TextMeshProUGUI>().text = defense.ToString();
        }
    }
    
    public void BarColor(Color32 color)
    {
        hpFiller.color = color;
    }
}
