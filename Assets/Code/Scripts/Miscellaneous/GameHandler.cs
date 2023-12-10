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
        // if the player is dead, start unqueueing enemmy to show death screen
        if(player.EditableChar.IsDead && !deathMenu.IsDequeueing && !deathMenu.gameObject.activeSelf)
        {
            deathMenu.gameObject.SetActive(true);
            SpriteRenderer sp = player.GetComponent<SpriteRenderer>();
            player.GetComponent<PlayerInputs>().enabled = false;
            player.deathSound.Play();
            sp.sortingLayerID = SortingLayer.NameToID("Text");
            sp.sortingOrder = 99;
            deathMenu.DequeueEnnemies(EnnemiesKilled);
        }
    }
}
