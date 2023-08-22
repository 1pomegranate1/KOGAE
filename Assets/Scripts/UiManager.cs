using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    [SerializeField] TextMeshProUGUI playerAngleText;

    private void Awake()
    {
        instance = this;
    }

    static public void PlayerAngleUI(float angle) => instance._PlayerAngleUI(angle);
    void _PlayerAngleUI(float angle)
    {
        playerAngleText.text = angle.ToString("f1");
    }
}
