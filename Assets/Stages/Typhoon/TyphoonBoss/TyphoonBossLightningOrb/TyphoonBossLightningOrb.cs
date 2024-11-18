using Assets.Common.Consts;
using UnityEngine;

public class TyphoonBossLightningOrb : MonoBehaviour
{
    public new Transform transform;
    public new GameObject gameObject;
    public SpriteRenderer sprite;
    public new CircleCollider2D collider;
    public float timeLeftUntilColliderActivates;
    private void FixedUpdate()
    {
        timeLeftUntilColliderActivates -= Time.fixedDeltaTime;
        if(timeLeftUntilColliderActivates < 0 && timeLeftUntilColliderActivates > -2)
        {
            collider.enabled = true;
        }
        if(timeLeftUntilColliderActivates > 0)
        {
            sprite.color = Color.Lerp(Color.white, Color.clear, timeLeftUntilColliderActivates - .1f);
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
