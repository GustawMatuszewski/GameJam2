using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public int playerHp;
    public ItemData itemHeldData;
    public List<ItemData> inventory = new List<ItemData>();

    public void AddToInventory(ItemData item)
    {
        inventory.Add(item);
        Debug.Log($"Picked up: {item.name} | Inventory count: {inventory.Count}");
    }

    public void TakeDamage(int damage)
    {
        playerHp -= damage;
        playerHp = Mathf.Max(playerHp, 0);
        if (playerHp <= 0)
            Debug.Log("Player is dead!");
    }
}