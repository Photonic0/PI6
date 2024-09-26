using Assets.Helpers;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] PlayerControl player;
    [SerializeField] float maxX;
    [SerializeField] float maxY;
    [SerializeField] float minX;
    [SerializeField] float minY;
    [SerializeField] new Camera camera;//assign in inspector
    [SerializeField] float decay;
    void FixedUpdate()//avoid camera jankiness because following rigidbody
    {
        Vector3 position = transform.position;
        Vector3 targetPos = player.transform.position;
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        position.x = Helper.Decay(position.x, targetPos.x, decay);
        position.y = Helper.Decay(position.y, targetPos.y, decay);
        transform.position = position;
    }
    public void ApplyOverride(CameraNewModifier modifier)
    {
        if (!float.IsNaN(modifier.maxX))
        {
            maxX = modifier.maxX;
        }
        if (!float.IsNaN(modifier.minX))
        {
            minX = modifier.minX;
        }
        if (!float.IsNaN(modifier.maxY))
        {
            minY = modifier.minY;
        }
        if (!float.IsNaN(modifier.minY))
        {
            minY = modifier.minY;
        }
    }
}
