using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Common.Effects.Lightning;
using Assets.Helpers;
using UnityEditor;
using UnityEngine;

public class TyphoonHazardLightning : MonoBehaviour
{
    const float Downtime = 2;
    const float ActiveTime = 3;

    [SerializeField] GameObject objToDeactivateWhenFar;
    bool inactive;
    [SerializeField] SimpleLightningRenderer lightningRenderer;
    [SerializeField] Transform node1transform;
    [SerializeField] Transform node2transform;
    [SerializeField] Animator node1Animator;
    [SerializeField] Animator node2Animator;
    [SerializeField] new Transform transform;
    [SerializeField] bool alwaysHarmful;
    [SerializeField] new BoxCollider2D collider;
    [SerializeField] ParticleSystem telegraphParticles;
    float timer = 0;

    Vector2 currentPos;
    const float DeactivationDist = 15;
#if UNITY_EDITOR
    Mesh hdWireCircleMesh;
#endif
    void Start()
    {
#if UNITY_EDITOR
        hdWireCircleMesh = Gizmos2.GetHDWireCircleMesh(transform.position, DeactivationDist, 25);
#endif
        currentPos = transform.position;
        inactive = true;
        node1Animator.CrossFade(TyphoonTilesLightningNodePair.RandAnimHash, 0);
        node2Animator.CrossFade(TyphoonTilesLightningNodePair.RandAnimHash, 0);
        if (!alwaysHarmful)
        {
            timer += (currentPos.x % Downtime) + ActiveTime + 0.0001f;
        }
        if (timer < ActiveTime)
        {
            float duration = alwaysHarmful ? float.PositiveInfinity : (ActiveTime - timer);
            lightningRenderer.ActivateAndSetAttributes(0.1f, node1transform.position, node2transform.position, duration, .8f);
        }
    }
    void Update()
    {

        if (!inactive)//if active
        {
            if (((Vector2)GameManager.PlayerPosition - currentPos).sqrMagnitude > DeactivationDist * DeactivationDist)
            {
                objToDeactivateWhenFar.SetActive(false);
                inactive = true;//active = false
            }

            if (!alwaysHarmful)
            {
                timer += Time.deltaTime;
            }
            if (timer < ActiveTime)
            {
                collider.enabled = true;
            }
            else if (timer < ActiveTime + Downtime)
            {
                collider.enabled = false;

                if (telegraphParticles != null)
                {
                    if (timer < ActiveTime + Downtime - .2f)
                    {
                        float chancePer45thSec = .5f;
                        Vector2 node1 = node1transform.localPosition;
                        Vector2 node2 = node2transform.localPosition;
                        float timeLeftUntilActivation = ActiveTime + Downtime - timer + .2f;
                        float sizeIncrease = Helper.Remap(timer, ActiveTime, ActiveTime + Downtime - .2f, 0, 0.25f);
                        if (Random2.Percent(chancePer45thSec, 45))
                        {
                            ParticleSystem.EmitParams emitParams = new();
                            Vector2 deltaPos = node2 - node1;
                            float lifetime = Random2.Float(.2f, .45f);
                            emitParams.position = node1 + Random2.Circular(0.1f);
                            emitParams.velocity = deltaPos / (1.7f * lifetime) + Random2.Circular(1);
                            emitParams.startLifetime = Mathf.Min(lifetime, timeLeftUntilActivation);
                            emitParams.startColor = FlipnoteColors.Yellow;
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
                            emitParams.startColor = FlipnoteColors.Yellow;
                            emitParams.startSize = Random2.Float(.01f, .11f) + sizeIncrease;

                            telegraphParticles.Emit(emitParams, 1);
                        }
                    }
                }
            }
            else
            {
                timer %= ActiveTime;
                lightningRenderer.ActivateAndSetAttributes(0.1f, node1transform.position, node2transform.position, ActiveTime - timer, .8f);
            }
        }
        else//if inactive
        {
            if (((Vector2)GameManager.PlayerPosition - currentPos).sqrMagnitude < DeactivationDist * DeactivationDist)
            {
                objToDeactivateWhenFar.SetActive(true);
                inactive = false;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(4);
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float rotation = transform.rotation.eulerAngles.z;

        Gizmos2.DrawRotatedRectangle(collider.offset.RotatedBy(rotation * Mathf.Deg2Rad) + (Vector2)transform.position, collider.size, rotation, Handles.UIColliderHandleColor);
        Gizmos.DrawMesh(hdWireCircleMesh);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(node1transform.position, .1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(node2transform.position, .1f);

    }
#endif

}
