using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject pressRestart;
    [SerializeField] private string mainMenuScene;
    [SerializeField] private float timeByEnnemies = 0.5f;

    private CustomInputs input = null;
    Queue<Ennemy.AttackType> queueEnnemies;
    private void Awake()
    {
        input = new CustomInputs();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.InteractBis.performed += OnRestartPerformed;
    }
    private void OnDisable()
    {
        input.Disable();
        input.Player.InteractBis.performed -= OnRestartPerformed;
    }

    bool canRestart;

    private void OnRestartPerformed(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(mainMenuScene);
    }


    private void Start()
    {
        IsDequeueing = false;
        canRestart = false;
        pressRestart.SetActive(false);
    }

    public void DequeueEnnemies(Queue<Ennemy.AttackType> _queueEnnemies)
    {
        queueEnnemies = _queueEnnemies;
        IsDequeueing = true;
        Invoke(nameof(DequeueEnnemy), timeByEnnemies + 2f);
    }

    public bool IsDequeueing { private set; get; }

    private void DequeueEnnemy()
    {
        if (queueEnnemies.Count > 0)
        {
            Ennemy.AttackType ennemy = queueEnnemies.Dequeue();
            AddEnnemyToObjects(ennemy);
            Invoke(nameof(DequeueEnnemy), timeByEnnemies);
        }
        else
        {
            Debug.Log("Hii2");
            pressRestart.SetActive(true);
            canRestart = true;
        }

    }
    int defaultOrder = 100;
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
            newEnnemy.GetComponent<Canvas>().sortingOrder = defaultOrder;
            defaultOrder++;
        }
    }
}
