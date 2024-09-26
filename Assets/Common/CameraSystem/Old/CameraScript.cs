using Assets.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    public bool canMoveUp;
    public bool canMoveDown;
    public bool canMoveRight;
    public bool canMoveLeft;
    public Vector3 viewOffset;
    public float? xOverride;
    public float? yOverride;
    [SerializeField] Camera cam;
    private void Awake()
    {
        xOverride = null;
        yOverride = null;
        canMoveUp = true;
        canMoveDown = true;
        canMoveRight = true;
        canMoveLeft = true;
    }
    public void AddModifier(CameraModifier_Old modifier)
    {
        InterpretLockCode(modifier.lockCodeMoveUp, ref canMoveUp);
        InterpretLockCode(modifier.lockCodeMoveDown, ref canMoveDown);
        InterpretLockCode(modifier.lockCodeMoveRight, ref canMoveRight);
        InterpretLockCode(modifier.lockCodeMoveLeft, ref canMoveLeft);
        viewOffset = modifier.viewOffset;
        InterpretOverrideChangeCode(modifier.xOverride, modifier.xOverrideChangeCode, ref xOverride);
        InterpretOverrideChangeCode(modifier.yOverride, modifier.yOverrideChangeCode, ref yOverride);
    }
    public void RemoveModifier(CameraModifier_Old modifier)
    {

    }
    //0 for remove, 1 for override, 2 for no change
    void InterpretOverrideChangeCode(float overrideValue, byte changeCode, ref float? overrideToChange)
    {
        if (changeCode == 0)
        {
            overrideToChange = null;
            return;
        }
        if (changeCode == 1)
        {
            overrideToChange = overrideValue;
            return;
        }

    }
    void InterpretLockCode(byte lockCode, ref bool flagToChange)
    {
        if(lockCode == 0)
        {
            flagToChange = false;
            return;
        }
        if(lockCode == 1)
        {
            flagToChange = true;
            return;
        }
    }
    void Start()
    {
        
    }
    void FixedUpdate()
    {
        float decayFactor = 10;
        Vector3 targetPoint = GameManager.PlayerPosition + viewOffset;
        if (xOverride.HasValue)
        {
            targetPoint.x = xOverride.Value;
        }
        if(yOverride.HasValue)
        {
            targetPoint.y = yOverride.Value;
        }
        Vector3 currentPos = cam.transform.position;
        targetPoint.z = currentPos.z;
        if ((canMoveRight && currentPos.x < targetPoint.x) || (canMoveLeft && currentPos.x > targetPoint.x))
        {
            currentPos.x = Helper.Decay(currentPos.x, targetPoint.x, decayFactor);
        }
        if ((canMoveUp && currentPos.y < targetPoint.y) || (canMoveDown && currentPos.y > targetPoint.y))
        {
            currentPos.y = Helper.Decay(currentPos.y, targetPoint.y, decayFactor);
        }
        transform.position = currentPos;

    }
}
