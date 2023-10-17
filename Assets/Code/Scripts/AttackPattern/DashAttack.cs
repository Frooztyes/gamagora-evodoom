using System.Collections;
using UnityEngine;

public class DashAttack : AttackPattern
{
    [SerializeField] private float initalAngle;

    private Transform player;
    bool isRunning = false;
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        hasFinished = true;
    }

    private Vector3 GetTargetPosition()
    {
        return player.transform.position;
    }

    private IEnumerator JumpToTarget()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = GetTargetPosition();
        Vector3 handlePosition = Vector3.Lerp(startPosition, targetPosition, 0.5f);
        handlePosition.y += 5;

        float distance = (startPosition - targetPosition).magnitude;
        float movementSpeed = 20;
        float duration = distance / movementSpeed;

        for (float f = 0; f < 1; f += Time.deltaTime / duration)
        {
            transform.position = Vector3.Lerp(
                Vector3.Lerp(
                    startPosition,
                    handlePosition,
                    f),
                Vector3.Lerp(
                    handlePosition,
                    targetPosition,
                    f),
                f);

            yield return null;
        }
        isRunning = false;
    }

    public void SetTrajectory(Vector2 target, float initialAngle)
    {
        animator.SetInteger("AttackAnimation", 1);
        Vector3 p = target;

        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = initialAngle * Mathf.Deg2Rad;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = 0;

        float initialVelocity = 1 / Mathf.Cos(angle) * Mathf.Sqrt(0.5f * gravity * Mathf.Pow(distance, 2) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > transform.position.x ? 1 : -1); ;
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        // Fire!
        finalVelocity.x *= rb.gravityScale * 2 * 0.3f;
        finalVelocity.y *= rb.gravityScale;
        rb.velocity = finalVelocity;
    }

    private bool SpriteCollides(GameObject go)
    {
        SpriteRenderer r1 = go.GetComponent<SpriteRenderer>();
        SpriteRenderer r2 = GetComponent<SpriteRenderer>();

        Vector2 r1Size = r1.bounds.size;
        Vector2 r2Size = r2.bounds.size;

        Rect rec1 = new Rect(go.transform.position.x - (r1Size.x / 2), go.transform.position.y - (r1Size.y/2), r1Size.x, r1Size.y);
        Rect rec2 = new Rect(transform.position.x - (r2Size.x / 2), transform.position.y, r2Size.x, r2Size.y);


        Debug.DrawLine(new Vector3(rec1.x, rec1.y), new Vector3(rec1.x + rec1.width, rec1.y), Color.green);
        Debug.DrawLine(new Vector3(rec1.x, rec1.y), new Vector3(rec1.x, rec1.y + rec1.height), Color.red);
        Debug.DrawLine(new Vector3(rec1.x + rec1.width, rec1.y + rec1.height), new Vector3(rec1.x + rec1.width, rec1.y), Color.green);
        Debug.DrawLine(new Vector3(rec1.x + rec1.width, rec1.y + rec1.height), new Vector3(rec1.x, rec1.y + rec1.height), Color.red);

        Debug.DrawLine(new Vector3(rec2.x, rec2.y), new Vector3(rec2.x + rec2.width, rec2.y), Color.blue);
        Debug.DrawLine(new Vector3(rec2.x, rec2.y), new Vector3(rec2.x, rec2.y + rec2.height), Color.yellow);
        Debug.DrawLine(new Vector3(rec2.x + rec2.width, rec2.y + rec2.height), new Vector3(rec2.x + rec2.width, rec2.y), Color.blue);
        Debug.DrawLine(new Vector3(rec2.x + rec2.width, rec2.y + rec2.height), new Vector3(rec2.x, rec2.y + rec2.height), Color.yellow);

        return rec1.Overlaps(rec2);
    }

    private bool isReloading = true;
    // Update is called once per frame
    void Update()
    {
        if(SpriteCollides(player.gameObject))
        {
            player.GetComponent<MyCharacterController>().TakeDamage(player.transform.position.x < transform.position.x);
            animator.SetInteger("AttackAnimation", 2);
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("PowerImp_attack2")
                && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1
                && !animator.IsInTransition(0))
            {
                animator.SetInteger("AttackAnimation", 3);
                hasFinished = true;
                isReloading = true;
                Invoke(nameof(Reload), 1f);
            }
            return;
        }

        if (isReloading) return;

        if (Mathf.Abs(rb.velocity.y) > 0)
        {
            return;
        }

        animator.SetInteger("AttackAnimation", 2);
        if (Vector2.Distance(player.position, transform.position) > 3)
        {
            hasFinished = false;
            SetTrajectory(player.position, 40);
        }

        Debug.Log(player.GetComponent<SpriteRenderer>().bounds.Contains(transform.position));



        //if(isRunning)
        //{
        //    HasFinished = false;
        //}
        //if(!isRunning && !HasFinished)
        //{
        //    animator.SetInteger("AttackAnimation", 2);
        //    
        //}
    }

    private void Reload()
    {
        isReloading = false;
    }
    public bool IsReloading() { return isReloading; }


    bool AnimatorIsPlaying()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length >
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    Animator animator;

    public void SetInformations(Transform target, Animator anim, Rigidbody2D rb)
    {
        player = target;
        animator = anim;
        this.rb = rb; 
        hasFinished = false;
    }

    public override void StartAttack()
    {
        hasFinished = false;
        isRunning = true;
        animator.SetInteger("AttackAnimation", 1);
        SetTrajectory(player.position, 30);
    }

    public override void StopAttack()
    {
    }

    public override bool IsOver()
    {
        return hasFinished;
    }
}
