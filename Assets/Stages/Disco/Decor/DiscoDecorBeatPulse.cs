using Assets.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoDecorBeatPulse : MonoBehaviour, IMusicSyncable
{
    public int BeatsPerAction => 1;
    float pulseTimer;
    new Transform transform;
    void Start()
    {
        transform = base.transform;
        pulseTimer = 0;
        DiscoMusicEventManager.AddSyncableObject(this);
    }

    void Update()
    {
        float pulseDuration = 0.1f;
        float scale = Easings.SqrOut(Mathf.InverseLerp(0, pulseDuration / 2f, pulseTimer) * Mathf.InverseLerp(pulseDuration, pulseDuration / 2f, pulseTimer));
        scale = Mathf.Lerp(1, 1.2f, scale);
        transform.localScale = new Vector3(scale, scale, scale);
        pulseTimer += Time.deltaTime;
    }

    public void DoMusicSyncedAction()
    {
        pulseTimer = 0;
    }
}
