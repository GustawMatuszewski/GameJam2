using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Crafting : MonoBehaviour
{
    [Header("Slots")]
    public List<ItemSelector> slots;
    [Header("Recipes")]
    public List<CraftingRecipe> recipes;
    [Header("Output")]
    public Transform outputSpawnPoint;
    public ParticleSystem craftParticles;
    public bool craft = false;
    public PlayerController player;
    public CameraMover move;

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                craft = true;
                SpawnParticles(hit.point);
                break;
            }
        }
        if (craft)
        {
            TryCraft();
            craft = false;
        }
    }

    void TryCraft()
    {
        List<ItemData> current = GetCurrentItems();
        foreach (CraftingRecipe recipe in recipes)
        {
            if (Matches(current, recipe.ingredients))
            {
                Instantiate(recipe.output.itemPrefab, outputSpawnPoint.position, outputSpawnPoint.rotation);
                player.itemHeldData = recipe.output;
                Debug.Log("Crafted: " + recipe.output.name + " Type: " + recipe.output.itemType);
                move.moveNow = true;
                return;
            }
        }
        Debug.Log("No matching recipe found");
    }

    List<ItemData> GetCurrentItems()
    {
        List<ItemData> result = new();
        foreach (ItemSelector slot in slots)
            result.Add(slot.items[slot.currentIndex]);
        return result;
    }

    bool Matches(List<ItemData> current, List<ItemData> required)
    {
        if (current.Count != required.Count) return false;
        for (int i = 0; i < current.Count; i++)
            if (current[i] != required[i]) return false;
        return true;
    }

    void SpawnParticles(Vector3 point)
    {
        if (craftParticles == null) return;
        ParticleSystem p = Instantiate(craftParticles, point, Quaternion.identity);
        p.Play();
        Destroy(p.gameObject, p.main.duration);
    }
}