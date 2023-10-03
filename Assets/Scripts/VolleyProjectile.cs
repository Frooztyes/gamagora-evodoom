using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolleyProjectile : AttackPattern
{
    [SerializeField] private GameObject projectile;
    [SerializeField] private float spread;
    [SerializeField] private int bulletPerVolley;
    [SerializeField] private float bulletPerSecond;
    [SerializeField] private int damage;

    private AudioSource projectileSound;

    int idAngle;
    bool upwardSpread;

    public bool IsShooting = false;

    private void OnDrawGizmos()
    {
        if(bulletPerVolley <= 1)
        {
            Gizmos.DrawRay(transform.position, new Vector2(1, 0));
            return;
        } 

        for (int i = 0; i < bulletPerVolley; i++)
        {
            float percent = (1.0f * i / (bulletPerVolley - 1));
            float angle = (spread * 2.0f * percent) - spread;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * transform.right * (transform.lossyScale.x < 0 ? -1 : 1);
            Gizmos.DrawRay(transform.position, dir);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        projectileSound = GetComponent<AudioSource>();
        idAngle = Random.Range(0, bulletPerVolley);
        upwardSpread = true;
        //Projectile p = Instantiate(projectile, projectilePosition.position, Quaternion.identity).GetComponent<Projectile>();
        InvokeRepeating("ShootProjectile", 0, 1.0f/bulletPerSecond);
    }

    public override void StartAttack()
    {
        IsShooting = true;
    }

    public override void StopAttack()
    {
        IsShooting = false;
    }

    void ShootProjectile()
    {
        if (!IsShooting) return;
        float percent = 1.0f * idAngle / (bulletPerVolley - 1);
        float angle = (spread * 2.0f * percent) - spread;
        Vector2 dir = Quaternion.Euler(0f, 0f, angle) * transform.right * (transform.lossyScale.x < 0 ? -1 : 1);

        Projectile p = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Projectile>();
        projectileSound.Play();
        p.setStatistics(dir, damage);

        idAngle += upwardSpread ? 1 : -1;
        if(idAngle >= bulletPerVolley)
        {
            idAngle = bulletPerVolley - 2;
            upwardSpread = !upwardSpread;
        } 
        else if(idAngle < 0)
        {
            idAngle = 1;
            upwardSpread = !upwardSpread;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
