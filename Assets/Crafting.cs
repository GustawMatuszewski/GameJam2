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
    public AudioClip[] craftSounds;
    [Range(0f, 5f)] public float volume = 1f;
    public PlayerController player;
    public CameraMover move;
    AudioSource audioSource;
    [Header("Crafting Effect")]
    public CraftingEffect craftingEffect;
    [Header("Post Craft Delay")]
    public int postCraftDelay = 2;
    private bool isCrafting = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (isCrafting) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return;
        TryCraft(hit.point);
    }

    void TryCraft(Vector3 hitPoint)
    {
        List<ItemData> current = GetCurrentItems();
        foreach (CraftingRecipe recipe in recipes)
        {
            if (!Matches(current, recipe.ingredients)) continue;
            List<GameObject> itemGOs = new();
            foreach (var slot in slots)
                if (slot.CurrentInstance != null)
                    itemGOs.Add(slot.CurrentInstance);
            isCrafting = true;
            StartCoroutine(DoCraft(itemGOs, recipe, hitPoint));
            return;
        }
        Debug.Log("No matching recipe found");
    }

    IEnumerator DoCraft(List<GameObject> itemGOs, CraftingRecipe recipe, Vector3 center)
    {
        PlaySound();
        foreach (var go in itemGOs)
            go.transform.SetParent(null);
        yield return craftingEffect.PlayCraftSequence(
            itemGOs,
            outputSpawnPoint.position,
            recipe.output.itemPrefab
        );
        player.itemHeldData = recipe.output;
        Debug.Log("Crafted: " + recipe.output.name + " Type: " + recipe.output.itemType);
        yield return new WaitForSeconds(postCraftDelay);
        GameObject spawnedResult = craftingEffect.GetSpawnedResult();
        if (spawnedResult != null)
            Destroy(spawnedResult);
        move.moveNow = true;
        ResetTable();
        isCrafting = false;
    }

    void ResetTable()
    {
        foreach (ItemSelector slot in slots)
        {
            slot.currentIndex = 0;
            slot.RefreshDisplay();
        }
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

    void PlaySound()
    {
        if (craftSounds == null || craftSounds.Length == 0) return;
        AudioClip clip = craftSounds[Random.Range(0, craftSounds.Length)];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.volume = 1f;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}