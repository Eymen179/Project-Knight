using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    //Slotun tipi --> Sword - Other
    public Item.ItemType allowedItemType;

    public void OnDrop(PointerEventData eventData)
    {
        //Suruklenen objenin bir InventoryItem oldugundan emin ol.
        if (eventData.pointerDrag.TryGetComponent<InventoryItem>(out InventoryItem draggedItem))
        {
            //Slot tipi kontrolcusu
            if (draggedItem.item.itemType != allowedItemType)
            {
                return;
            }

            //Slot boslugu kontrolcusu
            if (transform.childCount == 0)
            {
                //Bossa suruklenen esyayi bu slota yerlestir.
                draggedItem.parentAfterDrag = transform;
            }
            else
            {
                //Aktif slottaki esya
                InventoryItem itemInSlot = transform.GetChild(0).GetComponent<InventoryItem>();

                //Stack kontrolcusu
                if (draggedItem.item == itemInSlot.item && itemInSlot.item.isStackable)
                {
                    itemInSlot.count += draggedItem.count;
                    itemInSlot.RefreshCount();

                    Destroy(draggedItem.gameObject);
                }
                else //Farkli tip esyalar
                {
                    //Slottaki eþyayý, suruklenenin eski slotuna gonder.
                    itemInSlot.transform.SetParent(draggedItem.parentAfterDrag);
                    itemInSlot.transform.localPosition = Vector3.zero; // Yeni slotunun ortasýna koy

                    //Suruklenen esyayi bu slota al
                    draggedItem.parentAfterDrag = transform;
                }
            }
        }
    }
}