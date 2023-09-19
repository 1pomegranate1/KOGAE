using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public class EnemyScript : MonoBehaviour
{
    bool isLookPlayer;
    [SerializeField] SpriteRenderer spriteRenderer;
    void Start()
    {
        
    }
    RaycastHit2D playerHit;
    void Update()
    {
        playerHit = Physics2D.Raycast(transform.position,  (PlayerReference.playerTransfrom.position - transform.position).normalized , 20, LayerMask.GetMask("Default") | LayerMask.GetMask("Floor"));
        if (playerHit && playerHit.collider.CompareTag("Player"))
        {
            Debug.Log("PlayerLook");
            isLookPlayer = true;
        }
        else
        {
            isLookPlayer = false;
        }
        
    }
    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    private void OnDrawGizmos()
    {
        if (playerHit)
        {
            Color rayColor; 
            if (isLookPlayer)
                rayColor = Color.green;
            else
                rayColor = Color.red;
            Gizmos.color = rayColor;
            Gizmos.DrawLine(transform.position, playerHit.point);
            rayColor.a = 0.25f;
            Gizmos.color = rayColor;
            Gizmos.DrawLine(transform.position, transform.position + (PlayerReference.playerTransfrom.position - transform.position).normalized * Mathf.Clamp(Vector3.Distance(transform.position, PlayerReference.playerTransfrom.position), 0, 20));
        }
        else
            Gizmos.DrawLine(transform.position, transform.position + ( PlayerReference.playerTransfrom.position - transform.position).normalized * Mathf.Clamp(Vector3.Distance(transform.position, PlayerReference.playerTransfrom.position), 0, 20));
    }
}
