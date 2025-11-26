using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullanmak çok daha iyi, Unity'ye eklemeyi unutma

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    // GÜNCELLENDÝ: Her eþyanýn kendi yazý objesi olmalý
    public TextMeshProUGUI countText;

    [HideInInspector] public Item item;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public int count = 1;

    public void InitializeItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.inventorySprite;
        RefreshCount();
    }

    // GÜNCELLENDÝ: Artýk kendi 'countText' objesini güncelliyor
    public void RefreshCount()
    {
        // Sayý 1'den büyükse göster, deðilse gizle
        countText.text = count > 1 ? count.ToString() : "";
        countText.gameObject.SetActive(count > 1);
    }

    // --- Drag System (Burasý çoðunlukla doðruydu, küçük eklemeler yapýldý) ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        countText.raycastTarget = false; // Yazýnýn da týklamayý engellemesini önle
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root); // Canvas'ýn en üstüne al
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    /*public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        countText.raycastTarget = true;
        transform.SetParent(parentAfterDrag); // Býrakýldýðý yeni slota (veya eskisine) geri dön
        transform.localPosition = Vector3.zero; // Slotun tam ortasýna yerleþ
    }*/
    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        countText.raycastTarget = true;

        // YENÝ: Mouse herhangi bir UI objesinin üzerinde DEÐÝLSE (yani dünyaya býrakýldýysa)
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // InventoryManager üzerinden yere atma iþlemini baþlat
            InventoryManager.Instance.DropItem(this);

            // UI objesi yok olacaðý veya sayýsý azalacaðý için parent iþlemine gerek kalmayabilir
            // Ama sayý azalýrsa eski yerine dönsün diye yine de parent atamasý yapýyoruz:
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            // Eðer UI üzerine (baþka slota veya panele) býrakýldýysa normal iþlem
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;
        }
        /*kutsal*/
        EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();
        if (equipmentManager != null)
        {
            equipmentManager.ValidateEquipment();
        }
    }
}