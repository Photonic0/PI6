using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Common.Effects.Lightning;
using Assets.Helpers;
using UnityEngine;

public class TyphoonHazardLightning : MonoBehaviour
{
    [SerializeField] GameObject objToDeactivateWhenFar;
    bool inactive;
    [SerializeField] SimpleLightningRenderer lightningRenderer;
    [SerializeField] Transform node1transform;
    [SerializeField] Transform node2transform;
    [SerializeField] Animator node1Animator;
    [SerializeField] Animator node2Animator;
    [SerializeField] new Transform transform;
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
        lightningRenderer.ActivateAndSetAttributes(0.1f, node1transform.position, node2transform.position, float.PositiveInfinity, .8f); ;
    }
    void Update()
    {
        if(!inactive)
        {
            if (((Vector2)GameManager.PlayerPosition - currentPos).sqrMagnitude > DeactivationDist * DeactivationDist)
            {
                objToDeactivateWhenFar.SetActive(false);
                inactive = true;
            }
        }
        else
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
            collision.GetComponent<PlayerLife>().Damage(4);
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawMesh(hdWireCircleMesh);
    }
#endif

}
