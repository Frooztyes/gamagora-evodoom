using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RocketPattern : AttackPattern
{
    [SerializeField] private GameObject projectile;

    int rocketPositionIndex = 0;
    private AudioSource rocketLaunch;
    // Start is called before the first frame update
    void Start()
    {
        rocketLaunch = GetComponent<AudioSource>(); 
    }

    bool launching = false;
    bool canStart = false;
    private bool isReloading = true;

    void CreateMissile()
    {
        isReloading = false;

        GameObject rocketPosition = transform.GetChild(rocketPositionIndex).gameObject;
        rocketLaunch = rocketPosition.GetComponent<AudioSource>();
        Instantiate(projectile, rocketPosition.transform.position, rocketPosition.transform.rotation);
        rocketLaunch.Play();
        rocketPositionIndex++;
        launching = false;
        HasFinished = false;
    }
    

    // Update is called once per frame
    void Update()
    {
        if (!canStart) return;
        if (launching) return;
        if(rocketPositionIndex < transform.childCount)
        {
            launching = true;
            Invoke(nameof(CreateMissile), 0.5f);
        }  
        else
        {
            launching = true;
            HasFinished = true;
            isReloading = true;
            rocketPositionIndex = 0;
            Invoke(nameof(CreateMissile), 5f);
        }
    }


    public bool IsReloading() { return isReloading; }

    public override void StartAttack()
    {
        canStart = true;
    }

    public override void StopAttack()
    {
        canStart = false;
    }
}
