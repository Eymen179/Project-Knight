using UnityEngine;

public class LocalTeleportPoint : MonoBehaviour
{
    [Tooltip("Ayný sahne içindeki hedef SpawnPoint objesi")]
    public Transform localTargetPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // CharacterController varsa anlýk kapatmak zorundayýz
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Sahne yüklemeden direkt pozisyonu deðiþtir
            other.transform.position = localTargetPoint.position;
            other.transform.rotation = localTargetPoint.rotation;

            if (cc != null) cc.enabled = true;
        }
    }
}