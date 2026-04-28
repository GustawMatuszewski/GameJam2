using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public List<Limb> limbs;
    public int hp;
    public int attackDamage = 1;
    public PlayerController player;

    void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    void Update()
    {
        hp = 0;
        foreach (Limb limb in limbs)
            hp += limb.limbHp;

        if (hp <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            if (player != null)
            {
                player.TakeDamage(attackDamage);
                Debug.Log($"Enemy attacks player for {attackDamage} damage! Player HP: {player.playerHp}");
            }
        }
    }
}