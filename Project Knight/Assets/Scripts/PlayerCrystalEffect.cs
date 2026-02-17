using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCrystalEffect : MonoBehaviour
{
    [SerializeField] private InputActionReference[] inventorySlotActions;

    [SerializeField] private InventorySlot[] inventorySlots;

    private PlayerAttackSystem playerAttackSystem;
    private EquipmentManager equipmentManager;

    void Start()
    {
        if(inventorySlotActions.Length != inventorySlots.Length)
        {
            Debug.LogError("inventorySlotActions ve inventorySlots dizileri ayný uzunlukta olmalýdýr!");
            return;
        }
        playerAttackSystem = GetComponent<PlayerAttackSystem>();
        equipmentManager = GetComponent<EquipmentManager>();
    }
    // Update is called once per frame
    void Update()
    {

    }
    private void OnEnable()
    {
        foreach (var inputs in inventorySlotActions)
        {
            if (inputs != null)
            {
                inputs.action.Enable();
                // Olayý baðla
                inputs.action.performed += UseCrystalPerformed;
            }
        }
    }

    private void OnDisable()
    {
        foreach (var inputs in inventorySlotActions)
        {
            if (inputs != null)
            {
                // Olay baðýný kopar (Çok önemli! Yoksa hafýza sýzýntýsý olur)
                inputs.action.performed -= UseCrystalPerformed;
                inputs.action.Disable();
            }
        }
    }

    // Tüm tuþlar bu fonksiyona düþer
    private void UseCrystalPerformed(InputAction.CallbackContext context)
    {
        // Örnek: Hangi input olduðunu bulup ona göre iþlem yapma
        for (int i = 0; i < inventorySlotActions.Length; i++)
        {
            if (inventorySlotActions[i].action == context.action)
            {
                if (inventorySlots[i].transform.childCount > 0)
                {
                    ApplyCrystalEffect(inventorySlots[i]);
                }
                else
                {
                    //Ses Efekti daha sonra eklenecek.
                }
            }
        }
    }

    private void ApplyCrystalEffect(InventorySlot inventorySlot)
    {
        InventoryItem crystalInSlot = inventorySlot.transform.GetChild(0).GetComponent<InventoryItem>();
        Item crystalToUse = crystalInSlot.item;

        if(crystalToUse.effectDuration != 0)
        {
            StartCoroutine(EffectDuration());
        }
        else
        {
            playerAttackSystem.damageToDeal += crystalToUse.attackDamage;
            playerAttackSystem
            equipmentManager.speedMultiplier += (crystalToUse.attackSpeed / 10);
            //Player can sistemleri ileride eklenecek.
        }
    }

    IEnumerator EffectDuration()
    {

    }
}

