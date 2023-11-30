using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    MyCharacterController player;

    [SerializeField] private DeathMenu deathMenu;

    public Queue<Ennemy.AttackType> EnnemiesKilled { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<MyCharacterController>();
        EnnemiesKilled = new Queue<Ennemy.AttackType>();
        deathMenu.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(player.editableChar.IsDead)
        {
            deathMenu.gameObject.SetActive(true);
            deathMenu.DequeueEnnemies(EnnemiesKilled);
        }
    }
}
