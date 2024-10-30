using Assets.Helpers;
using UnityEngine;


public class TyphoonTilesCloudParticle : MonoBehaviour
{
    public new GameObject gameObject;
    public SpriteRenderer sprite;
    public new Transform transform;
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
        float opacity = Mathf.InverseLerp(0, 1, timer) * Mathf.InverseLerp(4, 3, timer);
        spriteCol.a = opacity;
        sprite.color = spriteCol;
        if(timer >= 4)
        {
            timer = 0;
            gameObject.SetActive(false);
        } 
    }
}

