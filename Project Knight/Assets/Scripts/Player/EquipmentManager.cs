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

    //Saldýrý Hýzý
    [HideInInspector] public float bonusAttackSpeed = 0f;

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

            // --- YENÝ EKLENEN KRÝTÝK DÜZELTME ---
            // 1. Kýlýç elimizdeyken süzülme animasyonuna ihtiyacýmýz yok, scripti sil:
            if (currentEquippedWeapon.TryGetComponent<Benjathemaker.SimpleGemsAnim>(out Benjathemaker.SimpleGemsAnim floatAnim))
            {
                Destroy(floatAnim);
            }
            // --- YENÝ EKLENEN KRÝTÝK KISIM: KILICIN FÝZÝÐÝNÝ YOK ET ---
            // Kýlýcýn kendisinde veya alt objelerinde (býçak, kabza vs.) bulunan tüm Collider'larý bul ve sil.
            Collider[] weaponColliders = currentEquippedWeapon.GetComponentsInChildren<Collider>();
            foreach (Collider col in weaponColliders)
            {
                col.enabled = false;
            }
            // ---------------------------------------------------------
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
            UpdateAttackSpeed();

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
    // YENÝ: Hýz güncelleme fonksiyonu (Kristal kullanýnca da bunu çaðýracaðýz)
    public void UpdateAttackSpeed()
    {
        if (currentItemInHand == null) return;

        // Formül: (Silah Hýzý / 10) + Bonus Hýz
        float baseSpeed = currentItemInHand.attackSpeed;
        float totalSpeed = baseSpeed + bonusAttackSpeed;

        GetComponent<Animator>().SetFloat("fAttackSpeed", totalSpeed);
        Debug.Log($"Yeni Saldýrý Hýzý: {totalSpeed} (Silah: {baseSpeed} + Bonus: {bonusAttackSpeed})");
    }
    public int GetCurrentWeaponDamage()
    {
        // Eðer elimizde bir eþya varsa ve bu eþyanýn bir hasar deðeri varsa döndür
        if (currentItemInHand != null)
        {
            return currentItemInHand.attackDamage;
        }

        // Eðer elimiz boþsa veya hasarsýz bir eþya varsa (Yumruk hasarý)
        return 1; // Ýstersen burayý 5 yapabilirsin.
    }
}