using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ItemSelector : MonoBehaviour
{
    public List<ItemData> items;
    public int currentIndex = 0;
    public ParticleSystem selectParticles;
    GameObject currentInstance;

    public MagicType CurrentMagicType => items.Count > 0 ? items[currentIndex].itemType : MagicType.Water;

    void Start()
    {
        if (items.Count > 0)
            SpawnItem(items[0]);
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
                SpawnParticles(hit.point);
                currentIndex = (currentIndex + 1) % items.Count;
                SpawnItem(items[currentIndex]);
                return;
            }
        }
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
}