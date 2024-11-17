using Assets.Common.Consts;
using UnityEngine;

public class TyphoonBossLightningOrb : MonoBehaviour
{
    public new Transform transform;
    public new GameObject gameObject;
    public SpriteRenderer sprite;
    public new CircleCollider2D collider;
    public float timeLeftUntilColliderActivates;
    public TyphoonBossAI boss;
    public int index;
    private void FixedUpdate()
    {
        timeLeftUntilColliderActivates -= Time.fixedDeltaTime;
        if(timeLeftUntilColliderActivates < 0)
        {
            collider.enabled = true;
        }
        if(timeLeftUntilColliderActivates > 0)
        {
            sprite.color = Color.Lerp(Color.white, Color.clear, timeLeftUntilColliderActivates - .1f);
        }
    }
    private void Update()
    {
        if(boss != null)
        {
            Vector2 pos = boss.transform.position;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(4);
        }   
    }
}
