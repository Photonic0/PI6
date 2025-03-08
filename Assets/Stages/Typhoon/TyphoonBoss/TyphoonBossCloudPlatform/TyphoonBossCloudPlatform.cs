using Assets.Common.Consts;
using Assets.Helpers;
using System;
using UnityEngine;

public class TyphoonBossCloudPlatform : MonoBehaviour
{
    const int StateIDHarmless = 0;
    const int StateIDHarmful = 1;
    const float HarmlessDuration = 3f;
    const float HarmfulDuration = 0.5f;
    const float TelegraphDuration = 1f;
    static readonly int animHarmless = Animator.StringToHash("CloudPlatformHarmless");
    static readonly int animHarmful = Animator.StringToHash("CloudPlatformHarmful");
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Animator animator;
    [SerializeField] new BoxCollider2D collider;
    public new Transform transform;
    public GameObject upMovementIndicator;
    public GameObject downMovementIndicator;
    public GameObject doubleUpMovementIndicator;
    public GameObject doubleDownMovementIndicator;
    float timer = 0;
    int state;
    public const float ColliderYOffset = 0.2195106f;
    public const float HalfColliderHeight = 0.6481868f * .5f;

    private void Start()
    {
        //so it only becomes harmful on command, never by its own
        timer = float.MinValue;
    }
    void Update()
    {
        switch (state)
        {
            case StateIDHarmful: //ID 1
                State_Harmful();
                break;
            default://StateIDHarmless ID 0
                State_Harmless();
                break;
        }
        timer += Time.deltaTime;
    }
    private void FixedUpdate()
    {
        UpdateCollider();
    }
    private void State_Harmful()
    {
        if (timer > HarmfulDuration)
        {
            state = StateIDHarmless;
            animator.CrossFade(animHarmless, 0);
            timer = float.MinValue;//so it doesn't become harmful by its own over time, only by command
        }
    }
    private void State_Harmless()
    {
        //flash as warning
        //todo: replace this terrible telegraph
        if (timer > HarmlessDuration - TelegraphDuration)
        {
            sprite.color = Helper.Remap(timer, HarmlessDuration - TelegraphDuration, HarmlessDuration, Color.white, Color.black);
        }
        if (timer > HarmlessDuration)
        {
            sprite.color = Color.white;
            timer %= HarmfulDuration;
            animator.CrossFade(animHarmful, 0);
            state = StateIDHarmful;
        }
    }
    public void BecomeHarmful(float duration)
    {
        timer = HarmfulDuration - duration;
        state = StateIDHarmful;
        animator.CrossFade(animHarmful, 0);
        sprite.color = Color.white;
    }
    public void StartTransitionToHarmfulAndBecomeHarmful()
    {
        timer = HarmlessDuration - TelegraphDuration;
        state = StateIDHarmless;
        animator.CrossFade(animHarmless, 0);
    }
    void UpdateCollider()
    {
        if (state == StateIDHarmful)
        {
            collider.enabled = true;
            return;
        }
        collider.enabled = true;
    }
    private void GetPlayerBottomAndColliderTop(out Vector2 playerBottom, out Vector2 colliderTop)
    {
        playerBottom = GameManager.PlayerControl.transform.position;
        playerBottom.y -= 1;//the actual bottom of the player collider
        colliderTop = transform.position;
        colliderTop.y += ColliderYOffset;//collider y offset
        colliderTop.y += HalfColliderHeight;//half collider height
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForDamage(collision);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckForDamage(collision);
    }
    private void CheckForDamage(Collision2D collision)
    {
        if (state == StateIDHarmful && collision.gameObject.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(5);
        }
    }
    public void MoveTo(Vector2 pos)
    {
        MoveTo(pos.x, pos.y);
    }
    public void MoveTo(float x, float y)
    {
        Vector3 pos = transform.position;
        //GetPlayerBottomAndColliderTop(out Vector2 playerBottom, out Vector2 oldColliderTop);
        //Vector3 oldPos = pos;
        pos.Set(x, y, 0);
        transform.position = pos;
        //GetPlayerBottomAndColliderTop(out _, out Vector2 colliderTop);
        //if (!collider.enabled)
        //{
        //    return;
        //}
        //Vector2 deltaPos = new(x - oldPos.x, y - oldPos.y);
        //RaycastHit2D hit = Physics2D.BoxCast(oldPos, collider.size, 0f, deltaPos.normalized, deltaPos.magnitude, Layers.Player);
        //if (hit.collider != null)
        //{

        //    Vector3 playerPos = GameManager.PlayerControl.transform.position;
        //    if(Mathf.Abs(hit.normal.x) > 0.9f)
        //    {
        //        x = collider.bounds.center.x;
        //        float halfWidth = collider.bounds.extents.x;
        //        playerPos.x = x + halfWidth * hit.normal.x;
                
        //        GameManager.PlayerControl.transform.position = playerPos;
        //        return;
        //    }

        //    float amountToMovePlayer = Mathf.InverseLerp(oldColliderTop.y, colliderTop.y, playerBottom.y);
        //    playerPos.x = Mathf.Lerp(playerPos.x, x, amountToMovePlayer);
        //    playerPos.y = y;//don't lerp Y because player will always be at the top of the platform
        //    playerPos.y += 0.2195106f;//collider y offset
        //    playerPos.y += 0.6481868f * .5f;//half collider height
        //    playerPos.y++;//to make it set to bottom of the player, not the center (player is 2 units tall)
        //    GameManager.PlayerControl.rb.velocity = Vector2.zero;
        //    GameManager.PlayerControl.transform.position = playerPos;
        //}
    }
    public void EnableAppropriateMovementIndicator(sbyte oldPositionID, sbyte currentPositionID)
    {
        DisableMovementIndicators();

        int movementDifference = currentPositionID - oldPositionID;
        if (movementDifference > 0)
        {
            upMovementIndicator.SetActive(true);
            if (movementDifference > 1)
            {
                doubleUpMovementIndicator.SetActive(true);
            }
        }
        else if (movementDifference < 0)
        {
            downMovementIndicator.SetActive(true);
            if (movementDifference < -1)
            {
                doubleDownMovementIndicator.SetActive(true);
            }
        }
    }

    //public void EnableAppropriateMovementIndicator(sbyte oldPositionID, sbyte currentPositionID)
    //{
    //    if(oldPositionID > currentPositionID)
    //    {
    //        downMovementIndicator.SetActive(true);
    //        upMovementIndicator.SetActive(false);
    //        doubleUpMovementIndicator.SetActive(false);
    //        if(Mathf.Abs(oldPositionID - currentPositionID) > 1)
    //        {
    //            doubleDownMovementIndicator.SetActive(true);
    //        }
    //    }
    //    else if(oldPositionID < currentPositionID)
    //    {
    //        downMovementIndicator.SetActive(false);
    //        upMovementIndicator.SetActive(true);
    //        doubleDownMovementIndicator.SetActive(false);
    //        if (Mathf.Abs(oldPositionID - currentPositionID) > 1)
    //        {
    //            doubleUpMovementIndicator.SetActive(true);
    //        }
    //    }
    //    else
    //    {
    //        DisableMovementIndicators();
    //    }
    //}
    public void DisableMovementIndicators()
    {
        upMovementIndicator.SetActive(false);
        downMovementIndicator.SetActive(false);
        doubleUpMovementIndicator.SetActive(false);
        doubleDownMovementIndicator.SetActive(false);
    }
}
