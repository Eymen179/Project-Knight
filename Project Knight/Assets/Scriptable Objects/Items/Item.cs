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
    public float attackSpeed;
    public float attackRange = 1.0f;

    public float attackDamageMultiplier;
    [Range(0, 100)]
    public int attackDamageMultiplierChance;

    public int health;
    public int healthRegenerationAmount;
    public int healthRegenerationSpeed;

    public float effectDuration;

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
