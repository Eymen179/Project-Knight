using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullanmak çok daha iyi, Unity'ye eklemeyi unutma

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;

    public TextMeshProUGUI countText;

    [HideInInspector] public Item item;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public int count = 1;

    //Envanter itemini aktif etme metodu
    public void InitializeItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.inventorySprite;

        RefreshCount();
    }

    //Ýleriye yonelik: Stackable itemler icin sayiyi guncelleme metodu
    public void RefreshCount()
    {
        // Sayý 1'den büyükse göster, deðilse gizle
        countText.text = count > 1 ? count.ToString() : "";
        countText.gameObject.SetActive(count > 1);
    }

    //Envanter esyasini surukleme islemleri icin gerekli metodlar
    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        countText.raycastTarget = false;

        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root); //Canvas'in en ustune alir.

        ItemDetailsManager.ShowItemDetails(item);
        UIManager.Instance.pnlItemDetails.SetActive(true); //Esya detay panelini kapat.
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        countText.raycastTarget = true;

        //Mouse herhangi bir UI objesinin uzerinde degilse (yani dunyaya birakildiysa)
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            //InventoryManager uzerinden yere atma islemini baslat
            InventoryManager.Instance.DropItem(this);

            //UI objesi yok olacagi veya sayisi azalacagi icin parent islemine gerek kalmayabilir.
            //Ama sayý azalirsa eski yerine dönsün diye yine de parent atamasi yapilir.
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            //Eger UI üzerine (baska slota veya panele) birakildiysa normal islem
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;
        }
        //-----Kullanilacak kilic slotuna esya birakildiysa esyayi tanimla-----
        EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();
        if (equipmentManager != null)
        {
            equipmentManager.ValidateEquipment();
        }

        UIManager.Instance.pnlItemDetails.SetActive(false);
    }
}