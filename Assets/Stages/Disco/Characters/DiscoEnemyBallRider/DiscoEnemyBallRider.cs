using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class DiscoEnemyBallRider : Enemy, IMusicSyncable
{
    public int BeatsPerAction => 1;
    public override int LifeMax => 12;
    [SerializeField] GameObject leftSprite;
    [SerializeField] GameObject rightSprite;
    [SerializeField] Transform leftSpriteTransform;
    [SerializeField] Transform rightSpriteTransform;
    [SerializeField] new Transform transform;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform ballTransform;
    float bounceTimer;
    public override void Start()
    {
        DiscoMusicEventManager.AddSyncableObject(this);
        base.Start();
    }
    public void DoMusicSyncedAction()
    {
        bounceTimer = 0;
        if(leftSprite == null || rightSprite == null || ballTransform == null)
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
        if (Helper.EnemyAggroCheck(pos, playerPos, 6))
        {
            vel.x = Helper.Decay(vel.x, Mathf.Sign(playerPos.x - pos.x) * 4f, 50);
            rb.velocity = vel;
        }
        pos.y += offsetY;
        leftSpriteTransform.position = pos;
        rightSpriteTransform.position = pos;
        ballTransform.Rotate(0, 0, vel.x * -Mathf.Rad2Deg * Time.deltaTime);
    }
    public override bool PreKill()
    {
        EffectsHandler.SpawnMediumExplosion(FlipnoteColors.ColorID.Magenta, leftSpriteTransform.position);
        EffectsHandler.SpawnMediumExplosion(FlipnoteColors.ColorID.Magenta, ballTransform.position);
        leftSprite.SetActive(false);
        rightSprite.SetActive(false);
        ballTransform.gameObject.SetActive(false);
        return base.PreKill();
    }
}
