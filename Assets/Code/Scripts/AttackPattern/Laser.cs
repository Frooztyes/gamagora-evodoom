using UnityEngine;

public class Laser : AttackPattern
{
    [SerializeField] private float defDistanceRay = 100;
    [SerializeField] AudioSource laserSound;
    [SerializeField] AudioSource tickSound;
    public LayerMask mask;
    public Transform laserFirePoint;
    public LineRenderer m_lineRenderer;
    Transform m_transform;
    [SerializeField] private GameObject endVFX;
    [SerializeField] private float cooldown;

    [SerializeField] private float laserWidth;
    [SerializeField] private float indicatorWidth;
    [SerializeField] private float startingAngle;
    [SerializeField] private float endingAndle;

    [SerializeField] int damage = 10;

    private float currentSpeed;
    private bool inReset = false;
    private bool laserStarted = false;

    float resetTime = 1f;
    float time = 0f;
    float width;

    // Start is called before the first frame update
    void Start()
    {
        m_transform = GetComponent<Transform>();
        // deactivate in editor because of gizmos issue
        m_lineRenderer.useWorldSpace = true;
        endVFX.SetActive(false);

        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, startingAngle);

        m_lineRenderer.startWidth = 0;
        StartAttack();
    }

    private bool playIndicator = false;
    private bool playLaser = false;

    private void StartIndicator()
    {
        playLaser = false;
        playIndicator = true;
        tickSound.pitch = 1;

        m_lineRenderer.startWidth = indicatorWidth;
    }

    private void StartLaser()
    {
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, startingAngle);

        playIndicator = false;
        playLaser = true;

        m_lineRenderer.startWidth = laserWidth;
        laserStarted = true;
        laserSound.Play();
    }


    public override void StartAttack()
    {
        hasFinished = false;
        restartLaser = true;
        StartIndicator();
    }

    public override void StopAttack()
    {
        restartLaser = false;
    }

    void RotateLaser()
    {
        if (inReset)
        {
            time += Time.deltaTime;
            float newWidth = width * ((resetTime / 4 - time) / resetTime);
            m_lineRenderer.startWidth = newWidth > 0 ? newWidth : 0;
            return;
        }


        float angle;
        if(transform.localRotation.eulerAngles.z > 180)
        {
            angle = -360 - transform.localRotation.eulerAngles.z;
        } 
        else
        {
            angle = transform.localRotation.eulerAngles.z;
        }

        if (angle >= endingAndle)
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
        ShootLaser(true);
    }

    private float stackTime = 0;
    void RotateIndicator()
    {
        float angle = transform.localRotation.eulerAngles.z;
        if (transform.localRotation.eulerAngles.z > 180)
        {
            angle = -360 - angle;
        }

        if (angle >= endingAndle)
        {
            StartLaser();
            return;
        }

        stackTime += Time.deltaTime;
        if (stackTime >= 0.04)
        {
            transform.Rotate(Vector3.forward * 5);
            tickSound.Play();
            tickSound.pitch += 0.05f;
            stackTime = 0;
        }
        ShootLaser(false);
    }

    private bool restartLaser = false;

    void Update()
    {
        if(playIndicator)
        {
            if (!m_lineRenderer.enabled) m_lineRenderer.enabled = true;
            RotateIndicator();
            return;
        }

        if(playLaser)
        {
            if (!endVFX.activeSelf) endVFX.SetActive(true);
            if (!m_lineRenderer.enabled) m_lineRenderer.enabled = true;
            RotateLaser();
        }

        if (m_lineRenderer.startWidth <= 0)
        {
            endVFX.SetActive(false);
        }

    }



    void ResetLaser()
    {
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, startingAngle);
        hasFinished = true;
        inReset = false;
        playLaser = false;
        m_lineRenderer.startWidth = laserWidth;
        m_lineRenderer.enabled = false;
        if (restartLaser)
            Invoke(nameof(StartAttack), cooldown);
    }

    void ShootLaser(bool damageful)
    {
        if(Physics2D.Raycast(m_transform.position, transform.right, defDistanceRay, mask))
        {
            RaycastHit2D _hit = Physics2D.Raycast(m_transform.position, transform.right, defDistanceRay, mask);
            Draw2DRay(laserFirePoint.position, _hit.point);
            if (LayerMask.LayerToName(_hit.collider.gameObject.layer) == "Player" && damageful)
            {
                MyCharacterController player = _hit.collider.gameObject.GetComponent<MyCharacterController>();
                if (!player.hasDodge)
                {
                    player.TakeDamage(player.transform.position.x < transform.position.x);
                }
            }
        } 
        else
        {
            Draw2DRay(laserFirePoint.position, transform.right * defDistanceRay + transform.position);
            RaycastHit2D _hit = Physics2D.Raycast(laserFirePoint.position, transform.right, defDistanceRay, mask);
            if (_hit && LayerMask.LayerToName(_hit.collider.gameObject.layer) == "Player" && damageful)
            {
                MyCharacterController player = _hit.collider.gameObject.GetComponent<MyCharacterController>();
                if (!player.hasDodge)
                {
                    player.TakeDamage(player.transform.position.x < transform.position.x);
                }
            }
        }
        Debug.DrawRay(laserFirePoint.position, transform.right * defDistanceRay, Color.green);
    }

    void Draw2DRay(Vector2 startPos, Vector2 endPos)
    {
        m_lineRenderer.SetPosition(0, startPos);
        m_lineRenderer.SetPosition(1, endPos);
        endVFX.transform.position = endPos;
        endVFX.transform.rotation = Quaternion.Euler(0, 0, -Vector2.Angle(startPos, endPos));
    }

    public override bool IsOver()
    {
        return hasFinished;
    }
}
