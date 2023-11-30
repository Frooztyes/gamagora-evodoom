using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{

    [System.Serializable]
    private struct SpriteEnnemy
    {
        public Ennemy.AttackType typeEnnemy;
        public Sprite image;
    }

    [SerializeField] private List<SpriteEnnemy> ennemies;
    [SerializeField] private GameObject ennemyBlock;
    [SerializeField] private GameObject ennemiesBlock;

    // Start is called before the first frame update
    void Start()
    {
        AddEnnemyToObjects(Ennemy.AttackType.LASER);
        AddEnnemyToObjects(Ennemy.AttackType.VOLLEY);
        AddEnnemyToObjects(Ennemy.AttackType.MELEE);
        AddEnnemyToObjects(Ennemy.AttackType.LASER);
        AddEnnemyToObjects(Ennemy.AttackType.MELEE);
        AddEnnemyToObjects(Ennemy.AttackType.LASER);
        AddEnnemyToObjects(Ennemy.AttackType.LASER);
        AddEnnemyToObjects(Ennemy.AttackType.MELEE);

    }

    void AddEnnemyToObjects(Ennemy.AttackType ennemyType)
    {
        SpriteEnnemy? se = null;
        for (int i = 0; i < ennemies.Count; i++)
        {
            if(ennemies[i].typeEnnemy == ennemyType)
            {
                se = ennemies[i];
            }
        }
        if (se is SpriteEnnemy s)
        {
            GameObject newEnnemy = Instantiate(ennemyBlock, transform.position, Quaternion.identity);
            newEnnemy.GetComponent<Image>().sprite = s.image;
            newEnnemy.transform.SetParent(ennemiesBlock.transform);

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
