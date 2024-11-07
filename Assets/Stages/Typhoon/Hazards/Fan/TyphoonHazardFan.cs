using Assets.Helpers;
using UnityEngine;

public class TyphoonHazardFan : MonoBehaviour
{
    float timer = 0;
    int state;
    const int StateIDOn = 0;
    const int StateIDOff = 1;
    const float OnDuration = 3;
    const float OffDuration = 2;
    const float WindColumnWidth = 2;//compensates for player width as well. It's going to be checking Rect x point
    [SerializeField] Animator animator;
    [SerializeField] new Transform transform;
    [SerializeField] ParticleSystem windParticleEmitter;
    //state begins at 0, beaing on, so don't need to call particleSystem.Stop() inside Start() method.
    void Update()
    {
        timer += Time.deltaTime;
        switch (state)
        {
            case StateIDOff:
                State_Off();
                break;
            default://StateIDOn
                State_On();
                break;
        }
    }
    private void State_Off()
    {
        if (timer > OffDuration)
        {
            windParticleEmitter.Play();
            state = StateIDOn;
            timer = 0;
            animator.enabled = true;
        }
    }
    private void State_On()
    {
        Vector2 playerPos = GameManager.PlayerPosition;
        Vector2 center = transform.position;
        float halfWidth = WindColumnWidth / 2;
        if (playerPos.x < center.x + halfWidth && playerPos.x > center.x - halfWidth && playerPos.y > center.y)
        {
            float deltaY = playerPos.y - center.y;
            float pushSpeed = Helper.Remap(deltaY, 0, 15, 30, 1);
            Vector2 vel = GameManager.PlayerControl.rb.velocity;
            vel.y = Mathf.Max(pushSpeed, vel.y);
            GameManager.PlayerControl.rb.velocity = vel;
        }

        if (timer > OnDuration)
        {
            windParticleEmitter.Stop();
            state = StateIDOff;
            timer = 0;
            animator.enabled = false;
        }
    }
}
