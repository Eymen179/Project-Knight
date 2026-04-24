using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    // 1. Bu slotun hangi tip eþyayý kabul ettiðini Inspector'dan seçeceðiz
    public Item.ItemType allowedItemType;

    public void OnDrop(PointerEventData eventData)
    {
        // 1. Sürüklenen þeyin bir InventoryItem olduðundan emin ol
        if (eventData.pointerDrag.TryGetComponent<InventoryItem>(out InventoryItem draggedItem))
        {
            // 2. Eþya tipi bu slota uygun mu? (Genel slotlar "Other" olacak)
            if (draggedItem.item.itemType != allowedItemType)
            {
                return; // Uygun deðilse, býrakma iþlemini iptal et
            }

            // 3. Bu slot boþ mu? (içinde alt obje yok mu?)
            if (transform.childCount == 0)
            {
                // Boþsa, eþyayý bu slotun içine yerleþtir
                draggedItem.parentAfterDrag = transform;
                // Not: InventoryItem.cs'deki OnEndDrag() geri kalan iþi halledecek
            }
            else // 4. Slot doluysa (içinde baþka bir eþya var)
            {
                // Slotta zaten var olan eþyayý bul
                InventoryItem itemInSlot = transform.GetChild(0).GetComponent<InventoryItem>();

                // 5. Eþyalar ayný ve stacklenebilir mi?
                if (draggedItem.item == itemInSlot.item && itemInSlot.item.isStackable)
                {
                    // Stack'le: Slottaki eþyanýn sayýsýný artýr
                    itemInSlot.count += draggedItem.count;
                    itemInSlot.RefreshCount();
                    // Sürüklediðimiz eþyayý artýk silebiliriz
                    Destroy(draggedItem.gameObject);
                }
                else // 6. Stacklenemezse veya farklý eþyalarsa, yer deðiþtir (Swap)
                {
                    // Slottaki eþyayý, sürüklenenin eski slotuna gönder
                    itemInSlot.transform.SetParent(draggedItem.parentAfterDrag);
                    itemInSlot.transform.localPosition = Vector3.zero; // Yeni slotunun ortasýna koy

                    // Sürüklenen eþyayý bu slota al
                    draggedItem.parentAfterDrag = transform;
                    // Not: OnEndDrag() bu eþyayý buraya (transform) yerleþtirecek
                }
            }
        }
    }
}