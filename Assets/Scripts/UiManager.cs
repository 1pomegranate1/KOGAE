using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    [SerializeField] TextMeshProUGUI playerAngleText;

    [Header("입력 디버그")]
    [SerializeField] Image wKey, aKey, sKey, dKey;
    Color yellowGreen = new Color(0.52f, 0.82f, 0);
    private void Awake()
    {
        instance = this;
    }

    static public void PlayerAngleUI(float angle) => instance._PlayerAngleUI(angle);
    void _PlayerAngleUI(float angle)
    {
        playerAngleText.text = angle.ToString("f1");
    }

    static public void PlayerInputUI(int w, int a, int s, int d) => instance._PlayerInputUI(w, a, s, d);
    void _PlayerInputUI(int w, int a, int s, int d)
    {
        switch (w)
        {
            case -1:
                break;
            case 0:
                wKey.color = Color.white;
                break;
            case 1:
                wKey.color = Color.green;
                break;
            case 2:
                wKey.color = Color.yellow;
                break;
        }
        switch (a)
        {
            case -1:
                break;
            case 0:
                aKey.color = Color.white;
                break;
            case 1:
                aKey.color = Color.green;
                break;
            case 2:
                aKey.color = Color.yellow;
                break;
        }
        switch (s)
        {
            case -1:
                break;
            case 0:
                sKey.color = Color.white;
                break;
            case 1:
                sKey.color = Color.green;
                break;
            case 2:
                sKey.color = Color.yellow;
                break;
        }
        switch (d)
        {
            case -1:
                break;
            case 0:
                dKey.color = Color.white;
                break;
            case 1:
                dKey.color = Color.green;
                break;
            case 2:
                dKey.color = Color.yellow;
                break;
        }
    }
}
