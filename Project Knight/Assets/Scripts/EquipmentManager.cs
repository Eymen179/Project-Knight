using UnityEngine;
using UnityEngine.InputSystem;

public class EquipmentManager : MonoBehaviour
{
    public InventorySlot toolbarSwordSlot;
    public Transform handTransform;

    [SerializeField] private InputActionReference equipSlot1Action;

    private GameObject currentEquippedWeapon;

    // --- YENÝ EKLENDÝ ---
    private PlayerMovement playerMovement; // PlayerMovement script'ine referans
    private PlayerCombatManager combatManager;

    public Item currentItemInHand;

    private bool isWeaponEquipped = false; // Senin 'pressCounter' mantýðý için toggle

    void Start() // YENÝ EKLENDÝ
    {
        // EquipmentManager ve PlayerMovement ayný obje üzerindeyse
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script'i bulunamadý!");
        }
        // YENÝ: PlayerCombatManager referansýný al
        combatManager = GetComponent<PlayerCombatManager>();
        if (combatManager == null) Debug.LogError("PlayerCombatManager script'i bulunamadý!");
    }

    private void OnEnable()
    {
        equipSlot1Action.action.Enable();
        equipSlot1Action.action.performed += OnEquipSlot1Performed;
    }

    private void OnDisable()
    {
        equipSlot1Action.action.Disable();
        equipSlot1Action.action.performed -= OnEquipSlot1Performed;
    }

    // --- GÜNCELLENDÝ: Artýk 'pressCounter' gibi aç/kapa mantýðý içeriyor ---
    private void OnEquipSlot1Performed(InputAction.CallbackContext context)
    {
        // 1. Eðer zaten bir silah kuþanmýþsak, silahý býrak
        if (isWeaponEquipped)
        {
            UnequipWeapon();
        }
        // 2. Silah kuþanmamýþsak, slotu kontrol et ve kuþan
        else
        {
            // Slotta bir eþya var mý?
            if (toolbarSwordSlot.transform.childCount > 0)
            {
                EquipWeaponFromSlot();
            }
            // Slot boþsa hiçbir þey yapma
        }
    }

    // --- YENÝ FONKSÝYON: Sadece kuþanma iþini yapar ---
    private void EquipWeaponFromSlot()
    {
        // Slottaki item'i al
        InventoryItem itemInSlot = toolbarSwordSlot.transform.GetChild(0).GetComponent<InventoryItem>();
        Item itemToEquip = itemInSlot.item;

        if (itemToEquip.itemObject != null)
        {
            // 3D modeli oluþtur
            currentEquippedWeapon = Instantiate(itemToEquip.itemObject, handTransform);
            currentEquippedWeapon.transform.localPosition = Vector3.zero;
            currentEquippedWeapon.transform.localRotation = Quaternion.identity;

            currentItemInHand = itemToEquip;

            // Durumu ve animasyonu güncelle
            isWeaponEquipped = true;
            playerMovement.SetEquippedState(true); // PlayerMovement'a haber ver!

            if (combatManager != null)
            {
                combatManager.isWeaponEquipped = true;
            }

            // --- BURASI EKLENECEK ---
            // Eline aldýðýn silahý "Player" layer'ýna (veya Ignore Raycast'e) çekmelisin.
            // Böylece PlayerInteraction scripti (Raycast) bu silahý GÖRMEZDEN GELÝR.
            int playerLayer = LayerMask.NameToLayer("Player"); // Veya "Ignore Raycast"
            InventoryManager.SetLayerRecursively(currentEquippedWeapon, playerLayer);
            // ------------------------

            //Saldýrý hýzý ayarý
            float speedMultiplier = itemToEquip.attackSpeed / 10f;
            // Animator'e deðeri gönder
            // (PlayerMovement veya CombatManager üzerinden Animator'e ulaþtýðýný varsayýyorum)
            GetComponent<Animator>().SetFloat("fAttackSpeed", speedMultiplier);

            Debug.Log($"Saldýrý Hýzý Ayarlandý: {speedMultiplier}x");

            Debug.Log(itemToEquip.itemName + " kuþanýldý!");
        }
    }

    // --- YENÝ FONKSÝYON: Sadece silahý býrakma iþini yapar ---
    private void UnequipWeapon()
    {
        if (currentEquippedWeapon != null)
        {
            Destroy(currentEquippedWeapon);
            currentEquippedWeapon = null;
        }

        currentItemInHand = null;

        // Durumu ve animasyonu güncelle
        isWeaponEquipped = false;
        playerMovement.SetEquippedState(false); // PlayerMovement'a haber ver!
        Debug.Log("Silah býrakýldý.");
        /* deneme*/
        if (combatManager != null)
        {
            combatManager.isWeaponEquipped = false;
        }
    }

    // --- YENÝ KRÝTÝK FONKSÝYON: DOÐRULAMA SÝSTEMÝ ---
    // Bu fonksiyonu eþyalarýn yeri deðiþtiðinde çaðýracaðýz.
    public void ValidateEquipment()
    {
        // Eðer elimizde silah yoksa kontrole gerek yok
        if (!isWeaponEquipped) return;

        // 1. Kýlýç Slotu tamamen boþaldýysa -> Silahý Býrak
        if (toolbarSwordSlot.transform.childCount == 0)
        {
            UnequipWeapon();
            return;
        }

        // 2. Kýlýç Slotunda eþya var AMA elimizdekiyle ayný deðilse (Swap yapýldýysa)
        InventoryItem itemInSlot = toolbarSwordSlot.transform.GetChild(0).GetComponent<InventoryItem>();
        if (itemInSlot.item != currentItemInHand)
        {
            // Önce eskisini býrak, sonra yenisini (varsa) kuþan
            UnequipWeapon();
            EquipWeaponFromSlot();
        }
    }
}