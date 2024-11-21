using Assets.Systems;
using UnityEngine;

public class DiscoDecorBeatPulse : MonoBehaviour, IMusicSyncable
{
    public int BeatsPerAction => 1;
    float pulseTimer;
    new Transform transform;
    [SerializeField] float scaleMultiplier = 1;
    [SerializeField] DiscoMusicEventManager.SyncableObjAddFlags syncableAddFlags = DiscoMusicEventManager.SyncableObjAddFlags.LevelOnly;
    void Start()
    {
        transform = base.transform;
        pulseTimer = 0;
        DiscoMusicEventManager.AddSyncableObject(this, syncableAddFlags);
    }

    void Update()
    {
        float pulseDuration = 0.1f;
        float scale = Easings.SqrOut(Mathf.InverseLerp(0, pulseDuration / 2f, pulseTimer) * Mathf.InverseLerp(pulseDuration, pulseDuration / 2f, pulseTimer));
        scale = Mathf.Lerp(1, 1.2f, scale);
        scale *= scaleMultiplier;
        transform.localScale = new Vector3(scale, scale, scale);
        pulseTimer += Time.deltaTime;
    }

    public void DoMusicSyncedAction()
    {
        pulseTimer = 0;
    }
}
