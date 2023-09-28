using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor;

public class EnnemyAI : MonoBehaviour
{
    enum State
    {
        IDLE,
        RUN,
        SHOOT,
        DEAD
    }

    enum AttackType
    {
        VOLEY,
        LASER,
        SHOOT,
        MELEE
    }

    [SerializeField] private Transform target;
    [SerializeField] private float speed = 200f;
    [SerializeField] private float nextWaypointDistance = 3f;
    [SerializeField] private Transform ennemyGFX;
    [SerializeField] private float aggroRadius = 10f;
    [SerializeField] private float shootingRadius = 5f;
    [SerializeField] private bool canLooseAggro = false;
    [SerializeField] private LayerMask canSee;
    [SerializeField] private Animator animator;
    [SerializeField] private AttackType attackType;
    [SerializeField] private GameObject attackPattern;
    [SerializeField] private Transform attackPosition;


    private GameObject attackGO;

    private State currentState;

    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        currentState = State.IDLE;
        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    void UpdatePath()
    {
        if (seeker.IsDone() && currentState == State.RUN)
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, transform.forward, aggroRadius);
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, transform.forward, shootingRadius);
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    bool IsTargetInAggroRange()
    {
        return Vector2.Distance(transform.position, target.position) <= aggroRadius;
    }

    bool IsTargetInShootingRange()
    {
        if (Vector2.Distance(transform.position, target.position) <= shootingRadius)
        {
            Vector3 dirTarget = (transform.position - target.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -dirTarget, shootingRadius, canSee);
            return hit && hit.collider.gameObject == target.gameObject;
        }
        return false;
    }

    void EnterRunState()
    {
        animator.SetBool("IsShooting", false);
    }

    void ExitShootState()
    {
        Destroy(attackGO);
    }

    void EnterShootState()
    {
        switch (attackType)
        {
            case AttackType.VOLEY:
                GameObject o = attackPattern;
                //attackPattern.TryGetValue("VOLLEY", out o);
                if(o)
                {
                    attackGO = Instantiate(o, attackPosition.position, Quaternion.identity);
                    attackGO.transform.parent = attackPosition;
                    if (target.position.x > transform.position.x)
                        attackGO.transform.localScale = new Vector3(-attackGO.transform.localScale.x, attackGO.transform.localScale.y, attackGO.transform.localScale.z);
                }
                break;
            case AttackType.LASER:
                break;
            case AttackType.SHOOT:
                break;
            case AttackType.MELEE:
                break;
            default:
                break;
        }
        animator.SetBool("IsShooting", true);
    }

    void EnterDeadState()
    {
        animator.SetBool("IsShooting", false);
        animator.SetBool("IsDead", true);
    }

    void EnterIdleState()
    {
        animator.SetBool("IsShooting", false);
    }

    State ChangeState()
    {
        //if (ennemy.isDead())
        //{
        //    EnterDeadState()
        //    newState = State.Dead;
        //}

        if (IsTargetInShootingRange() && currentState == State.SHOOT) return State.SHOOT;
        else if (IsTargetInShootingRange() && (currentState == State.IDLE || currentState == State.RUN))
        {
            EnterShootState();
            return State.SHOOT;
        }

        if (IsTargetInAggroRange() && currentState == State.RUN) return State.RUN;
        else if (IsTargetInAggroRange() && !IsTargetInShootingRange() && (currentState == State.IDLE || currentState == State.SHOOT))
        {
            if (State.SHOOT == currentState) ExitShootState();
            EnterRunState();
            return State.RUN;
        }

        if (!IsTargetInAggroRange() && currentState == State.IDLE) return State.IDLE;
        if (!IsTargetInAggroRange() && (currentState == State.RUN || currentState == State.SHOOT) && canLooseAggro)
        {
            if (State.SHOOT == currentState) ExitShootState();
            EnterIdleState();
            return State.IDLE;
        }

        return currentState;

    }

    void StateRunTick()
    {
        if (path == null) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        reachedEndOfPath = false;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;
        animator.SetFloat("Speed", force.magnitude);

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
            currentWaypoint++;

        if (force.x >= 0.01f)
        {
            ennemyGFX.localScale = new Vector3(Mathf.Abs(ennemyGFX.localScale.x), ennemyGFX.localScale.y, ennemyGFX.localScale.z);
        }
        else if (force.x <= -0.01f)
        {
            ennemyGFX.localScale = new Vector3(-Mathf.Abs(ennemyGFX.localScale.x), ennemyGFX.localScale.y, ennemyGFX.localScale.z);
        }
    }

    void StateIdleTick()
    {

    }

    void StateShootTick()
    {
        bool isBehind = target.position.x < transform.position.x;
        if (!isBehind)
        {
            ennemyGFX.localScale = new Vector3(Mathf.Abs(ennemyGFX.localScale.x), ennemyGFX.localScale.y, ennemyGFX.localScale.z);
        }
        else if (isBehind)
        {
            ennemyGFX.localScale = new Vector3(-Mathf.Abs(ennemyGFX.localScale.x), ennemyGFX.localScale.y, ennemyGFX.localScale.z);
        }

        if (attackType == AttackType.VOLEY)
        {

        }
    }

    // Update is called once per frame
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
            StateShootTick();
            return;
        }
        
        if(currentState == State.RUN)
        {
            StateRunTick();
        }

        if(currentState == State.IDLE)
        {
            StateIdleTick();
        }
    }
}
