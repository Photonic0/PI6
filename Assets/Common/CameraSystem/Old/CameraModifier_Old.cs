using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraModifier_Old : MonoBehaviour
{
    [SerializeField] new BoxCollider2D collider;
    
    //MEANING:
    //0 MEANS FALSE
    //1 MEANS TRUE  
    //2 MEANS NO CHANGE
    [Tooltip("0 for false, 1 for true, 2 for no change")]
    public byte lockCodeMoveUp;
    [Tooltip("0 for false, 1 for true, 2 for no change")]
    public byte lockCodeMoveDown;
    [Tooltip("0 for false, 1 for true, 2 for no change")]
    public byte lockCodeMoveRight;
    [Tooltip("0 for false, 1 for true, 2 for no change")]
    public byte lockCodeMoveLeft;
    public Vector3 viewOffset;
    [Tooltip("0 for remove, 1 for override, 2 for no change")]
    public byte xOverrideChangeCode;
    public float xOverride;
    [Tooltip("0 for remove, 1 for override, 2 for no change")]
    public byte yOverrideChangeCode;
    public float yOverride;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out CameraScript camera))
        {
            camera.AddModifier(this);
        }
    }
}
