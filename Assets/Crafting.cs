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

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
    }

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
        PlaySound();
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