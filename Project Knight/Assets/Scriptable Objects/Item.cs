using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    [Header("Details")]
    public string itemName;
    public string description;

    [Header("Visuals")]
    public Sprite inventorySprite;
    public GameObject itemObject;

    [Header("Attributes")]
    public int attackDamage;
    public int attackSpeed;

    public float attackDamageMultiplier;
    public float attackSpeedMultiplier;

    public int armor;
    public int health;
    public int healthRegenerationAmount;
    public int healthRegenerationSpeed;

    [Header("Inventory")]
    public bool isStackable = true;

    public ItemType itemType;
    public void ShowDetails()
    {

    }
    public enum ItemType
    {
        Sword,
        Other
    }
}
