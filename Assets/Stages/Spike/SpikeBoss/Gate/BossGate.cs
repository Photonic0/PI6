using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossGate : MonoBehaviour
{
    [SerializeField] Transform[] gateSegments;
    [SerializeField] float timer;
    [SerializeField] float xDistThresholdForOpening = 3;
    [SerializeField] float animationDuration = .5f;
    [SerializeField] new BoxCollider2D collider;
    [SerializeField] Transform cameraPositionLockPoint;//arena bounds is 16x8
    [SerializeField] Enemy boss;
    [SerializeField] AudioSource source;
    [SerializeField] GameObject[] objsToDestroy;
    [SerializeField] GameObject[] objsToDisable;
    Transform mainCamTransform;
    Transform cameraParentTransform;
    bool locked;
    Vector2 Top { get {
            Vector2 result = transform.position;
            result.y++;
            return result;
        }
    }
    private void Start()
    {
        Camera mainCam = Camera.main;
        mainCamTransform = mainCam.transform;
        cameraParentTransform = mainCamTransform.parent;
        timer = 0;
    }
    void Update()
    {

        if (collider != null && !locked && GameManager.PlayerPosition.x - Top.x > .5f)
        {
            Lock();
        }
        if (locked && cameraPositionLockPoint != null)
        {
            if (CameraRailSystem.instance != null)
            {
                CameraRailSystem.instance.enabled = false;
                Vector3 newCameraPos = cameraParentTransform.position;
                float cameraZ = newCameraPos.z;
                newCameraPos = Vector2.MoveTowards(newCameraPos, cameraPositionLockPoint.position, Time.deltaTime * 20);
                newCameraPos.z = cameraZ;
                cameraParentTransform.position = newCameraPos;
            }
            else
            {
                Vector3 newCameraPos = cameraParentTransform.position;
                float cameraZ = TyphoonCameraSystem.GetZPos();
                newCameraPos = Helper.Decay(newCameraPos, cameraPositionLockPoint.position, 15);
                newCameraPos.z = cameraZ;
                cameraParentTransform.position = newCameraPos;
                mainCamTransform.GetComponent<TyphoonCameraSystem>().enabled = false;
            }
            mainCamTransform.localPosition = ScreenShakeManager.GetCameraOffset();
        }
        float oldTimer = timer;
        if (!locked && Mathf.Abs(Top.x - GameManager.PlayerPosition.x) < xDistThresholdForOpening)
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
        Vector2 top = Top;
        for (int i = 0; i < gateSegments.Length; i++)
        {
            Transform gateSegment = gateSegments[i];
            //make all of them have the same offset but clamp the y of the earlier ones
            float yOffset = Helper.Remap(timer, 0, animationDuration, 3, 0);
            if (yOffset > i + 1)
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
        if (boss is SpikeBossAI spikeBoss)
        {
            spikeBoss.ChangeToIntro();
            MusicManager.StartBossMusic();
        }
        else if (boss is TyphoonBossAI typhoonBoss)
        {
            if (SceneManager.GetActiveScene().buildIndex == SceneIndices.TyphoonStage)
            {
                //disable cloud particle spawning during bossfight (wont be visible)
                //also handles spawning lightning nodes
                FindObjectOfType<TyphoonTilesCloudsEffectSpawner>().enabled = false;
                FindObjectOfType<TyphoonCameraSystem>().enabled = false;
            }
            typhoonBoss.ChangeToIntro();
            MusicManager.StartBossMusic();
            
        }
        else if (boss is DiscoBossAI discoBoss)
        {
            discoBoss.ChangeToIntro();
            DiscoMusicEventManager.Disable();
        }
        DestroyObjsOnLock();
        DisableObjsOnLock();
    }

    private void DisableObjsOnLock()
    {
        if (objsToDisable == null || objsToDisable.Length <= 0)
            return;
        for (int i = 0; i < objsToDisable.Length; i++)
        {
            if (objsToDisable[i] == null)//null comparison to check for destroyed game object
            {
                objsToDisable[i] = null;//null assignment to remove reference from the array
                continue;//continue so we don't try to disable null ref
            }
            objsToDisable[i].SetActive(false);
        }
    }

    private void DestroyObjsOnLock()
    {
        if (objsToDestroy == null || objsToDestroy.Length <= 0)
            return;
        for (int i = 0; i < objsToDestroy.Length; i++)
        {
            if (objsToDestroy[i] == null)//null comparison to check for destroyed game object
            {
                objsToDestroy[i] = null;//null assignment to remove reference from the array
                continue;//continue so we don't try to destroy null ref
            }
            Destroy(objsToDestroy[i]);
        }
    }
}
