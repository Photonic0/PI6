using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Helpers;
using Assets.Systems;
using UnityEngine;

public class OneUp : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] SpriteRenderer sprite;
    bool collected;
    float timer;
    const float CollectAnimDuration = .25f;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.Player))
        {
            audioSource.Play();
            PlayerLife.chances++;
            GetComponent<BoxCollider2D>().enabled = false;
            sprite.sortingOrder = 20;
            collected = true;
            GameManager.UpdatePermanentlyDestroyedObjFlagsAndDestroyObj(gameObject, 4f);
        }
    }
    private void Update()
    {
        if (collected)
        {
            timer += Time.deltaTime;
            float scale = Helper.Remap(timer, 0f, CollectAnimDuration, 1f, 2f, Easings.SqrIn);
            float opacity = Mathf.InverseLerp(CollectAnimDuration, 0f, timer);
            Color color = sprite.color;
            color.a = opacity;
            sprite.color = color;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
