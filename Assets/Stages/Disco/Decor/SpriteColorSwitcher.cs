using Assets.Common.Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorSwitcher : MonoBehaviour, IMusicSyncable
{
    byte colorID;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] DiscoMusicEventManager.SyncableObjAddFlags syncableObjAddFlags = DiscoMusicEventManager.SyncableObjAddFlags.LevelOnly;
    public int BeatsPerAction => 1;
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
            sprite.color = FlipnoteColors.Magenta;
        }
        else
        {
            sprite.color = FlipnoteColors.Yellow;
        }
    }
    public void SwitchColor()
    {
        colorID += 1;
        colorID %= 2;
        if (colorID == 0)
        {
            sprite.color = FlipnoteColors.Magenta;
        }
        else
        {
            sprite.color = FlipnoteColors.Yellow;
        }
    }
}
