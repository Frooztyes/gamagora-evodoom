using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor;
using System;
using Unity.VisualScripting;
using UnityEngine.Rendering.PostProcessing;

public class EnnemyAI : MonoBehaviour
{
    #region Pattern
    [Serializable]
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

    private GameObject pivot;
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

    // Start is called before the first frame update
    void Start()
    {
        editableEnnemy = Instantiate(ennemy);

        pivot = transform.GetChild(0).gameObject;

        ennemyGFX = Instantiate(editableEnnemy.VFX, transform.position, Quaternion.identity);
        ennemyGFX.transform.parent = pivot.transform;
        ennemyGFX.transform.localPosition = editableEnnemy.pivotPoint;

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
            ennemyGFX.transform.localScale = new Vector3(
                Mathf.Abs(ennemyGFX.transform.localScale.x),
                ennemyGFX.transform.localScale.y,
                ennemyGFX.transform.localScale.z
            );
        }
        else if (force.x <= -0.01f)
        {
            ennemyGFX.transform.localScale = new Vector3(
                -Mathf.Abs(ennemyGFX.transform.localScale.x),
                ennemyGFX.transform.localScale.y,
                ennemyGFX.transform.localScale.z
            );
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

                    attackGameObject.transform.parent = attackPosition;
                    if (target.position.x < transform.position.x)
                        attackGameObject.transform.localScale = new Vector3(-attackGameObject.transform.localScale.x, attackGameObject.transform.localScale.y, attackGameObject.transform.localScale.z);

                    attackGameObject.StartAttack();
                }
                break;
            case Ennemy.AttackType.LASER:
                if (go = FindPatternByName("LASER"))
                {
                    attackGameObject = Instantiate(go, attackPosition.position, Quaternion.identity).GetComponent<Laser>();

                    attackGameObject.transform.parent = attackPosition;
                    if (target.position.x > transform.position.x)
                        attackGameObject.transform.localScale = new Vector3(-attackGameObject.transform.localScale.x, attackGameObject.transform.localScale.y, attackGameObject.transform.localScale.z);

                    attackGameObject.StartAttack();
                }
                break;
            case Ennemy.AttackType.SHOOT:
                break;
            case Ennemy.AttackType.MELEE:
                break;

            case Ennemy.AttackType.ROCKET:
                Debug.Log("rocket");
                if (go = FindPatternByName("ROCKET"))
                {
                    attackGameObject = Instantiate(go, attackPosition.position, go.transform.rotation).GetComponent<RocketPattern>();
                    attackGameObject.transform.Rotate(Vector2.up * 180);
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
                ennemyGFX.transform.localScale = new Vector3(
                    Mathf.Abs(ennemyGFX.transform.localScale.x),
                    ennemyGFX.transform.localScale.y,
                    ennemyGFX.transform.localScale.z
                );
            }
            else if (isBehind)
            {
                ennemyGFX.transform.localScale = new Vector3(
                    -Mathf.Abs(ennemyGFX.transform.localScale.x),
                    ennemyGFX.transform.localScale.y,
                    ennemyGFX.transform.localScale.z
                );
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
                break;
        }
    }
    #endregion

    #region DeadState
    void EnterDeadState()
    {
        if (attackGameObject != null) ExitShootState();
        animator.SetBool("IsShooting", false);
        rb.gravityScale = 2;
        animator.SetBool("IsDead", true);
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
}