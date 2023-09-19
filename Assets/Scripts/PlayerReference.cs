using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReference : MonoBehaviour
{
    [SerializeField] Player player;
    static public Transform playerTransfrom;
    private void Reset()
    {
        player = GetComponent<Player>();
    }
    private void Awake()
    {
        playerTransfrom = player.transform;
    }
}
