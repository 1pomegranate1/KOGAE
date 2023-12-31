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

    [Header("입력 디버그")]
    [SerializeField] Transform velocityUI;
    [SerializeField] GameObject velocityPlus;
    [SerializeField] float velocityUIMuliy;
    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

    static public void PlayerAngleUI(float angle) => instance._PlayerAngleUI(angle);
    void _PlayerAngleUI(float angle)
    {
        playerAngleText.text = angle.ToString("f1") + "\n<size=-10> Angle</size>";
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

    static public void PlayerVelocityUI(float velcity) => instance.playerVelocityUI(velcity);
    void playerVelocityUI(float velcity)
    {
        velcity *= velocityUIMuliy;
        if (velcity < -0.7f)
        {
            velocityUI.localScale = new Vector2(1, -0.7f);
            velocityPlus.SetActive(true);
        }
        else if (velcity > 0.3f)
        {
            velocityUI.localScale = new Vector2(1, 0.3f);
            velocityPlus.SetActive(false);
        }
        else
        {
            velocityUI.localScale = new Vector2(1, velcity);
            velocityPlus.SetActive(false);
        }
    }
}
