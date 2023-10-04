using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;

public class Laser : AttackPattern
{
    [SerializeField] private float defDistanceRay = 100;
    [SerializeField] AudioSource laserSound;
    public LayerMask mask;
    public Transform laserFirePoint;
    public LineRenderer m_lineRenderer;
    Transform m_transform;
    [SerializeField] private GameObject endVFX;
    [SerializeField] private float cooldown;

    [SerializeField] private float startingAngle;
    [SerializeField] private float endingAndle;

    [SerializeField] float acceleration = 0.01f;
    [SerializeField] int damage = 10;

    private float currentSpeed;
    private bool inReset = false;
    private bool laserStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        m_transform = GetComponent<Transform>();
        defaultWidth = m_lineRenderer.startWidth;
        // deactivate in editor because of gizmos issue
        m_lineRenderer.useWorldSpace = true;
        endVFX.SetActive(false); 
        
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, startingAngle);

        m_lineRenderer.startWidth = defaultWidth;


    }

    float resetTime = 1f;
    float time = 0f;
    float width;
    float defaultWidth;


    void RotateLaser()
    {
        if (inReset)
        {
            time += Time.deltaTime;
            float newWidth = width * ((resetTime / 4 - time) / resetTime);
            m_lineRenderer.startWidth = newWidth > 0 ? newWidth : 0;
            return;
        }

        float localRotation;
        if (transform.lossyScale.x > 0)
        {
            // faire un truc diff
            //localRotation = transform.rotation.eulerAngles.z < 0 ? transform.rotation.eulerAngles.z + 180 : transform.rotation.eulerAngles.z;
            localRotation = transform.rotation.eulerAngles.z > 180 ? (360.0f - transform.rotation.eulerAngles.z) * -1 : transform.rotation.eulerAngles.z;

        }
        else
        {
            localRotation = transform.rotation.eulerAngles.z > 180 ? (360.0f - transform.rotation.eulerAngles.z) * -1 : transform.rotation.eulerAngles.z;
        }

        if (localRotation >= endingAndle)
        {
            inReset = true;
            width = m_lineRenderer.startWidth;
            time = 0f;
            endVFX.SetActive(false);
            Invoke(nameof(ResetLaser), resetTime);
            return;
        }

        currentSpeed = (endingAndle-startingAngle) * 2.0f * Time.deltaTime;
        transform.Rotate(new Vector3 (0, 0, currentSpeed));
        ShootLaser();
    }

    public override void StartAttack()
    {
        HasFinished = false;
        laserStarted = true;
        restartLaser = true;
        laserSound.Play();
    }

    public override void StopAttack()
    {
        restartLaser = false;
    }

    private bool restartLaser = true;


    void Update()
    {
        if(laserStarted)
        {
            if(!endVFX.activeSelf) endVFX.SetActive(true);
            if(!m_lineRenderer.enabled) m_lineRenderer.enabled = true;
            RotateLaser();
        } 
        else
        {
            m_lineRenderer.enabled = false;
        }
        if(m_lineRenderer.startWidth <= 0)
        {
            endVFX.SetActive(false);
        }
        if(!laserSound.isPlaying) HasFinished = true;
        else HasFinished = false;
    }


    void ResetLaser()
    {
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, startingAngle);
        inReset = false;
        laserStarted = false;
        m_lineRenderer.startWidth = defaultWidth;
        if(restartLaser)
            Invoke(nameof(StartAttack), cooldown);
    }

    void ShootLaser()
    {
        if(Physics2D.Raycast(m_transform.position, transform.right, defDistanceRay, mask))
        {
            RaycastHit2D _hit = Physics2D.Raycast(m_transform.position, transform.right, defDistanceRay, mask);
            Draw2DRay(laserFirePoint.position, _hit.point);
            if (LayerMask.LayerToName(_hit.collider.gameObject.layer) == "Player")
            {
                MyCharacterController player = _hit.collider.gameObject.GetComponent<MyCharacterController>();
                player.TakeDamage(damage, player.transform.position.x < transform.position.x);
            }
        } 
        else
        {
            Draw2DRay(laserFirePoint.position, laserFirePoint.transform.right * defDistanceRay);
        }
    }

    void Draw2DRay(Vector2 startPos, Vector2 endPos)
    {
        m_lineRenderer.SetPosition(0, startPos);
        m_lineRenderer.SetPosition(1, endPos);
        endVFX.transform.position = endPos;
        endVFX.transform.rotation = Quaternion.Euler(0, 0, -Vector2.Angle(startPos, endPos));
    }
}
