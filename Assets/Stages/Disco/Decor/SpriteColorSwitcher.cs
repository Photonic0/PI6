using Assets.Common.Consts;
using UnityEngine;

public class SpriteColorSwitcher : MonoBehaviour, IMusicSyncable
{
    byte colorID;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] DiscoMusicEventManager.SyncableObjAddFlags syncableObjAddFlags = DiscoMusicEventManager.SyncableObjAddFlags.LevelOnly;
    public int BeatsPerAction => 1;
    public int BeatOffset => 0;

    private void Start()
    {
        DiscoMusicEventManager.AddSyncableObject(this, syncableObjAddFlags);
        Vector3 pos = transform.position;
        colorID = (byte)(((int)pos.x + (int)pos.y) % 2);
    }
    public void DoMusicSyncedAction()
    {
        colorID += 1;
        colorID %= 2;
        if (colorID == 0)
        {
            sprite.color = FlipnoteStudioColors.Magenta;
        }
        else
        {
            sprite.color = FlipnoteStudioColors.Yellow;
        }
    }
    public void SwitchColor()
    {
        colorID += 1;
        colorID %= 2;
        if (colorID == 0)
        {
            sprite.color = FlipnoteStudioColors.Magenta;
        }
        else
        {
            sprite.color = FlipnoteStudioColors.Yellow;
        }
    }
    private void OnDestroy()
    {
        DiscoMusicEventManager.RemoveLevelSyncableObject(this);
    }
}
