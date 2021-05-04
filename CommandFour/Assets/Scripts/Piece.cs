using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public float FallAnimationSpeed = 1.0f;
    public float WinAnimationSpeed = 0.5f;
    public float ResetAnimatonSpeed = 1.0f;

    public Material WinMaterial;
    public Material ResetMaterial;

    // Used to show victory pieces
    public bool IsWinnerPiece = false;

    public enum ANIMATION_TYPE
    {
        NONE,
        FALL,
        WIN,
        RESET
    };

    public delegate void OnAnimationEnded_Delegate(Transform transform);

    public OnAnimationEnded_Delegate OnFallAnimationEnded;
    public OnAnimationEnded_Delegate OnWinAnimationEnded;
    public OnAnimationEnded_Delegate OnResetAnimationEnded;

    ANIMATION_TYPE Type = ANIMATION_TYPE.NONE;

    float timePassed = 0.0f;

    // Used to animate piece material based in a Texture Mask
    float MaterialAnimation = 0.0f;

    // When animating the cell the fall, we need to know the start and the end position
    Vector3 FallAnimation_EndPosition;

    // The time of the animation is based in the number of cells it must fall
    float FallAnimation_Time;

    public void StartFallAnimation(Vector3 endPosition, float animationTime)
    {
        timePassed = 0.0f;
        Type = ANIMATION_TYPE.FALL;
        FallAnimation_EndPosition = endPosition;
        FallAnimation_Time = animationTime;
    }

    public void StartWinAnimation()
    {
        if (IsWinnerPiece)
            return;

        Type = ANIMATION_TYPE.WIN;
        MaterialAnimation = 0.0f;
        timePassed = 0.0f;

        GetComponent<Renderer>().material = WinMaterial;
    }

    public void StartResetAnimation()
    {
        Type = ANIMATION_TYPE.RESET;
        MaterialAnimation = 0.0f;
        timePassed = 0.0f;

        GetComponent<Renderer>().material = ResetMaterial;
    }

    void UpdateFallAnimation()
    {
        // Avoiding timePassed to pass the limit of the animation
        timePassed = (timePassed + Time.deltaTime) > FallAnimation_Time ?
                      FallAnimation_Time : (timePassed + Time.deltaTime * FallAnimationSpeed);

        // Animating the fall to right position
        Vector3 animationPosition = FallAnimation_EndPosition;
        animationPosition.y *= timePassed / FallAnimation_Time;

        transform.localPosition = animationPosition;

        if (timePassed >= FallAnimation_Time)
        {
            Type = ANIMATION_TYPE.NONE;

            if (OnFallAnimationEnded != null)
                OnFallAnimationEnded(transform);
        }
    }

    void UpdateMaterialAnimation(ANIMATION_TYPE type, float animationSpeed)
    {
        timePassed += Time.deltaTime * animationSpeed;
        timePassed = timePassed > 1.0f ? 1.0f : timePassed;

        GetComponent<Renderer>().material.SetFloat("_AnimationTime", timePassed);

        if (timePassed >= 1.0f)
        {
            if (type == ANIMATION_TYPE.WIN)
            {
                if (OnWinAnimationEnded != null)
                    OnWinAnimationEnded(transform);

            }
            else
            {
                if (OnResetAnimationEnded != null)
                    OnResetAnimationEnded(transform);
            }

            Type = ANIMATION_TYPE.NONE;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Type == ANIMATION_TYPE.NONE)
            return;

        switch (Type)
        {
            case ANIMATION_TYPE.FALL:
                UpdateFallAnimation();
            break;
            case ANIMATION_TYPE.WIN:
                UpdateMaterialAnimation(ANIMATION_TYPE.WIN, WinAnimationSpeed);
            break;
            case ANIMATION_TYPE.RESET:
                UpdateMaterialAnimation(ANIMATION_TYPE.RESET, ResetAnimatonSpeed);
            break;
        }
    }
}
