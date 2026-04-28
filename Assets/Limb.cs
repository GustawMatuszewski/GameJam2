using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class LimbDrop
{
    public ItemData item;
    [Range(0f, 100f)] public float dropChance;
}

public class Limb : MonoBehaviour
{
    public int limbHp;
    public MagicType magicType;
    public PlayerController player;
    public ParticleSystem hitParticles;
    public AudioClip[] hitSounds;
    public List<LimbDrop> drops;
    public float dropDistance = 1f;
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
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                Attack(hit.point);
                return;
            }
        }
    }

    void Attack(Vector3 hitPoint)
    {
        if (player.itemHeldData == null)
        {
            Debug.Log("No item held");
            return;
        }

        int damage = 1;
        if (IsStrongAgainst(player.itemHeldData.itemType, magicType))
            damage = 2;
        else if (IsWeakAgainst(player.itemHeldData.itemType, magicType))
            damage = 0;

        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(hitParticles, hitPoint, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration);
        }

        PlayHitSound(damage);
        TryDrop(damage);

        limbHp -= damage;
        Debug.Log("Dealt " + damage + " damage. Limb HP: " + limbHp);
        player.itemHeldData = null;
    }

    void PlayHitSound(int damage)
    {
        if (hitSounds == null || hitSounds.Length == 0) return;
        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.volume = damage == 2 ? 1f : damage == 1 ? 0.6f : 0.3f;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    void TryDrop(int damage)
    {
        if (drops == null || drops.Count == 0) return;

        float chanceMultiplier = damage == 2 ? 1.5f : damage == 1 ? 1f : 0.5f;
        Vector3 dropPos = transform.position + transform.forward * dropDistance;

        foreach (LimbDrop drop in drops)
        {
            if (drop.item == null || drop.item.itemPrefab == null) continue;
            float roll = Random.Range(0f, 100f);
            if (roll <= drop.dropChance * chanceMultiplier)
                Instantiate(drop.item.itemPrefab, dropPos, Quaternion.identity);
        }
    }

    bool IsStrongAgainst(MagicType attacker, MagicType defender)
    {
        return attacker == MagicType.Water && defender == MagicType.Fire ||
               attacker == MagicType.Fire  && defender == MagicType.Air  ||
               attacker == MagicType.Air   && defender == MagicType.Rock ||
               attacker == MagicType.Rock  && defender == MagicType.Water;
    }

    bool IsWeakAgainst(MagicType attacker, MagicType defender)
    {
        return attacker == MagicType.Fire  && defender == MagicType.Water ||
               attacker == MagicType.Air   && defender == MagicType.Fire  ||
               attacker == MagicType.Rock  && defender == MagicType.Air   ||
               attacker == MagicType.Water && defender == MagicType.Rock;
    }
}