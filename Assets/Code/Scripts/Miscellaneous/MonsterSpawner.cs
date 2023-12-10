using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public enum MonsterType 
    {
        MELEE, VOLLEY, ROCKET, LASER
    }

    [System.Serializable]
    private struct MonsterTypeObject
    {
        public MonsterType type;
        public Ennemy scriptableObject;
    }

    [SerializeField] private List<MonsterTypeObject> monsters;
    [SerializeField] private GameObject ennemyPrefab;

    /// <summary>
    /// Spawn an enemmy at position considering his type
    /// </summary>
    /// <param name="type"></param>
    public void SpawnMonster(MonsterType type)
    {
        Ennemy e = monsters.Where(i => i.type == type).FirstOrDefault().scriptableObject;
        if (e.VFX == null) return;
        Vector3 randomDir = Vector3.zero;

        if (type == MonsterType.ROCKET) 
            randomDir = new(0, Random.value > 0.5 ? 0 : 180, 0);

        EnnemyAI g = Instantiate(ennemyPrefab, transform.position, Quaternion.Euler(randomDir)).GetComponent<EnnemyAI>();
        
        g.ennemy = e;
    }
}
