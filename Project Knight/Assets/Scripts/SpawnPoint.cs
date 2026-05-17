using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("TeleportPoint içindeki ID ile BÝREBÝR AYNI olmalýdýr!")]
    public string spawnPointID;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 2f); // Karakterin bakacaðý yön
    }
}