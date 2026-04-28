using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class ItemSelector : MonoBehaviour
{
    public List<ItemData> items;
    public int currentIndex = 0;
    public ParticleSystem selectParticles;
    public AudioClip[] selectSounds;
    [Range(0f, 5f)] public float volume = 1f;
    GameObject currentInstance;
    AudioSource audioSource;

    public MagicType CurrentMagicType => items.Count > 0 ? items[currentIndex].itemType : MagicType.Water;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        if (items.Count > 0)
            SpawnItem(items[0]);
    }

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return;
        SpawnParticles(hit.point);
        PlaySound();
        currentIndex = (currentIndex + 1) % items.Count;
        SpawnItem(items[currentIndex]);
    }

    void SpawnItem(ItemData data)
    {
        if (currentInstance != null)
            Destroy(currentInstance);
        currentInstance = Instantiate(data.itemPrefab, transform.position, transform.rotation);
        currentInstance.transform.SetParent(transform);
    }

    void SpawnParticles(Vector3 point)
    {
        if (selectParticles == null) return;
        ParticleSystem p = Instantiate(selectParticles, point, Quaternion.identity);
        p.Play();
        Destroy(p.gameObject, p.main.duration);
    }

    void PlaySound()
    {
        if (selectSounds == null || selectSounds.Length == 0) return;
        AudioClip clip = selectSounds[Random.Range(0, selectSounds.Length)];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.volume = 1f;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
    // Add this public property
    public GameObject CurrentInstance => currentInstance;

    // Add this public method
    public void RefreshDisplay()
    {
        SpawnItem(items[currentIndex]);
    }
}