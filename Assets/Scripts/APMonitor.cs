using System.Collections.Generic;
using UnityEngine;

public class APMonitor : MonoBehaviour
{
    [SerializeField] private List<GameObject> icons;

    public void UpdateAP(int ap)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].SetActive(i < ap);
        }
    }
}
