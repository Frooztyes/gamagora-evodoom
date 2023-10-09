using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.Rendering.PostProcessing;

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

    [SerializeField] private Ennemy ennemy;
    private Ennemy editableEnnemy;

    [Header("Behaviour")]
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask canSee;

    [Header("Attack patterns")]
    [SerializeField] private Pattern[] patterns;

    [Header("Misc")]
    [SerializeField] private float nextWaypointDistance = 3f;
    [SerializeField] private GameObject explosionEffect;

    private Transform pivot;
    private GameObject ennemyGFX;
    private AttackPattern attackGameObject;
    private Transform attackPosition;

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

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(System.Environment.TickCount);

        editableEnnemy = Instantiate(ennemy);

        pivot = transform.GetChild(0).gameObject.transform;
        defaultRotation = transform.rotation.eulerAngles.y;

        ennemyGFX = Instantiate(editableEnnemy.VFX, transform.position, Quaternion.identity);
        ennemyGFX.transform.parent = pivot;

        if (defaultRotation == 0)
        {
            ennemyGFX.transform.localPosition = new Vector2(-editableEnnemy.pivotPoint.x, editableEnnemy.pivotPoint.y);
        } 
        else
        {
            ennemyGFX.transform.localPosition = editableEnnemy.pivotPoint;
        }
        
        sprite = ennemyGFX.GetComponent<SpriteRenderer>();
        attackPosition = ennemyGFX.transform.GetChild(0);
        animator = ennemyGFX.GetComponent<Animator>();

        BoxCollider2D gfxCollider = ennemyGFX.GetComponent<BoxCollider2D>();
        BoxCollider2D collider = this.AddComponent<BoxCollider2D>();
        collider.size = gfxCollider.size;
        collider.offset = gfxCollider.offset;

        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = editableEnnemy.GravityScale;
        currentState = State.IDLE;

        if(target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Update A* path every .5 seconds
        InvokeRepeating(nameof(UpdatePath), 0f, .5f);
    }

    #region Editor
    private void OnDrawGizmos()
    {
        if (editableEnnemy == null) editableEnnemy = Instantiate(ennemy);
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, transform.forward, editableEnnemy.AggroRadius);
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, transform.forward, editableEnnemy.ShootRadius);
    }
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
        if (currentState == State.DEAD)
            return State.DEAD;

        if (ennemy.IsDead)
        {
            EnterDeadState();
            return State.DEAD;
        }

        if (attackGameObject != null && !attackGameObject.HasFinished) return State.SHOOT;

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

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
            currentWaypoint++;

        if (force.x >= 0.01f)
        {
            ennemyGFX.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (force.x <= -0.01f)
        {
            ennemyGFX.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
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
    }

    void EnterShootState()
    {
        GameObject go = null;
        switch (editableEnnemy.AttackPattern)
        {
            case Ennemy.AttackType.VOLLEY:

                //attackPattern.TryGetValue("VOLLEY", out o);
                if (go = FindPatternByName("VOLLEY"))
                {
                    attackGameObject = Instantiate(go, attackPosition.position, Quaternion.identity).GetComponent<VolleyProjectile>();
                    attackGameObject.transform.Rotate((defaultRotation != 0 ? 180 : 0) * Vector2.up);
                    attackGameObject.transform.parent = attackPosition;
                    attackGameObject.StartAttack();
                }
                break;
            case Ennemy.AttackType.LASER:
                if (go = FindPatternByName("LASER"))
                {
                    attackGameObject = Instantiate(go, attackPosition.position, Quaternion.identity).GetComponent<Laser>();
                    attackGameObject.transform.Rotate(transform.rotation.eulerAngles.y * Vector2.up);
                    attackGameObject.transform.parent = attackPosition;
                    attackGameObject.StartAttack();
                }
                break;
            case Ennemy.AttackType.SHOOT:
                break;
            case Ennemy.AttackType.MELEE:
                break;

            case Ennemy.AttackType.ROCKET:
                if (go = FindPatternByName("ROCKET"))
                {
                    attackGameObject = Instantiate(go, attackPosition.position, go.transform.rotation).GetComponent<RocketPattern>();
                    attackGameObject.transform.Rotate(transform.rotation.eulerAngles.y * Vector2.up);
                    attackGameObject.transform.parent = attackPosition;
                    attackGameObject.StartAttack();
                }
                break;



            default:
                break;
        }
        animator.SetBool("IsShooting", true);
    }

    void TickShootState()
    {
        bool isBehind = target.position.x < transform.position.x;
        if (editableEnnemy.CanRotate)
        {
            if (!isBehind)
            {
                transform.rotation = Quaternion.Euler(0, Mathf.Abs(180 - defaultRotation), 0);
            }
            else if (isBehind)
            {
                transform.rotation = Quaternion.Euler(0, defaultRotation, 0);
            }
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

        Instantiate(explosionEffect, explosionPosition, Quaternion.identity);
    }

    private void RemoveGameObject()
    {
        CancelInvoke(nameof(ExplosionDeath));
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
        if(editableEnnemy.TakeDamage(damage))
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
        sprite.color = IsRed ? Color.red : Color.white;
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
        currentState = ChangeState();

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

    public bool IsDead()
    {
        return editableEnnemy.IsDead;
    }
}