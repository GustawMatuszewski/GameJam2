using UnityEngine;
using System.Collections;
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
    public PlayerController player;
    public CameraMover move;

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return;
        StartCoroutine(CraftWithParticles(hit.point));
    }

    IEnumerator CraftWithParticles(Vector3 point)
    {
        if (craftParticles != null)
        {
            ParticleSystem p = Instantiate(craftParticles, point, Quaternion.identity);
            p.Play();
            yield return new WaitForSeconds(p.main.duration);
            Destroy(p.gameObject);
        }
        TryCraft();
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
}