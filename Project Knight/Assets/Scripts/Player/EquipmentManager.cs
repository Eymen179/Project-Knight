using UnityEngine;
using UnityEngine.InputSystem;

public class EquipmentManager : MonoBehaviour
{
    public InventorySlot toolbarSwordSlot;
    public Transform handTransform;

    [SerializeField] private InputActionReference equipSlot1Action;

    private GameObject currentEquippedWeapon;

    private PlayerMovement playerMovement;
    private PlayerCombatManager combatManager;

    public Item currentItemInHand;

    private bool isWeaponEquipped = false;

    //Saldiri hizi kristal efektleri
    [HideInInspector] public float bonusAttackSpeed = 0f;
    [HideInInspector] public float permanentBonusAttackSpeed = 0f; // Kalýcý

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script'i bulunamadý!");
        }

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

    //"1" tusu metodu
    private void OnEquipSlot1Performed(InputAction.CallbackContext context)
    {
        if (isWeaponEquipped)
        {
            UnequipWeapon();
        }
        else
        {
            // Slot dolulugu kontrolcusu
            if (toolbarSwordSlot.transform.childCount > 0)
            {
                EquipWeaponFromSlot();
            }
        }
    }

    //Kilic Kusanma Metodu
    private void EquipWeaponFromSlot()
    {
        InventoryItem itemInSlot = toolbarSwordSlot.transform.GetChild(0).GetComponent<InventoryItem>();
        Item itemToEquip = itemInSlot.item;

        if (itemToEquip.itemObject != null)
        {
            //Kilici ele spawnla
            currentEquippedWeapon = Instantiate(itemToEquip.itemObject, handTransform);
            currentEquippedWeapon.transform.localPosition = Vector3.zero;
            currentEquippedWeapon.transform.localRotation = Quaternion.identity;

            //Kilic objesi eldeyken yerdeykenki suzulme animasyonu olmamali.
            if (currentEquippedWeapon.TryGetComponent<Benjathemaker.SimpleGemsAnim>(out Benjathemaker.SimpleGemsAnim floatAnim))
            {
                Destroy(floatAnim);
            }

            //Collider kaldir.
            Collider[] weaponColliders = currentEquippedWeapon.GetComponentsInChildren<Collider>();
            foreach (Collider col in weaponColliders)
            {
                col.enabled = false;
            }

            currentItemInHand = itemToEquip;

            //Durum guncellemesi
            isWeaponEquipped = true;
            playerMovement.SetEquippedState(true);

            if (combatManager != null)
            {
                combatManager.isWeaponEquipped = true;
            }

            //Layer ayari
            int playerLayer = LayerMask.NameToLayer("Player");
            InventoryManager.SetLayerRecursively(currentEquippedWeapon, playerLayer);

            //Saldiri hizi guncellemesi (Kristal efektleri icin)
            UpdateAttackSpeed();

            Debug.Log(itemToEquip.itemName + " kuþanýldý!");
        }
    }

    //Kilici elden birakma metodu
    private void UnequipWeapon()
    {
        if (currentEquippedWeapon != null)
        {
            Destroy(currentEquippedWeapon);
            currentEquippedWeapon = null;
        }

        currentItemInHand = null;

        //Durum guncellemesi
        isWeaponEquipped = false;
        playerMovement.SetEquippedState(false);
        Debug.Log("Silah býrakýldý.");
        /* deneme*/
        if (combatManager != null)
        {
            combatManager.isWeaponEquipped = false;
        }
    }

    //Dogrulama sistemi metodu
    public void ValidateEquipment()
    {
        if (!isWeaponEquipped) return;

        //Kilic slotu bosaldiysa kilici birak.
        if (toolbarSwordSlot.transform.childCount == 0)
        {
            UnequipWeapon();
            return;
        }

        //Envanterdeki baska kilicla swap islemi
        InventoryItem itemInSlot = toolbarSwordSlot.transform.GetChild(0).GetComponent<InventoryItem>();
        if (itemInSlot.item != currentItemInHand)
        {
            UnequipWeapon();
            EquipWeaponFromSlot();
        }
    }
    //Hiz guncelleme metodu (Kristal efektleri icin)
    public void UpdateAttackSpeed()
    {
        if (currentItemInHand == null) return;

        //Formul: (Silah Hizi / 10) + Bonus Hiz
        float baseSpeed = currentItemInHand.attackSpeed;
        float totalSpeed = baseSpeed + bonusAttackSpeed + permanentBonusAttackSpeed;

        GetComponent<Animator>().SetFloat("fAttackSpeed", totalSpeed);
        Debug.Log($"Yeni Saldýrý Hýzý: {totalSpeed} (Silah: {baseSpeed} + Bonus: {bonusAttackSpeed} + Kalýcý: {permanentBonusAttackSpeed})");
    }

    //Eldeki kilicin guncel hasar degerini donduren metot
    public int GetCurrentWeaponDamage()
    {
        if (currentItemInHand != null)
        {
            return currentItemInHand.attackDamage;
        }

        return 1;
    }
}