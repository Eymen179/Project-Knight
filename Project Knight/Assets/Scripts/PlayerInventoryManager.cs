using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInventoryManager : MonoBehaviour
{
    [SerializeField] private InputActionReference openInventory;
    private int pressCounter = 0;

    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;

    void Update()
    {
        //TAB tusu
        if (openInventory.action.WasPressedThisFrame())
        {
            pressCounter++;
            if (pressCounter % 2 != 0)
            {
                UIManager.Instance.mainInventoryGroup.SetActive(true);
                GetComponent<PlayerMovement>().enabled = false;
                GetComponent<PlayerCombatManager>().enabled = false;

                UIManager.Instance.toolBarBarrier.enabled = false;
            }
            else
            {
                UIManager.Instance.mainInventoryGroup.SetActive(false);
                GetComponent<PlayerMovement>().enabled = true;
                GetComponent<PlayerCombatManager>().enabled = true;

                UIManager.Instance.toolBarBarrier.enabled = true;
            }
        }
    }
    public bool AddItem(Item item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot tempSlot = inventorySlots[i];
            InventoryItem itemInSlot = tempSlot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot == null)
            {
                SpawnNewItem(item, tempSlot);
                return true;
            }
        }
        return false;
    }
    void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItemObject = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemObject.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }
}
