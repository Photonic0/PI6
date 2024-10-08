using Assets.Helpers;
using UnityEngine;

public class BossGate : MonoBehaviour
{
    [SerializeField] Transform[] gateSegments;
    [SerializeField] float timer;
    [SerializeField] float xDistThresholdForOpening = 3;
    [SerializeField] float animationDuration = .5f;
    [SerializeField] new BoxCollider2D collider;
    [SerializeField] Transform cameraPositionLockPoint;
    [SerializeField] Enemy boss;
    [SerializeField] AudioSource source;
    Transform cameraParentTransform;
    bool locked;
    Vector2 top;
    private void Start()
    {
        cameraParentTransform = Camera.main.GetComponentInParent<Transform>();
        timer = 0;
        top = transform.position;
        //add 1 so the the top is inside the block above the gate
        top.y += 1;
    }
    void Update()
    {
        
        if(collider != null && !locked && GameManager.PlayerPosition.x - top.x > .5f)
        {
            Lock();
        }
        if (locked && cameraPositionLockPoint != null)
        {
            CameraRailSystem.instance.enabled = false;
            Vector3 newCameraPos = cameraParentTransform.position;
            float cameraZ = newCameraPos.z;
            newCameraPos = Vector2.MoveTowards(newCameraPos, cameraPositionLockPoint.position, Time.deltaTime * 8);
            newCameraPos.z = cameraZ;
            cameraParentTransform.position = newCameraPos;
        }
        float oldTimer = timer;
        if (!locked && Mathf.Abs(top.x - GameManager.PlayerPosition.x) < xDistThresholdForOpening)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer -= Time.deltaTime;
        }
        timer = Mathf.Clamp(timer, 0, animationDuration);
        if (timer != oldTimer && (oldTimer == 0 || oldTimer == animationDuration))
        {
            CommonSounds.PlayGateOpen(source);
        }
        for (int i = 0; i < gateSegments.Length; i++)
        {
            Transform gateSegment = gateSegments[i];
            //make all of them have the same offset but clamp the y of the earlier ones
            float yOffset = Helper.Remap(timer, 0, animationDuration, 3, 0);
            if(yOffset > i + 1)
            {
                yOffset = i + 1;
            }
            gateSegment.position = new Vector3(top.x, top.y - yOffset);
        }
    }
    public void Lock()
    {
        if (collider != null)
        {
            collider.enabled = true;
        }
        locked = true;
        if(boss is SpikeBossAI spikeBoss)
        {
            spikeBoss.ChangeToIntro();
        }
    }
}
