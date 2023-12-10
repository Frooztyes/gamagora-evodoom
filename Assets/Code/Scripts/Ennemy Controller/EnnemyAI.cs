using UnityEngine;
using Pathfinding;
using UnityEditor;

public class EnnemyAI : MonoBehaviour
{
    #region Pattern
    [System.Serializable]
    private struct Pattern
    {
        public string name;
        public GameObject attackPattern;
    }

    GameObject FindPatternByName(string name)
    {
        foreach (Pattern p in patterns)
        {
            if (p.name == name) return p.attackPattern;
        }
        return null;
    }
    #endregion

    enum State
    {
        IDLE,
        RUN,
        SHOOT,
        DEAD
    }

    public Ennemy ennemy;
    private Ennemy editableEnnemy;

    [Header("Behaviour")]
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask canSee;

    [Header("Attack patterns")]
    [SerializeField] private Pattern[] patterns;

    [Header("Misc")]
    [SerializeField] private float nextWaypointDistance = 3f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private bool FacingRight = true;
    [SerializeField] private GameObject coin;

    private Transform pivot;
    private GameObject ennemyGFX;
    private AttackPattern attackGameObject;
    private Transform attackPosition;

    private MyCharacterController player;

    private Animator animator;

    private State currentState;
    private bool isInvincible = false;
    private bool IsRed = false;

    // A*
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private Seeker seeker;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private float defaultRotation;

    private GameHandler gameHandler;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(System.Environment.TickCount);

        editableEnnemy = Instantiate(ennemy);

        pivot = transform.GetChild(0).gameObject.transform;

        ennemyGFX = Instantiate(editableEnnemy.VFX, transform.position, Quaternion.identity);
        ennemyGFX.transform.parent = pivot;

        if(!FacingRight)
        {
            transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        } 
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        CenterSprite();
        
        
        sprite = ennemyGFX.GetComponent<SpriteRenderer>();
        attackPosition = ennemyGFX.transform.GetChild(0);
        animator = ennemyGFX.GetComponent<Animator>();

        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = editableEnnemy.GravityScale;
        currentState = State.IDLE;

        if(target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        player = target.GetComponent<MyCharacterController>();
        gameHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<GameHandler>();

        // Update A* path every .5 seconds
        InvokeRepeating(nameof(UpdatePath), 0f, .5f);
    }

    private void CenterSprite()
    {
        SpriteRenderer sprite = ennemyGFX.GetComponent<SpriteRenderer>();
        ennemyGFX.transform.localPosition = new Vector3(
            (sprite.bounds.size.x / 2) * editableEnnemy.pivotPoint.x, 
            (sprite.bounds.size.y / 2) * editableEnnemy.pivotPoint.y, 0
        );
    }

