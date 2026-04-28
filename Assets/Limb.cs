using UnityEngine;
using UnityEngine.InputSystem;

public class Limb : MonoBehaviour
{
    public int limbHp;
    public MagicType magicType;
    public PlayerController player;
    public ParticleSystem hitParticles;

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

        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(hitParticles, hitPoint, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration);
        }

        int damage = 1;

        if (IsStrongAgainst(player.itemHeldData.itemType, magicType))
            damage = 2;
        else if (IsWeakAgainst(player.itemHeldData.itemType, magicType))
            damage = 0;

        limbHp -= damage;
        Debug.Log("Dealt " + damage + " damage. Limb HP: " + limbHp);

        player.itemHeldData = null;
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