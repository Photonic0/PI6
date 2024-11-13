
using UnityEngine;

public class SpikeShot : Projectile
{
    public override int Damage => 3;
    [SerializeField] new Transform transform;
    private void FixedUpdate()
    {
        transform.Rotate(new Vector3(0,0,Time.fixedDeltaTime * 720));
    }
}
