using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <TODO>
///   Fix wall sliding (snap model to the wall?)
/// </TODO>

public class PlayerMovement : MonoBehaviour
{
    CharacterController characterController;
    //Animator animator;
    AudioSource audioSource;

    [Header("General Variables")]
    [SerializeField] Transform groundPoint;
    [SerializeField] float normalSpeed = 10f;
    //[SerializeField] float sprintSpeed = 20f;
    [SerializeField] float gravity = 9.81f, lookSpeed = 3f, verticalRotationLimit = 330f;

    [Header("Jumping")]
    [SerializeField] float jumpVelocity = 10f;
    [SerializeField] float jumpPressWhileInAirTime = 0.5f, fallingTimeWindowsForJump = 0.1f;

    Vector2 movement; 
    Vector3 horizontal, lastVelocity;
    float yVelocity, currentSpeed, sinceLastAirJumpPress = 5f, sinceMovementFixes;
    float sinceGround, sinceJump;
    Camera normalCamera;
    Transform normalCameraPoint;
    Transform freeCameraPoint;
    bool isSprinting;
    bool inFreecam, inJump;

    MovementState state;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        //animator = GetComponent<Animator>();
    }
    private void Start()
    {
        normalCamera = Player.instance.normalCamera;
        normalCameraPoint = Player.instance.normalCameraPoint;
        freeCameraPoint = Player.instance.freeCameraPoint;
        Cursor.lockState = CursorLockMode.Locked; // TODO: if not training
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        //print(animator.GetInteger("jumpState"));
        sinceLastAirJumpPress += Time.fixedDeltaTime;
        sinceJump += Time.fixedDeltaTime;
        sinceGround += Time.fixedDeltaTime;
        sinceMovementFixes += Time.fixedDeltaTime;
        currentSpeed = normalSpeed;
        //currentSpeed = isSprinting ? sprintSpeed : normalSpeed;
        

        //if (state != MovementState.diving)  Neki drugi state??
        //{
            /// Unmodified horizontal movement
            //else {
                horizontal = transform.forward * movement.y + transform.right * movement.x;
                /// Normalize the horizontal speed so pressing both buttons isn't faster
                if (horizontal.magnitude > 1)
                    horizontal = horizontal.normalized;
            //}
        //}

        /// Jumping
        if (characterController.isGrounded)
        {
            sinceGround = 0f;
            if (inJump)
                inJump = false;
            if (yVelocity < 0)
                yVelocity = 0;
            if (sinceLastAirJumpPress < jumpPressWhileInAirTime)
            {
                Jump();
                sinceLastAirJumpPress = jumpPressWhileInAirTime + 1f;
            }
        }


        /// Apply gravity movement
        yVelocity -= gravity * Time.fixedDeltaTime;

        //lastVelocity = characterController.velocity;

        Vector3 immediateVelocity = new Vector3(0, FixStairs(yVelocity), 0);

        /// Move the character
        Vector3 baseVelocity = currentSpeed * horizontal + yVelocity * Vector3.up;
        baseVelocity = AdjustVelocityToSlope(baseVelocity);
        characterController.Move(Time.fixedDeltaTime * baseVelocity + immediateVelocity);

        StateHandler();
        AnimationHandler();
        SoundHandler();
    }

    #region Handlers
    MovementState previousState;
    private void StateHandler()
    {
        previousState = state;
        if (characterController.isGrounded)
            state = horizontal.magnitude > 0.01f ? MovementState.running : MovementState.idle;
        else
            state = yVelocity > 0 ? MovementState.airUp : MovementState.airDown;
    }

    private void AnimationHandler()
    {
        return;
        //if (previousState != state) /// if the state changed
        //{
        //    if (state == MovementState.airDown)
        //        animator.SetInteger("jumpState", 2);
        //    if (state == MovementState.running)
        //        animator.SetBool("running", true);
        //    else if (previousState == MovementState.running)
        //        animator.SetBool("running", false);
        //}
        //if (!previousState.IsGrounded() && state.IsGrounded())
        //{
        //    animator.SetInteger("jumpState", 0);
        //}
    }
    
    /// <summary>
    /// Handles only some sounds, mostly those that trigger every frame
    /// </summary>
    private void SoundHandler() 
    {
        if (!previousState.IsGrounded() && state.IsGrounded())
        {
            SoundManager.PlaySound(audioSource, SoundManager.Sound.land);
        }
        switch (state)
        {
            case MovementState.running:
                SoundManager.PlaySound(audioSource, SoundManager.Sound.step);
                break;
        }
    }    
    #endregion

    #region MovementFixes
    private Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        var ray = new Ray(groundPoint.position, Vector3.down);

        if(Physics.Raycast(ray, out RaycastHit hit, characterController.stepOffset))
        {
            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            var adjustedVelocity = slopeRotation * velocity;

            if (adjustedVelocity.y < 0)
            {
                sinceMovementFixes = 0f;
                return adjustedVelocity;
            }
        }
        return velocity;
    }
    private float FixStairs(float velocity)
    {
        if (velocity >= 0 || inJump || characterController.isGrounded || sinceGround > 0.2f)
            return 0;

        var ray = new Ray(groundPoint.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, characterController.stepOffset * 2f))
        {
            sinceMovementFixes = 0f;
            return hit.point.y - groundPoint.position.y;
        }
        else
            return 0;
    }
    #endregion

    #region InputActions
    public void OnMove(InputValue value) => movement = value.Get<Vector2>();

    public void OnJump(InputValue value)
    {
        if (characterController.isGrounded || (sinceGround < fallingTimeWindowsForJump && sinceJump > fallingTimeWindowsForJump * 1.5f))
        {
            Jump();
        }
        else
            sinceLastAirJumpPress = 0f;
    }

    Vector2 rotation;
    public void OnLook(InputValue value)
    {
        rotation.y += value.Get<Vector2>().x;
        rotation.x -= value.Get<Vector2>().y;
        rotation.x = Mathf.Clamp(rotation.x, -verticalRotationLimit, verticalRotationLimit);
        if (!inFreecam)
        {
            transform.eulerAngles = new Vector2(0, rotation.y) * lookSpeed;
            normalCamera.transform.localRotation = Quaternion.Euler(rotation.x * lookSpeed, 0, 0);
        }
        else
        {
            freeCameraPoint.eulerAngles = new Vector2(rotation.x, rotation.y) * lookSpeed;
            //freeCameraPoint.localRotation = Quaternion.Euler(rotation.x * lookSpeed, 0, 0);
        }
    }

    public void OnSprint(InputValue value) => isSprinting = value.Get<float>() > 0.5f;

    public void OnToggleFreecam(InputValue value)
    {
        inFreecam = !inFreecam;
        Player.instance.freeCameraPoint.gameObject.SetActive(!Player.instance.freeCameraPoint.gameObject.activeSelf);
        Player.instance.normalCamera.gameObject.SetActive(!Player.instance.normalCamera.gameObject.activeSelf);
    }
    #endregion

    void Jump()
    {
        yVelocity += jumpVelocity;
        sinceJump = 0f;
        inJump = true;
        SoundManager.PlaySound(audioSource, SoundManager.Sound.playerJump);
        //animator.SetInteger("jumpState", 1);
    }
}
