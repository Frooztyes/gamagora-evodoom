using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketPattern : AttackPattern
{
    [SerializeField] private GameObject projectile;

    int rocketPositionIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
    }

    bool launching = false;
    bool canStart = false;

    void CreateMissile()
    {
        GameObject rocketPosition = transform.GetChild(rocketPositionIndex).gameObject;
        GameObject miss = Instantiate(projectile, rocketPosition.transform.position, rocketPosition.transform.rotation);
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
            rocketPositionIndex = 0;
            Invoke(nameof(CreateMissile), 5f);
        }
    }

    public override void StartAttack()
    {
        canStart = true;
    }

    public override void StopAttack()
    {
        canStart = false;
    }
}