    #region Editor
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (editableEnnemy == null) editableEnnemy = Instantiate(ennemy);
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, transform.forward, editableEnnemy.AggroRadius);
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, transform.forward, editableEnnemy.ShootRadius);
    }
    #endif
    #endregion

    #region A*
    void UpdatePath()
    {
        if (seeker.IsDone() && currentState == State.RUN)
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    #endregion

    #region StateMachine
    State ChangeState()
    {
        if(editableEnnemy.AttackPattern == Ennemy.AttackType.MELEE)
            animator.SetBool("Reset", false);
        if (currentState == State.DEAD)
            return State.DEAD;

        if (ennemy.IsDead)
        {
            EnterDeadState();
            return State.DEAD;
        }
        if (attackGameObject != null && !attackGameObject.IsOver()) return State.SHOOT;

        if (IsTargetInShootingRange() && currentState == State.SHOOT) return State.SHOOT;
        else if (IsTargetInShootingRange() && (currentState == State.IDLE || currentState == State.RUN))
        {
            EnterShootState();
            return State.SHOOT;
        }

        if (IsTargetInAggroRange() && currentState == State.RUN) return State.RUN;
        else if (IsTargetInAggroRange() && !IsTargetInShootingRange()
            && (currentState == State.IDLE || currentState == State.SHOOT)
            && editableEnnemy.CanMove)
        {
            if (State.SHOOT == currentState) ExitShootState();
            EnterRunState();
            return State.RUN;
        }

        if (!IsTargetInAggroRange() && currentState == State.IDLE) return State.IDLE;
        if (!IsTargetInAggroRange() && (currentState == State.RUN || currentState == State.SHOOT) && editableEnnemy.CanLooseAggro)
        {
            if (State.SHOOT == currentState) ExitShootState();
            EnterIdleState();
            return State.IDLE;
        }

        return currentState;
    }

    #region RunState
    void EnterRunState()
    {
        animator.SetBool("IsShooting", false);
    }

    void TickRunState()
    {
        if (path == null) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        reachedEndOfPath = false;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * editableEnnemy.MoveSpeed * Time.deltaTime;
        animator.SetFloat("Speed", force.magnitude);

        if(rb.gravityScale > 0)
        {
            force *= rb.gravityScale;
        }
        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
            currentWaypoint++;

        if(editableEnnemy.CanRotate)
            transform.localRotation = Quaternion.Euler(0, target.position.x < transform.position.x ? 180 : 0, 0);
    }
    #endregion

    #region IdleState
    void EnterIdleState()
    {
        animator.SetBool("IsShooting", false);
    }

    void TickIdleState()
    {

    }

    #endregion

    #region ShootState
    void ExitShootState()
    {
        if(attackGameObject != null)
            Destroy(attackGameObject.gameObject);
        if (editableEnnemy.AttackPattern == Ennemy.AttackType.MELEE)
        {
            animator.SetBool("Reset", true);
            Destroy(ennemyGFX.GetComponent<DashAttack>());
        }
    }

    void EnterShootState()
    {
        GameObject go;
        switch (editableEnnemy.AttackPattern)
        {
            case Ennemy.AttackType.VOLLEY:

                //attackPattern.TryGetValue("VOLLEY", out o);
                if (go = FindPatternByName("VOLLEY"))
                {
                    attackGameObject = Instantiate(go, attackPosition.position, Quaternion.identity).GetComponent<VolleyProjectile>();
                }
                break;
            case Ennemy.AttackType.LASER:
                if (go = FindPatternByName("LASER"))
                {
                    attackGameObject = Instantiate(go, attackPosition.position, Quaternion.identity).GetComponent<Laser>();
                }
                break;
            case Ennemy.AttackType.SHOOT:
                break;
            case Ennemy.AttackType.MELEE:
                if(!ennemyGFX.GetComponent<DashAttack>())
                {
                    DashAttack script = ennemyGFX.AddComponent<DashAttack>();
                    script.SetInformations(target, animator, rb);
                    script.StartAttack();
                    animator.SetFloat("Speed", 0f);
                }
                break;

            case Ennemy.AttackType.ROCKET:
                if (go = FindPatternByName("ROCKET"))
                {
                    attackGameObject = Instantiate(go, attackPosition.position, go.transform.rotation).GetComponent<RocketPattern>();
                    //attackGameObject.transform.Rotate(transform.rotation.eulerAngles.y * Vector2.up);
                }
                break;



            default:
                break;
        }
        if(attackGameObject)
        {
            attackGameObject.transform.parent = attackPosition;
            attackGameObject.transform.localRotation = Quaternion.identity;
            attackGameObject.StartAttack();
        }
        animator.SetBool("IsShooting", true);
    }

    void TickShootState()
    {
        if (editableEnnemy.CanRotate && 
            (
                editableEnnemy.AttackPattern != Ennemy.AttackType.LASER || 
                (editableEnnemy.AttackPattern == Ennemy.AttackType.LASER && attackGameObject.IsOver())
            )
           )
        {
            transform.localRotation = Quaternion.Euler(0, target.position.x < transform.position.x ? 180 : 0, 0);
        }

        switch (editableEnnemy.AttackPattern)
        {
            case Ennemy.AttackType.VOLLEY:
                break;
            case Ennemy.AttackType.LASER:
                break;
            case Ennemy.AttackType.SHOOT:
                break;
            case Ennemy.AttackType.MELEE:
                animator.SetFloat("Speed", 0f);
                DashAttack d = ennemyGFX.GetComponent<DashAttack>();
                if (d && d.IsOver() && rb.velocity == Vector2.zero && !d.IsReloading())
                {
                    d.StartAttack();
                }
                break;
            case Ennemy.AttackType.ROCKET:
                if((attackGameObject as RocketPattern).IsReloading())
                {
                    animator.SetBool("IsShooting", false);
                } 
                else
                {
                    animator.SetBool("IsShooting", true);
                }
                break;
        }
    }
    #endregion

    #region DeadState
    void EnterDeadState()
    {
        if (attackGameObject != null) ExitShootState();
        animator.SetBool("IsShooting", false);
        rb.gravityScale = 20;
        animator.SetBool("IsDead", true);
        InvokeRepeating(nameof(ExplosionDeath), 0f, 0.2f);
        InvokeRepeating(nameof(RemoveGameObject), 1f, 0.5f);
        gameHandler.EnnemiesKilled.Enqueue(editableEnnemy.AttackPattern);
    }

    private void ExplosionDeath()
    {
        Vector2 explosionPosition = Vector2.zero;

        float[] rangeX = new float[] {
            sprite.bounds.center.x - sprite.bounds.size.x/2,
            sprite.bounds.center.x + sprite.bounds.size.x/2
        };

        float[] rangeY = new float[] {
            sprite.bounds.center.y - sprite.bounds.size.y/2,
            sprite.bounds.center.y + sprite.bounds.size.y/2
        };


        explosionPosition.x = Random.Range(rangeX[0], rangeX[1]);
        explosionPosition.y = Random.Range(rangeY[0], rangeY[1]);

        //Instantiate(explosionEffect, explosionPosition, Quaternion.identity);
    }

    private void RemoveGameObject()
    {
        CancelInvoke(nameof(ExplosionDeath));
        //int maxCoins = Random.Range(editableEnnemy.MinCoinsOnDeath, editableEnnemy.MaxCoinsOnDeath + 1);
        //for (int i = 0; i < maxCoins; i++)
        //{
        //    Rigidbody2D rb = Instantiate(coin, transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();

        //    Vector2 velocity = Vector2.up * Random.Range(200, 300) + Vector2.right * Random.Range(-200, 200);
        //    rb.AddForce(velocity);

        //}

        if(rb.velocity.y == 0)
            Destroy(gameObject);
    }
    #endregion

    #endregion

    bool IsTargetInAggroRange()
    {
        return Vector2.Distance(transform.position, target.position) <= editableEnnemy.AggroRadius;
    }

    bool IsTargetInShootingRange()
    {
        if (Vector2.Distance(attackPosition.position, target.position) <= editableEnnemy.ShootRadius)
        {
            Vector3 dirTarget = (attackPosition.position - target.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(attackPosition.position, -dirTarget, editableEnnemy.ShootRadius, canSee);
            return hit && hit.collider.gameObject == target.gameObject;
        }
        return false;
    }

    public void TakeDamage(int damage, bool fromRight)
    {
        if (isInvincible) return;
        if(editableEnnemy.TakeDamage(1))
        {
            EnterDeadState();
            currentState = State.DEAD;
        } 
        else
        {
            InvokeRepeating(nameof(BlinkRed), 0, 0.2f);
            Invoke(nameof(EndInvincibleFrame), editableEnnemy.InvincibleTime);
        }
    }

    public void BlinkRed()
    {
        IsRed = !IsRed;
        ennemyGFX.GetComponent<SpriteRenderer>().color = IsRed ? Color.red : Color.white;
    }

    private void EndInvincibleFrame()
    {
        CancelInvoke(nameof(BlinkRed));
        isInvincible = false;
        if (IsRed) BlinkRed();
    }

    /// <summary>
    /// Pseudo-implementation of a state machine
    /// </summary>
    void FixedUpdate()
    {
        if (player.EditableChar.IsDead)
        {
            if (attackGameObject != null)
                Destroy(attackGameObject.gameObject);
            return;
        }

        currentState = ChangeState();
        if (currentState != State.SHOOT && attackGameObject && attackGameObject.IsOver())
            Destroy(attackGameObject.gameObject);

        if (currentState == State.DEAD)
        {
            // handle death
            return;
        }

        if(currentState == State.SHOOT)
        {
            // handle shooting
            TickShootState();
            return;
        }
        
        if(currentState == State.RUN)
        {
            TickRunState();
        }

        if(currentState == State.IDLE)
        {
            TickIdleState();
        }
    }

    public int GetContactDamage()
    {
        if(currentState == State.SHOOT && editableEnnemy.AttackPattern == Ennemy.AttackType.MELEE)
        {
            DashAttack d = ennemyGFX.GetComponent<DashAttack>();
            if (d && !d.IsOver())
            {
                return editableEnnemy.ContactDamage * 2;
            }
        }
        if (editableEnnemy.IsDead) return 0;
        return editableEnnemy.ContactDamage;
    }

    public bool IsDead()
    {
        return editableEnnemy.IsDead;
    }
}