using Assets.Helpers;
using UnityEngine;


public class TyphoonBackgroundCloudParticle : MonoBehaviour
{
    public new GameObject gameObject;
    public SpriteRenderer sprite;
    public new Transform transform;
    public const float ParticleDuration = 4;
    public Vector3 velocity;
    float timer;
    [SerializeField] bool forceFaceDirection;
    public void SetFlipX()
    {
        if (forceFaceDirection)
        {
            sprite.flipX = velocity.x < 0;
        }
        else
        {
            sprite.flipX = Random2.Bool;
        }
    }
    private void Update()
    {
        timer += Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        Color spriteCol = sprite.color;
        float opacity = Mathf.InverseLerp(0, 1, timer) * Mathf.InverseLerp(ParticleDuration, ParticleDuration - 1, timer);
        spriteCol.a = opacity;
        sprite.color = spriteCol;
        if (timer >= ParticleDuration)
        {
            timer = 0;
            gameObject.SetActive(false);
        }
    }
}

