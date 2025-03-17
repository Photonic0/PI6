using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class DiscoEnemyBallRider : Enemy, IMusicSyncable, IMusicSyncableWithoutSlightDelay
{
    public int BeatsPerAction => 1;
    public int BeatOffset => 0;

    public override int LifeMax => 9;
    [SerializeField] GameObject leftSprite;
    [SerializeField] GameObject rightSprite;
    [SerializeField] Transform leftSpriteTransform;
    [SerializeField] Transform rightSpriteTransform;
    [SerializeField] new Transform transform;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform ballTransform;
    [SerializeField] AudioSource audioSource;
    [SerializeField] DiscoEnemyBallRiderBall ball;
    float bounceTimer;
    public override void Start()
    {
        DiscoMusicEventManager.AddSyncableObject(this);
        DiscoMusicEventManager.AddSyncableObjectWithoutSlightDelay(this);
        base.Start();
    }
    public void DoMusicSyncedActionWithoutDelay()
    {
        if (life <= 0 && enabled)
        {
            CommonSounds.PlayBwow(audioSource);
            EffectsHandler.SpawnMediumExplosion(FlipnoteStudioColors.ColorID.Magenta, leftSpriteTransform.position);
            EffectsHandler.SpawnMediumExplosion(FlipnoteStudioColors.ColorID.Magenta, ballTransform.position);
            leftSprite.SetActive(false);
            rightSprite.SetActive(false);
            OnDeathEvents();
            enabled = false;
            return;
        }
    }
    public void DoMusicSyncedAction()
    {
        if (!enabled)
        {
            return;
        }
        bounceTimer = 0;
        if (leftSprite == null || rightSprite == null || ballTransform == null)
        {
            return;
        }
        bool leftActive = leftSprite.activeInHierarchy;
        bool rightActive = rightSprite.activeInHierarchy;
        if (leftActive || rightActive)
        {
            leftSprite.SetActive(!leftActive);
            rightSprite.SetActive(!rightActive);
        }
    }
    private void FixedUpdate()
    {
        Vector2 point = transform.position;
        point.y -= 0.6338626f;
        Vector2 size = new(0.9321166f, 3.267649f);
        Collider2D collider = Physics2D.OverlapCapsule(point, size, CapsuleDirection2D.Vertical, 0, Layers.PlayerHurtbox);
        if (collider != null && collider.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(4);
        }
    }
    private void Update()
    {
        bounceTimer += Time.deltaTime;
        float offsetY = (2f * bounceTimer) / (float)DiscoMusicEventManager.SecondsPerBeat - 1;
        offsetY = Mathf.Max(0, 1 - (offsetY * offsetY)) * .5f;
        offsetY -= 0.3f;

        Vector3 playerPos = GameManager.PlayerPosition;
        Vector3 pos = transform.position;
        Vector2 vel = rb.velocity;
        float horizontalAggroRange = Mathf.Abs(rb.velocity.x * 2) + 7;
        if (Helper.EnemyAggroCheck(pos, playerPos, horizontalAggroRange, 3))
        {
            vel.x = Helper.Decay(vel.x, Mathf.Sign(playerPos.x - pos.x) * 4.5f, 50);
            rb.velocity = vel;
        }
        pos.y += offsetY;
        leftSpriteTransform.position = pos;
        rightSpriteTransform.position = pos;
        ballTransform.Rotate(0, 0, vel.x * -Mathf.Rad2Deg * Time.deltaTime);
    }
    public override void OnHit(int damageTaken)
    {
        DiscoStageSingleton.PlayBwowLite(audioSource);
    }
    public override bool PreKill()
    {
        ball.Release(rb.velocity);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        return false;
    }
    private void OnDestroy()
    {
        DiscoMusicEventManager.RemoveLevelSyncableObject(this);
    }
}
