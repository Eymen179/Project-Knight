using UnityEngine;

[RequireComponent(typeof(Collider))] // Üzerinde collider olmalý
public class ItemPickup : MonoBehaviour
{
    public Item item; //Ýlgili esyanýn scriptable object referansi

    //PlayerInteraction tarafindan cagrilacak metot
    public void Pickup()
    {
        //InventoryManager'a esyayi eklemesini soyle.
        bool success = InventoryManager.Instance.AddItem(item);
        if (success)
        {
            Destroy(gameObject); // Toplandiysa dunyadan sil.
        }
    }
}