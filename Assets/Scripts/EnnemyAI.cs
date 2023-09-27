using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor;

public class EnnemyAI : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 200f;
    [SerializeField] private float nextWaypointDistance = 3f;
    [SerializeField] private Transform ennemyGFX;
    [SerializeField] private float aggroRadius = 10f;
    [SerializeField] private float shootingRadius = 5f;
    [SerializeField] private LayerMask canSee;
    [SerializeField] private Animator animator;

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
        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    private bool targetAggroed = false;

    void UpdatePath()
    {
        if (!targetAggroed) return;
        if (seeker.IsDone())
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

    private bool shooting;

    // Update is called once per frame
    void FixedUpdate()
    {
        // regarde si la box du joueur est dans la zone d'aggro
        Vector3 dirTarget = (transform.position - target.position).normalized;
        if (!targetAggroed)
        {
            targetAggroed = target.GetComponent<SpriteRenderer>().bounds.Contains(transform.position - dirTarget * aggroRadius);
        } else if(target.GetComponent<SpriteRenderer>().bounds.Contains(transform.position - dirTarget * shootingRadius))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -dirTarget, shootingRadius, canSee);
            shooting = hit && hit.collider.gameObject == target.gameObject;
            if(shooting)
            {
                animator.SetBool("IsShooting", true);
            }
        }


        if(shooting)
        {
            return;
        }

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

        if(distance < nextWaypointDistance)
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
}
