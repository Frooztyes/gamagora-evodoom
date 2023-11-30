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

    public void DequeueEnnemies(Queue<Ennemy.AttackType> queueEnnemies)
    {
        while (queueEnnemies.Count > 0)
        {
            Ennemy.AttackType ennemy = queueEnnemies.Dequeue();
            AddEnnemyToObjects(ennemy);
        }
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
            Image newEnnemy = Instantiate(ennemyBlock, transform.position, Quaternion.identity).GetComponent<Image>();
            newEnnemy.sprite = s.image;
            newEnnemy.transform.SetParent(ennemiesBlock.transform);
            newEnnemy.transform.localScale = ennemyBlock.transform.localScale;
        }
    }
}
