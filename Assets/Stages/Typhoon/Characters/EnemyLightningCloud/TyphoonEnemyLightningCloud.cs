using Assets.Common.Characters.Main.Scripts;
using Assets.Helpers;
using UnityEngine;

public class TyphoonEnemyLightningCloud : Enemy
{

    public override int LifeMax => 9;
    public const int StateIDIdle = 0;
    public const int StateIDChasingPlayer = 1;
    public const int StateIDAttacking = 2;
    public const float AggroRange = 6;
    public const float MaxMoveSpeed = 5;
    public const float MinDistForStartAttack = 2;
    public const float AttackTelegraphDuration = 0.8f;
    public const float AttackDuration = 1;
    [SerializeField] LayerMask tilesLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform parent;
    [SerializeField] ParticleSystem deathParticle;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] LineRenderer lightningBoltRenderer;
    Vector2 lightningOrigin;
    Vector2 lightningEnd;
    float timer;
    int state;
    Vector2 lightningEndPoint;
    Vector2 attackDirection;
    private void Update()
    {
        switch (state)
        {
            case StateIDIdle:
                State_Idle();
                break;
            case StateIDChasingPlayer:
                State_ChasingPlayer();
                break;
            case StateIDAttacking:
                State_Attacking();
                break;
        }
        timer += Time.deltaTime;
    }


    private void State_Idle()
    {
        rb.velocity = new Vector2(0, Mathf.Cos(timer));
        if (Helper.EnemyAggroCheck(transform.position, GameManager.PlayerControl.Position, AggroRange))
        {
            state = StateIDChasingPlayer;
            timer = -Time.deltaTime;
        }
    }

    private void State_ChasingPlayer()
    {
        Vector2 playerPos = GameManager.PlayerControl.Position;
        Vector2 toTargetPos = (playerPos + new Vector2(0, 2.5f) - rb.position).normalized;
        rb.velocity = Vector2.Lerp(rb.velocity, toTargetPos * MaxMoveSpeed, Time.deltaTime * 5);
        if (playerPos.y < transform.position.y && Mathf.Abs(playerPos.x - transform.position.x) < MinDistForStartAttack)
        {
            state = StateIDAttacking;
            timer = -Time.deltaTime;
            attackDirection = (playerPos - (Vector2)transform.position).normalized;
        }
    }

    private void State_Attacking()
    {
        Vector2 vel = rb.velocity;
        vel.x = Helper.Decay(vel.x, 0, 20);
        vel.y = Helper.Decay(vel.y, 0, 20);
        rb.velocity = vel;
        if (timer < AttackTelegraphDuration)
        {
            RaycastHit2D hit = Physics2D.Raycast(rb.position, attackDirection, 20, tilesLayer);
            Vector2 start = transform.position + new Vector3(0, -0.3f);
            Vector2 end = hit.point;
            //add telegraph later
        }
        else if (timer < AttackDuration + AttackTelegraphDuration)
        {
            lightningBoltRenderer.enabled = true;
            RaycastHit2D hit = Physics2D.Raycast(rb.position, attackDirection, 20, tilesLayer);
            Vector2 start = transform.position + new Vector3(0, -0.3f);
            Vector2 end = hit.point;
            LightningVisual(start, end);
            lightningOrigin = start;
            lightningEnd = end;
            Collider2D playerCollider = Physics2D.OverlapBox((start + end) / 2, new Vector2(0.1f, (start - end).magnitude), attackDirection.Atan2Deg(), playerLayer);
            if (playerCollider != null)
            {
                if (playerCollider.TryGetComponent(out PlayerLife player))
                {
                    player.Damage(4);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log("lightning collision wasn't player?? was " + playerCollider.name + "instead");
#endif
                }
            }
        }
        else
        {
            timer = 0;
            lightningBoltRenderer.enabled = false;
            state = StateIDIdle;
        }
    }
    void LightningVisual(Vector2 start, Vector2 end)
    {
        Vector2 deltaPos = end - start;
        int pointsAmount = (int)Mathf.Max(deltaPos.magnitude, 2) + 2;
        Vector2 direction = deltaPos.normalized;
        Vector2 normal = new(-direction.y, direction.x);
        //confusing name
        float[] pointsTs = new float[pointsAmount];
        pointsTs[0] = 0;
        pointsTs[^1] = 1;
        for (int i = 1; i < pointsAmount - 1; i++)
        {
            pointsTs[i] = Random.value;
        }
        System.Array.Sort(pointsTs);
        Vector3[] positions = new Vector3[pointsAmount];
        float prevDeviation = 0;//so the first one is not near the middle
        for (int i = 0; i < pointsAmount; i++)
        {
            float t = pointsTs[i];
            Vector2 position = Vector2.Lerp(start, end, t);
            float dist = Mathf.Min((end - position).magnitude, (start - position).magnitude);
            float deviationMult = Random2.FloatWithExcludedRange(-.8f, .8f, prevDeviation - .4f, prevDeviation + .4f);
            prevDeviation = deviationMult;
            deviationMult *= Mathf.InverseLerp(0, 1, dist);
            position += normal * deviationMult;
            positions[i] = position;
        }
        if (pointsAmount != lightningBoltRenderer.positionCount)
        {
            lightningBoltRenderer.positionCount = pointsAmount;
        }
        lightningBoltRenderer.SetPositions(positions);
    }
    public override void OnHit(int damageTaken)
    {
        if (life <= 0)
        {
            deathParticle.transform.position = transform.position;
            deathParticle.Emit(50);
            Destroy(parent.gameObject, 1);
        }
    }
    //[SerializeField] float min;
    //[SerializeField] float max;
    //[SerializeField] float excludedRangeMin;
    //[SerializeField] float excludedRangeMax;


    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(lightningOrigin, .1f);
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(lightningEnd, .1f);
    //    Gizmos.DrawSphere(Vector2.up * .2f + Helper.MouseWorld + new Vector2(min, 0), .1f);
    //    Gizmos.DrawSphere(Vector2.up * .2f + Helper.MouseWorld + new Vector2(max, 0), .1f);
    //    Gizmos.DrawSphere(Vector2.up * .2f + Helper.MouseWorld + new Vector2(excludedRangeMin, 0), .1f);
    //    Gizmos.DrawSphere(Vector2.up * .2f + Helper.MouseWorld + new Vector2(excludedRangeMax, 0), .1f);
    //    for (int i = 0; i < 100; i++)
    //    {
    //        Gizmos.DrawSphere(new Vector2(Random2.FloatWithExcludedRange(min, max, excludedRangeMin, excludedRangeMax), 0) + Helper.MouseWorld, 0.02f);
    //    }
    //}
}
