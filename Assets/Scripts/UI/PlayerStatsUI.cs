using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private List<GameObject> throwIcons = new List<GameObject>();

    public void SetVisibleThrowCount(int throwCount)
    {
        for (int i = 0; i < throwIcons.Count; i++)
        {
            if (throwIcons[i] != null)
            {
                throwIcons[i].SetActive(i < throwCount);
            }
        }
    }
}
