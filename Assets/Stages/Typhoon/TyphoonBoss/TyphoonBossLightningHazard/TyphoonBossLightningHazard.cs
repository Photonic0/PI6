using Assets.Common.Consts;
using Assets.Common.Effects.Lightning;
using Assets.Helpers;
using UnityEditor;
using UnityEngine;

public class TyphoonBossLightningHazard : MonoBehaviour
{
    float telegraphDuration = 2;
    float hitboxDuration = 1;

    [SerializeField] SimpleLightningRenderer lightningRenderer;
    [SerializeField] Transform node1transform;
    [SerializeField] Transform node2transform;
    [SerializeField] Animator node1Animator;
    [SerializeField] Animator node2Animator;
    [SerializeField] new Transform transform;
    [SerializeField] new BoxCollider2D collider;
    [SerializeField] ParticleSystem telegraphParticles;
    const float LightningVisualDuration = .2f;
    float timer;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer < telegraphDuration)
        {
            collider.enabled = false;
            if (telegraphParticles != null)
            {
                if (timer < telegraphDuration - .2f)
                {
                    float chancePer45thSec = .5f;
                    Vector2 node1 = node1transform.localPosition;
                    Vector2 node2 = node2transform.localPosition;
                    float timeLeftUntilActivation = telegraphDuration - timer + .2f;
                    float sizeIncrease = Helper.Remap(timer, 0f, telegraphDuration - .2f, 0, 0.25f);
                    if (Random2.Percent(chancePer45thSec, 45))
                    {
                        ParticleSystem.EmitParams emitParams = new();
                        Vector2 deltaPos = node2 - node1;
                        float lifetime = Random2.Float(.2f, .45f);
                        emitParams.position = node1 + Random2.Circular(0.1f);
                        emitParams.velocity = deltaPos / (1.7f * lifetime) + Random2.Circular(1);
                        emitParams.startLifetime = Mathf.Min(lifetime, timeLeftUntilActivation);
                        emitParams.startColor = FlipnoteStudioColors.Yellow;
                        emitParams.startSize = Random2.Float(.01f, .11f) + sizeIncrease;
                        telegraphParticles.Emit(emitParams, 1);
                    }
                    if (Random2.Percent(chancePer45thSec, 45))
                    {
                        ParticleSystem.EmitParams emitParams = new();
                        Vector2 deltaPos = node1 - node2;
                        float lifetime = Random2.Float(.2f, .45f);
                        emitParams.position = node2 + Random2.Circular(0.1f);
                        emitParams.velocity = deltaPos / (1.7f * lifetime) + Random2.Circular(1);
                        emitParams.startLifetime = Mathf.Min(lifetime, timeLeftUntilActivation);
                        emitParams.startColor = FlipnoteStudioColors.Yellow;
                        emitParams.startSize = Random2.Float(.01f, .11f) + sizeIncrease;

                        telegraphParticles.Emit(emitParams, 1);
                    }
                }
            }
        }
        else if (timer < telegraphDuration + hitboxDuration)
        {
            if (!collider.enabled)
            {
                lightningRenderer.ActivateAndSetAttributes(0.1f, node1transform.position, node2transform.position, LightningVisualDuration, 0.4f);
            }
            collider.enabled = true;
        }
        else
        {
            if (timer > telegraphDuration + LightningVisualDuration)
            {
                gameObject.SetActive(false);
                lightningRenderer.Stop();
            }
            collider.enabled = false;
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(4);
        }
    }
    public void StartTelegraphAndThenActivate(float telegraphDuration, float hitboxDuration)
    {
        node1Animator.CrossFade(TyphoonTilesLightningNodePair.RandAnimHash, 0);
        node2Animator.CrossFade(TyphoonTilesLightningNodePair.RandAnimHash, 0);
        timer = 0;
        collider.enabled = false;
        this.hitboxDuration = hitboxDuration;
        this.telegraphDuration = telegraphDuration;
    }
    public void RaycastPositionsAndAdjustPositionAndRotation(Vector2 raycastCenter, float rotationRadians, out Vector2 node1AttachedSurfaceNormal, out Vector2 node2AttachedSurfaceNormal)
    {
        Vector2 raycastDirection = rotationRadians.PolarVector();
        RaycastHit2D hit1 = Physics2D.Raycast(raycastCenter, raycastDirection, 10f, Layers.Tiles);
        node1AttachedSurfaceNormal = hit1.normal;
        Vector2 node1Pos = hit1.point;
        node1Pos -= node1AttachedSurfaceNormal * .2f;//indent it a bit inside the tiles
        RaycastHit2D hit2 = Physics2D.Raycast(raycastCenter, -raycastDirection, 10f, Layers.Tiles);
        node2AttachedSurfaceNormal = hit2.normal;
        Vector2 node2Pos = hit2.point;
        node2Pos -= node2AttachedSurfaceNormal * .2f;//indent it a bit inside the tiles
        transform.SetPositionAndRotation((node1Pos + node2Pos) / 2f, (node1Pos - node2Pos).ToRotation());
        node1transform.SetPositionAndRotation(node1Pos, Quaternion.Euler(0f, 0f, rotationRadians * Mathf.Rad2Deg + 45 + 180));
        node2transform.SetPositionAndRotation(node2Pos, Quaternion.Euler(0f, 0f, rotationRadians * Mathf.Rad2Deg + 45));
       
    }

    public void GetNodeDirections(out Vector2 node1Dir, out Vector2 node2Dir)
    {
        node1Dir = ((node1transform.rotation.eulerAngles.z - 45) * Mathf.Deg2Rad).PolarVector();
        node2Dir = ((node2transform.rotation.eulerAngles.z - 45) * Mathf.Deg2Rad).PolarVector();
    }
    public void LimitAngle(Vector2 angleCenterNode1, Vector2 angleCenterNode2, float angleRangeRadians)
    {
        GetNodeDirections(out Vector2 node1Dir, out Vector2 node2Dir);
        node1transform.rotation = LimitAngle(angleRangeRadians, angleCenterNode1, node1Dir).ToRotation(45);
        node2transform.rotation = LimitAngle(angleRangeRadians, angleCenterNode2, node2Dir).ToRotation(45);
    }
    static Vector2 LimitAngle(float angleRangeRadians, Vector2 angleCenter, Vector2 angleToLimit)
    {
        float dot = Vector2.Dot(angleCenter, angleToLimit);
        float dotNormal = Vector2.Dot(angleToLimit, new Vector2(-angleCenter.y, angleCenter.x));
        float dotThreshold = Mathf.Cos(angleRangeRadians * .5f);

        if (dot >= dotThreshold)
        {
            return angleToLimit;
        }
        if (dotNormal < 0)
        {
            return angleCenter.RotatedBy(-angleRangeRadians * .5f);
        }
        return angleCenter.RotatedBy(angleRangeRadians * .5f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float rotation = transform.rotation.eulerAngles.z;
        GetNodeDirections(out Vector2 node1Dir, out Vector2 node2Dir);
        Gizmos2.DrawRotatedRectangle(collider.offset.RotatedBy(rotation * Mathf.Deg2Rad) + (Vector2)transform.position, collider.size, rotation, Handles.UIColliderHandleColor);
        Gizmos.color = Color.red;
        Handles.Label(node1transform.position, node1transform.position.ToString());
        Gizmos2.DrawArrow(node1transform.position, node1Dir);
        Gizmos.color = Color.blue;
        Handles.Label(node2transform.position, node1transform.position.ToString());
        Gizmos2.DrawArrow(node2transform.position, node2Dir);
    }
#endif
}
