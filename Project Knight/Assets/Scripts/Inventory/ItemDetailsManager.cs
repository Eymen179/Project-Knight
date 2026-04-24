using UnityEngine;
using System.Collections.Generic;

public class ItemDetailsManager : MonoBehaviour
{
    public static void ShowItemDetails(Item item)
    {
        if (UIManager.Instance == null || item == null) return;

        // 1. Eþya Adý ve Açýklamasýný Yazdýr
        if (UIManager.Instance.txtItemName != null)
            UIManager.Instance.txtItemName.text = item.itemName;

        if (UIManager.Instance.txtItemDescription != null)
            UIManager.Instance.txtItemDescription.text = item.description;

        // 2. Sadece deðeri 0'dan farklý olan özellikleri bu listeye topla
        List<string> activeStats = new List<string>();

        //Kiliç detaylari
        if (item.attackDamage != 0 && item.itemType == Item.ItemType.Sword)
            activeStats.Add($"Attack Damage: {item.attackDamage}");

        if (item.attackSpeed != 0 && item.itemType == Item.ItemType.Sword)
            activeStats.Add($"Attack Speed: {item.attackSpeed}");

        if (item.attackRange > 0 && item.itemType == Item.ItemType.Sword)
            activeStats.Add($"Range: {item.attackRange}");

        if (item.attackDamageMultiplierChance != 0)
            activeStats.Add($"Crit Chance: %{item.attackDamageMultiplierChance}");

        if (item.attackDamageMultiplier != 0 && item.itemType == Item.ItemType.Sword)
            activeStats.Add($"Crit Multiplier: {item.attackDamageMultiplier}x");

        //Kristal Detaylari
        if (item.attackDamage != 0 && item.itemType == Item.ItemType.Other)
            activeStats.Add($"Attack Damage: {(item.attackDamage > 0 ? "+" : "")}{item.attackDamage}");

        if (item.attackSpeed != 0 && item.itemType == Item.ItemType.Other)
            activeStats.Add($"Attack Speed: {(item.attackSpeed > 0 ? "+" : "")}{item.attackSpeed}");

        if (item.attackRange > 0 && item.itemType == Item.ItemType.Other)
            activeStats.Add($"Range: {(item.attackRange > 0 ? "+" : "")} {item.attackRange}");

        if (item.attackDamageMultiplier != 0 && item.itemType == Item.ItemType.Other)
            activeStats.Add($"Crit Multiplier: {(item.attackDamageMultiplier > 0 ? "+" : "")} {item.attackDamageMultiplier}");

        if (item.health != 0)
            activeStats.Add($"Health: {(item.health > 0 ? "+" : "")}{item.health}");

        if (item.healthRegenerationAmount != 0)
            activeStats.Add($"Health Regen: {(item.healthRegenerationAmount > 0 ? "+" : "")}{item.healthRegenerationAmount}");

        if (item.healthRegenerationSpeed != 0)
            activeStats.Add($"Regen Speed: {item.healthRegenerationSpeed}s");

        if (item.effectDuration > 0)
            activeStats.Add($"Duration: {item.effectDuration}s");

        // 3. Toplanan özellikleri UI Text'lerine aktar (TxtStat1, TxtStat2...)
        for (int i = 0; i < UIManager.Instance.txtStats.Length; i++)
        {
            // Eðer o anki Index listemizdeki eleman sayýsýndan küçükse (Yani yazdýracak stat varsa)
            if (i < activeStats.Count)
            {
                UIManager.Instance.txtStats[i].text = activeStats[i];
                UIManager.Instance.txtStats[i].gameObject.SetActive(true); // O text'i görünür yap
            }
            else
            {
                // Yazdýracak stat kalmadýysa o text yuvasýný gizle (Ekranda boþ yere "New Text" yazmasýn)
                UIManager.Instance.txtStats[i].gameObject.SetActive(false);
            }
        }
    }
}