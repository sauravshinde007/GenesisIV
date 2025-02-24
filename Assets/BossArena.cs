using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArena : MonoBehaviour
{
    public BossAI boss;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (boss == null) return;
            boss.isPlayerInBossArea = true;
            boss.healthBar.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (boss == null) return;
            boss.isPlayerInBossArea = false;
            boss.healthBar.gameObject.SetActive(false);
        }
    }
}
