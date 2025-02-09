using Assets.Helpers;
using UnityEditor;
using UnityEngine;

public class TyphoonCameraSystem : MonoBehaviour
{

    [SerializeField] Transform parentTransform;
    [SerializeField] new Transform transform;
    static TyphoonCameraSystem instance;
    const float ZPos = -8.660254f;
    const float ZPosForTyphoonBossPhase2 = -10.7179676972f;
    float targetY = 4;
    const float targetYLeniencyUp = 3;
    const float targetYLeniencyDown = 1.5f;
    float[] pastXVelocities;
    float zoomOutForTyphoonBossProgress;
    public static float GetZPos()
    {
        return Mathf.LerpUnclamped(ZPos, ZPosForTyphoonBossPhase2, instance.zoomOutForTyphoonBossProgress);
    }
    private void Awake()
    {
        instance = this;
        pastXVelocities = new float[90];
        GameManager.AssignCameraVars(Camera.main);
    }

    void FixedUpdate()
    {
        float xVel = GameManager.PlayerControl.rb.velocity.x;
        if (Mathf.Abs(xVel) > 0.01f)
        {
            for (int i = pastXVelocities.Length - 1; i >= 1; i--)
            {
                pastXVelocities[i] = pastXVelocities[i - 1];
            }
            pastXVelocities[0] = xVel;
        }

        Vector2 cameraPos = transform.position;
        Vector2 playerPos = GameManager.PlayerPosition;
        Vector2 relativePlayerPos = playerPos - cameraPos;

        if (playerPos.y > targetY + targetYLeniencyUp)
        {
            targetY = Helper.Decay(targetY + targetYLeniencyUp, playerPos.y, 20) - targetYLeniencyUp;
        }
        else if (playerPos.y < targetY - targetYLeniencyDown)
        {
            //use max -5f to not let the camera dip into the death zone
            targetY = Helper.Decay(targetY - targetYLeniencyDown, Mathf.Max(-5f,playerPos.y), 20) + targetYLeniencyDown;
        }
        float targetX = 0;
        for (int i = 0; i < pastXVelocities.Length; i++)
        {
            targetX += pastXVelocities[i];
        }
        targetX *= .001f;
        targetX = Mathf.Clamp(targetX, -1, 1);
        targetX = Helper.Decay(targetX, relativePlayerPos.x, 10);
        parentTransform.position = new(targetX + cameraPos.x, targetY, GetZPos());
        transform.localPosition = ScreenShakeManager.GetCameraOffset();
    }
    public static void SetCameraPos(Vector3 pos)
    {
        pos.z = GetZPos();
        instance.targetY = pos.y;
        instance.parentTransform.position = pos;
        instance.transform.position = pos;

    }

    public static void SetTyphoonBossPhase2ZoomOutProgress(float progress)
    {
        instance.zoomOutForTyphoonBossProgress = progress;
    }
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Vector3 drawPos = transform.position;
        drawPos.z = 0;
        Camera mainCam = null;
        if(GameManager.instance != null)
        {
            mainCam = GameManager.CurrentCam;
        }
        if(mainCam == null)
        {
            mainCam = Camera.main;
        }
        Helper.GetCameraViewBoundsAtZ(transform.position, mainCam.fieldOfView, 0f, mainCam.aspect, out float minX, out float maxX, out float minY, out float maxY);
        Vector3 viewSize = new Vector3(maxX - minX, maxY - minY);
        Gizmos.DrawWireCube(drawPos, viewSize);
    }
#endif

}
