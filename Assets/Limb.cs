using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class LimbDrop
{
    public ItemData item;
    [Range(0f, 100f)] public float dropChance;
}

public class Limb : MonoBehaviour
{
    public Transform dropPoint;
    public int limbHp;
    public int maxLimbHp = 10;
    public MagicType magicType;
    public PlayerController player;
    public ParticleSystem hitParticles;
    public AudioClip[] hitSounds;
    public List<LimbDrop> drops;
    public float dropDistance = 1.5f;
    public float textHeight = 2f;
    public CameraMover cameraMover;
    [HideInInspector] public bool itemWasDropped = false;
    AudioSource audioSource;

    void Start()
    {
        maxLimbHp = limbHp;
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
            SpawnFloatingText("No item held!", Color.gray, hitPoint);
            return;
        }

        MagicType attackType = player.itemHeldData.itemType;
        int damage = 1;
        string effectiveness = "";
        Color textColor = Color.white;

        if (IsStrongAgainst(attackType, magicType))
        {
            damage = 2;
            effectiveness = $"SUPER EFFECTIVE!\n{attackType} beats {magicType}\n-{damage} HP";
            textColor = Color.green;
        }
        else if (IsWeakAgainst(attackType, magicType))
        {
            damage = 0;
            effectiveness = $"NOT EFFECTIVE\n{attackType} loses to {magicType}\n-{damage} HP";
            textColor = Color.red;
        }
        else
        {
            effectiveness = $"NEUTRAL\n{attackType} vs {magicType}\n-{damage} HP";
            textColor = Color.yellow;
        }

        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(hitParticles, hitPoint, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration);
        }

        PlayHitSound(damage);
        SpawnFloatingText(effectiveness, textColor, hitPoint);

        limbHp -= damage;
        limbHp = Mathf.Max(limbHp, 0);
        player.itemHeldData = null;

        bool badlyDamaged = limbHp <= maxLimbHp * 0.25f && limbHp > 0;
        bool killed = limbHp <= 0;

        if (killed || badlyDamaged)
            TryDrop();

        if (cameraMover != null)
            cameraMover.moveNow = true;
    }

    void SpawnFloatingText(string message, Color color, Vector3 worldPos)
    {
        GameObject go = new GameObject("FloatingText");
        go.transform.position = worldPos + Vector3.up * textHeight;
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localScale = Vector3.one * 0.01f;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = message;
        tmp.color = color;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, 200);
        StartCoroutine(FloatAndFade(go.transform, tmp));
    }

    IEnumerator FloatAndFade(Transform t, TextMeshProUGUI tmp)
    {
        float duration = 2f;
        float elapsed = 0f;
        Vector3 startPos = t.position;
        Color originalColor = tmp.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            t.position = startPos + Vector3.up * (elapsed * 0.5f);
            t.LookAt(t.position + Camera.main.transform.forward);
            float alpha = 1f - elapsed / duration;
            tmp.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        Destroy(t.gameObject);
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

    void TryDrop()
    {
        if (drops == null || drops.Count == 0) return;

        Vector3 dropPos;
        if (dropPoint != null)
            dropPos = dropPoint.position;
        else
        {
            Vector3 playerForward = player.transform.forward;
            dropPos = player.transform.position + playerForward * dropDistance;
            dropPos.y = transform.position.y;
        }

        itemWasDropped = false;
        foreach (LimbDrop drop in drops)
        {
            if (drop.item == null || drop.item.itemPrefab == null) continue;
            float roll = Random.Range(0f, 100f);
            if (roll <= drop.dropChance)
            {
                GameObject dropped = Instantiate(drop.item.itemPrefab, dropPos, Quaternion.identity);
                StartCoroutine(DroppedItemAnimation(dropped.transform));
                itemWasDropped = true;
            }
        }
    }

    IEnumerator DroppedItemAnimation(Transform t)
    {
        float bobSpeed = 2f;
        float bobHeight = 0.15f;
        float rotateSpeed = 90f;
        Vector3 basePos = t.position;
        while (t != null)
        {
            float y = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            t.position = basePos + Vector3.up * y;
            t.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            yield return null;
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