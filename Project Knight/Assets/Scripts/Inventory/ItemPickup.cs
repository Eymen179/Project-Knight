using UnityEngine;

[RequireComponent(typeof(Collider))] // Üzerinde collider olmalý
public class ItemPickup : MonoBehaviour
{
    public Item item; // Bu objenin temsil ettiði ScriptableObject (Inspector'dan ata)

    // PlayerInteraction tarafýndan çaðrýlacak
    public void Pickup()
    {
        // InventoryManager'a eþyayý eklemesini söyle
        bool success = InventoryManager.Instance.AddItem(item);
        if (success)
        {
            Destroy(gameObject); // Toplandýysa dünyadan sil
        }
        // Eðer envanter doluysa (success = false) eþya dünyada kalýr.
    }
}