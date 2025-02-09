using Assets.Helpers;
using Assets.Systems;
using UnityEngine;

public class TyphoonHazardFan : MonoBehaviour
{
    float timer = 0;
    int state;
    const int StateIDOn = 0;
    const int StateIDOff = 1;
    const float OnDuration = 3;
    const float OffDuration = 2;
    const float WindColumnWidth = 2.25f;//compensates for player width as well. It's going to be checking Rect x point
    const float WindColumnHeight = 15;
    const float WindPointBlankStrength = 200;
    [SerializeField] Animator animator;
    [SerializeField] new Transform transform;
    [SerializeField] ParticleSystem windParticleEmitter;
    [SerializeField] AudioSource audioSource;
    //state begins at 0, being on, so don't need to call particleSystem.Stop() inside Start() method.
    private void Start()
    {
        audioSource.volume = .5f;
        audioSource.clip = TyphoonStageSingleton.instance.fanNoises[Random.Range(0, TyphoonStageSingleton.instance.fanNoises.Length)];
        audioSource.Play();
        GameManager.OnPause += PauseFanNoise;
        GameManager.OnUnPause += ResumeFanNoise;
    }

    private void ResumeFanNoise()
    {
        audioSource.UnPause();
    }
    private void PauseFanNoise()
    {
        audioSource.Pause();
    }
    private void OnDestroy()
    {
        GameManager.OnPause -= PauseFanNoise;
        GameManager.OnUnPause -= ResumeFanNoise;
    }
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
            audioSource.clip = TyphoonStageSingleton.instance.fanNoises[Random.Range(0, TyphoonStageSingleton.instance.fanNoises.Length)];
            audioSource.Play();
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
        float rotationAmount = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        playerPos = (playerPos - center).RotatedBy(rotationAmount) + center;
        audioSource.volume = Mathf.InverseLerp(12f, 2f, (playerPos - center).magnitude) * .5f;
#if UNITY_EDITOR
        debug_transformedPlayerPosition = playerPos;
#endif

        if (playerPos.x < center.x + halfWidth && playerPos.x > center.x - halfWidth && playerPos.y > center.y)
        {
            float deltaY = playerPos.y - center.y;
            float pushSpeed = Helper.Remap(deltaY, 0, WindColumnHeight, WindPointBlankStrength, 1, Easings.SqrOut);
            Vector2 vel = new Vector2(0, pushSpeed).RotatedBy(-rotationAmount);
            //don't need to rotate the velocity for some reason idk why
            //vel = vel.RotatedBy(-rotationAmount);
#if UNITY_EDITOR

            debug_pushSpeedVelVector = vel;
#endif
            GameManager.PlayerControl.acceleration += vel * Time.deltaTime;
        }
        if (timer > OnDuration)
        {
            windParticleEmitter.Stop();
            state = StateIDOff;
            timer = 0;
            animator.enabled = false;
        }
    }
    public (float minX, float maxX, float minY, float maxY) GetWindBounds(float padding)
    {
        float fanSpriteHalfHeight = 0.74f;
        float fanSpriteHalfWidth = 1.04f;
        Vector2 pos = transform.position;
        return (pos.x - fanSpriteHalfWidth - padding,
        pos.x + fanSpriteHalfWidth + padding,
        pos.y - fanSpriteHalfHeight - padding,
        pos.y + fanSpriteHalfWidth + padding);
    }
#if UNITY_EDITOR

    [SerializeField] Vector2 debug_transformedPlayerPosition;
    [SerializeField] Vector2 debug_pushSpeedVelVector;
    private void OnDrawGizmos()
    {
        if (debug_transformedPlayerPosition != Vector2.zero)
        {
            Gizmos.DrawLine(debug_transformedPlayerPosition, transform.position);
        }
        float rotationAmount = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 center = (Vector2)transform.position + new Vector2(0, WindColumnHeight / 2).RotatedBy(rotationAmount);
        Gizmos2.DrawRotatedRectangle(center, new Vector2(WindColumnWidth, WindColumnHeight), rotationAmount * Mathf.Rad2Deg);
        if (debug_transformedPlayerPosition != Vector2.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)debug_pushSpeedVelVector);
        }
    }
#endif

}
