using Assets.Common.Consts;
using UnityEngine;

public class DiscoHazardSpotlightLightCone : MonoBehaviour, IMusicSyncable
{
    [SerializeField] new PolygonCollider2D collider;
    [SerializeField] SpriteRenderer spriterenderer;

    private void Start()
    {
        DiscoMusicEventManager.AddSyncableObject(this);
    }
    public int BeatsPerAction => 4;
    public int BeatOffset => 3;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(4);
        }
    }

    public void DoMusicSyncedAction()
    {
        collider.enabled = !collider.enabled;
        spriterenderer.enabled = collider.enabled;
    }
}
