using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFoot : MonoBehaviour
{
    [Header("Component")]
    [SerializeField]Player player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Floor"))
        {
            //player.Landing();
        }
    }
    private void Reset()
    {
        player = transform.parent.GetComponent<Player>();
    }
}
