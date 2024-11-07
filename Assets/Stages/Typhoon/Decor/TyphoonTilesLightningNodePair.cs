using Assets.Common.Effects.Lightning;
using UnityEngine;

public class TyphoonTilesLightningNodePair : MonoBehaviour
{
    public static int RandAnimHash => animHashes[Random.Range(0, animHashes.Length)];
    public static int[] animHashes = new int[] { Animator.StringToHash("Anim0"), Animator.StringToHash("Anim1"), Animator.StringToHash("Anim2"), Animator.StringToHash("Anim3") };
    public Transform transform1;
    public GameObject gameObject1;
    public SpriteRenderer sprite1;
    public Transform transform2;
    public GameObject gameObject2;
    public SpriteRenderer sprite2;
    public SimpleLightningRenderer lightningRenderer;
    public Animator animator1;
    public Animator animator2;
    public float timeLeft = 0;
    void Update()
    {
        timeLeft -= Time.deltaTime;
        //sprite will fade out over the course of the last 1/5th of a second
        Color color = new Color(1, 1, 1, timeLeft * 5);
        sprite1.color = color;
        sprite2.color = color;
        if (timeLeft < 0)
        {
            timeLeft = 0;
            gameObject1.SetActive(false);
            gameObject2.SetActive(false);
        }
    }

}
