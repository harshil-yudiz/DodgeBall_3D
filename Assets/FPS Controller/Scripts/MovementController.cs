using UnityEngine;
using FirstPersonMobileTools.Utility;

namespace FirstPersonMobileTools.DynamicFirstPerson
{

    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CameraLook))]
    public class MovementController : MonoBehaviour
    {

        #region Class accessible field
        [HideInInspector] public bool Input_Sprint { get; set; }    // Accessed through [Sprint button] in the scene
        [HideInInspector] public bool Input_Jump { get; set; }    // Accessed through [Jump button] in the scene
        [HideInInspector] public bool Input_Crouch { get; set; }    // Accessed through [Crouch button] in the scene

        [HideInInspector] public float Walk_Speed { private get { return m_WalkSpeed; } set { m_WalkSpeed = value; } }          // Accessed through [Walk speed] slider in the settings
        [HideInInspector] public float Run_Speed { private get { return m_RunSpeed; } set { m_RunSpeed = value; } }             // Accessed through [Run speed] slider in the settings
        [HideInInspector] public float Crouch_Speed { private get { return m_CrouchSpeed; } set { m_CrouchSpeed = value; } }    // Accessed through [Crouch speed] slider in the settings
        [HideInInspector] public float Jump_Force { private get { return m_JumpForce; } set { m_JumpForce = value; } }          // Accessed through [Jump Force] slider in the settings
        [HideInInspector] public float Acceleration { private get { return m_Acceleration; } set { m_Acceleration = value; } }  // Accessed through [Acceleration] slider in the settings
        [HideInInspector] public float Land_Momentum { private get { return m_LandMomentum; } set { m_LandMomentum = value; } } // Accessed through [Landing Momentum] slider in the settings
        #endregion

        #region Editor accessible field
        // Input Settings
        [SerializeField] private Joystick m_Joystick;   // Available joystick mobile in the scene

        // Ground Movement Settings
        [SerializeField] private float m_Acceleration = 1.0f;
        [SerializeField] private float m_WalkSpeed = 1.0f;
        [SerializeField] private float m_RunSpeed = 3.0f;
        [SerializeField] private float m_CrouchSpeed = 0.5f;
        [SerializeField] private float m_CrouchDelay = 0.5f;    // Crouch transition time
        [SerializeField] private float m_CrouchHeight = 1.0f;   // Crouch target height

        // Air Movement Settings
        [SerializeField] private float m_JumpForce = 1.0f;      // y-axis force for jumping
        [SerializeField] private float m_Gravity = 10.0f;       // Gravity force
        [SerializeField] private float m_LandMomentum = 2.0f;   // Movement momentum strength after landed

        // Audio Settings
        [SerializeField] private AudioClip[] m_FootStepSounds;  // list of foot step sfx
        [SerializeField] private AudioClip m_JumpSound;         // Jumping sfx
        [SerializeField] private AudioClip m_LandSound;         // Landing sfx

        // Advanced Settings
        [SerializeField] private Bobbing m_WalkBob = new Bobbing(); // Bobbing for walking
        [SerializeField] private Bobbing m_IdleBob = new Bobbing(); // Bobbing for idling
        #endregion

        // Main reference class
        private Camera m_Camera;
        private CharacterController m_CharacterController;
        private CameraLook m_CameraLook;
        private AudioSource m_AudioSource;

        // Main global value
        private Vector3 m_MovementDirection;                        // Vector3 value for CharacterController.Move()
        private Vector3 m_HeadMovement;                             // Used for calculating all the head movement before applying to the camera position
        private Vector3 m_LandBobRange_FinalImpact;                 // Dynamic bob range based on falling velocity
        private Vector3 m_OriginalScale;                            // Original scale for crouching
        private const float m_StickToGround = -1f;                  // force character controller into the ground
        private float m_MinFallLand = -10f;                         // Minimum falling velocity to be considered as landed
        private float m_CrouchTimeElapse = 0.0f;                    // Time beofre 
        private float m_OriginalLandMomentum;                       // Slowdown momentum when landing
        private bool m_IsFloating = false;                          // Player state if is in the air
        private bool m_PreviousCrouchState = false;                 // Store previous crouch state
        private bool m_PreviousJumpState = false;                   // Store previous jump state
        private bool m_ResetMovementOnStateChange = false;          // Flag to reset movement after state change
        private float m_StateChangeTimer = 0f;                      // Timer for state change reset


        private float m_MovementVelocity
        {
            get { return new Vector2(m_CharacterController.velocity.x, m_CharacterController.velocity.z).magnitude; }
        }

        private Vector2 Input_Movement
        {
            get { if (m_Joystick != null) return new Vector2(m_Joystick.Horizontal, m_Joystick.Vertical); else return Vector2.zero; }
        }

        private bool m_IsWalking
        {
            get { return m_MovementVelocity > 0.0f; }
        }

        private bool m_OnLanded
        {
            get { return m_IsFloating && m_MovementDirection.y < m_MinFallLand && m_CharacterController.isGrounded; }
        }

        private bool m_OnJump
        {
            get { return !m_CharacterController.isGrounded && !m_IsFloating; }
        }

        private float m_speed
        {
            get
            {
#if UNITY_EDITOR
                return Input_Crouch ? m_CrouchSpeed : Input_Sprint ? m_RunSpeed :
                    Input_Movement.magnitude != 0 || External_Input_Movement.magnitude != 0 ? m_WalkSpeed : 0.0f;
#elif UNITY_ANDROID
                return Input_Crouch? m_CrouchSpeed : Input_Sprint? m_RunSpeed : Input_Movement.magnitude != 0? m_WalkSpeed : 0.0f; 
#endif
            }
        }

#if UNITY_EDITOR
        public Vector2 External_Input_Movement;
#endif


        private void Start()
        {

            m_Camera = GetComponentInChildren<Camera>();
            m_AudioSource = GetComponent<AudioSource>();
            m_CharacterController = GetComponent<CharacterController>();
            m_CameraLook = GetComponent<CameraLook>();

            m_CharacterController.height = m_CharacterController.height;
            m_OriginalScale = transform.localScale;
            m_OriginalLandMomentum = m_LandMomentum;

            m_WalkBob.SetUp();
            m_IdleBob.SetUp();

        }

        private void Update()
        {
            // Check for state changes that require movement reset
            CheckStateChanges();

            // AlignPlayerWithCamera();
            Handle_InputMovement();
            StopPlayerOnSprintRelease();
            Handle_AirMovement();
            Handle_Crouch();
            Handle_Step();

            //   UpdateWalkBob();

            m_CharacterController.Move(m_MovementDirection * Time.deltaTime);

            m_Camera.transform.localPosition += m_HeadMovement;
            m_HeadMovement = Vector3.zero;

            // Save the current state for next frame comparison
            m_PreviousCrouchState = Input_Crouch;
            m_PreviousJumpState = Input_Jump;
        }

        private void CheckStateChanges()
        {
            // Check if state has changed since last frame
            bool stateChanged = (Input_Crouch != m_PreviousCrouchState) || (Input_Jump != m_PreviousJumpState);

            // If we've detected a state change, flag that we need to reset movement
            if (stateChanged)
            {
                m_ResetMovementOnStateChange = true;
                m_StateChangeTimer = 0f;
                m_LandMomentum = m_OriginalLandMomentum; // Reset landing momentum immediately
            }

            // If we need to reset movement after a state change
            if (m_ResetMovementOnStateChange)
            {
                m_StateChangeTimer += Time.deltaTime;

                // If we detect movement input after a state change
                bool hasMovementInput = Input_Movement.magnitude > 0.1f;
#if UNITY_EDITOR
                hasMovementInput = hasMovementInput || External_Input_Movement.magnitude > 0.1f;
#endif

                // If there's movement input, handle instant acceleration
                if (hasMovementInput)
                {
                    // Reset the flag since we're handling it now
                    m_ResetMovementOnStateChange = false;

                    // Ensure landing momentum doesn't affect movement
                    m_LandMomentum = m_OriginalLandMomentum;
                }

                // If no movement detected within 0.5 seconds, cancel the reset
                if (m_StateChangeTimer > 0.5f)
                {
                    m_ResetMovementOnStateChange = false;
                }
            }
        }

        private void Handle_InputMovement()
        {
            Vector2 Input;
#if UNITY_EDITOR
            Input.x = Input_Movement.x == 0 ? External_Input_Movement.x : Input_Movement.x;
            Input.y = Input_Movement.y == 0 ? External_Input_Movement.y : Input_Movement.y;
#elif UNITY_ANDROID
            Input.x = Input_Movement.x;
            Input.y = Input_Movement.y;
#endif
            Vector3 WalkTargetDIrection =
                Input.y * transform.forward * m_speed +
                Input.x * transform.right * m_speed;

            WalkTargetDIrection = Input_Sprint && WalkTargetDIrection == Vector3.zero ? transform.forward * m_speed : WalkTargetDIrection;

            // Calculate acceleration factor - use higher acceleration after state changes when input is detected
            float accelerationFactor = m_ResetMovementOnStateChange && Input.magnitude > 0.1f ? m_Acceleration * 5f : m_Acceleration;

            m_MovementDirection.x = Mathf.MoveTowards(m_MovementDirection.x, WalkTargetDIrection.x, accelerationFactor * Time.deltaTime);
            m_MovementDirection.z = Mathf.MoveTowards(m_MovementDirection.z, WalkTargetDIrection.z, accelerationFactor * Time.deltaTime);

            if (m_LandMomentum != m_OriginalLandMomentum)
            {
                m_LandMomentum = Mathf.Clamp(m_LandMomentum + Time.deltaTime * 3f, 0, m_OriginalLandMomentum); // Faster recovery
                m_MovementDirection.x *= m_LandMomentum / m_OriginalLandMomentum;
                m_MovementDirection.z *= m_LandMomentum / m_OriginalLandMomentum;
            }
        }

        private void Handle_AirMovement()
        {

            if (m_OnLanded)
            {
                // Set landing momentum to a higher value to reduce the slowdown effect
                m_LandMomentum = m_OriginalLandMomentum * 0.7f; // Less momentum loss on landing
                PlaySound(m_LandSound);
            }

            if (m_CharacterController.isGrounded)
            {

                if (m_IsFloating) m_IsFloating = false;

                // force player to stick to ground or else CharacterController.IsGrounded will return true
                m_MovementDirection.y = m_StickToGround;

                if (Input_Jump)
                {
                    PlaySound(m_JumpSound);
                    m_MovementDirection.y = m_JumpForce;
                    if (m_JumpSound != null) PlaySound(m_JumpSound);
                }

            }
            else
            {

                if (!m_IsFloating) m_IsFloating = true;

                // Prevent floating if jumping is blocked 
                if (m_CharacterController.collisionFlags == CollisionFlags.Above)
                {
                    m_MovementDirection.y = 0.0f;
                }

                m_MovementDirection.y -= m_Gravity * Time.deltaTime;

            }

        }

        private void Handle_Crouch()
        {

            //Crouching State
            if (Input_Crouch && transform.localScale.y != (m_CrouchHeight / m_CharacterController.height) * m_OriginalScale.y)
            {
                CrouchTransition(m_CrouchHeight, Time.deltaTime);
            }

            // Standing State
            if (!Input_Crouch && transform.localScale.y != m_OriginalScale.y)
            {
                CrouchTransition(m_CharacterController.height, -Time.deltaTime);
            }

            void CrouchTransition(float TargetHeight, float value)
            {

                // Origin is on top of head to avoid any collision with the player it self
                Vector3 Origin = transform.position + (transform.localScale.y / m_OriginalScale.y) * m_CharacterController.height * Vector3.up;
                if (Physics.Raycast(Origin, Vector3.up, m_CharacterController.height - Origin.y))
                {
                    Input_Crouch = true;
                    return;
                }

                m_CrouchTimeElapse += value;

                m_CrouchTimeElapse = Mathf.Clamp(m_CrouchTimeElapse, 0, m_CrouchDelay);

                transform.localScale = new Vector3(
                    transform.localScale.x,
                    Mathf.Lerp(m_OriginalScale.y, (m_CrouchHeight / m_CharacterController.height) * m_OriginalScale.y, m_CrouchTimeElapse / m_CrouchDelay),
                    transform.localScale.z);

            }

        }

        private void Handle_Step()
        {
            if (m_FootStepSounds.Length == 0) return;
            if (m_WalkBob.OnStep) PlaySound(m_FootStepSounds[UnityEngine.Random.Range(0, m_FootStepSounds.Length - 1)]);
        }

        private void UpdateWalkBob()
        {
            if ((m_IsWalking && !m_IsFloating) || !m_WalkBob.BackToOriginalPosition)
            {
                float speed = m_MovementVelocity == 0 ? m_WalkSpeed : m_MovementVelocity;
                m_HeadMovement += m_WalkBob.UpdateBobValue(speed, m_WalkBob.BobRange);
            }
            else if (!m_IsWalking || !m_IdleBob.BackToOriginalPosition)
            {
                m_HeadMovement += m_IdleBob.UpdateBobValue(1, m_IdleBob.BobRange);
            }
        }

        // Utility function
        private void PlaySound(AudioClip audioClip)
        {
            m_AudioSource.clip = audioClip;
            if (m_AudioSource.clip != null) m_AudioSource.PlayOneShot(m_AudioSource.clip);
        }

        // Add these variables to the MovementController class
        private bool m_WasSprintingLastFrame = false;
        private bool m_StopOnSprintRelease = false;
        private Vector3 m_SprintVelocity = Vector3.zero;

        // Modified sprint handling logic
        private void StopPlayerOnSprintRelease()
        {
            // Check if sprint was just released
            if (m_WasSprintingLastFrame && !Input_Sprint)
            {
                m_StopOnSprintRelease = true;
                // Store the velocity at the moment sprint was released
                m_SprintVelocity = new Vector3(m_MovementDirection.x, 0, m_MovementDirection.z);
            }

            // Apply the stopping force if needed and not in a state change
            if (m_StopOnSprintRelease && !m_ResetMovementOnStateChange)
            {
                // Check if there's any directional input
                bool hasDirectionalInput = (Input_Movement.magnitude > 0.1f);

#if UNITY_EDITOR
                hasDirectionalInput = hasDirectionalInput || (External_Input_Movement.magnitude > 0.1f);
#endif

                if (!hasDirectionalInput)
                {
                    // No directional input, slow down to a complete stop
                    m_MovementDirection.x = Mathf.MoveTowards(m_MovementDirection.x, 0, m_Acceleration * 2f * Time.deltaTime);
                    m_MovementDirection.z = Mathf.MoveTowards(m_MovementDirection.z, 0, m_Acceleration * 2f * Time.deltaTime);

                    // Check if player has effectively stopped
                    if (Mathf.Abs(m_MovementDirection.x) < 0.01f && Mathf.Abs(m_MovementDirection.z) < 0.01f)
                    {
                        m_StopOnSprintRelease = false;
                    }
                }
                else
                {
                    // There is directional input, transition from sprint speed to walk speed
                    // Calculate current velocity magnitude
                    float currentSpeed = new Vector2(m_MovementDirection.x, m_MovementDirection.z).magnitude;

                    // Target walking speed
                    float targetSpeed = m_WalkSpeed;

                    // If current speed is greater than walk speed, smoothly transition down
                    if (currentSpeed > targetSpeed)
                    {
                        float speedRatio = Mathf.MoveTowards(currentSpeed, targetSpeed, m_Acceleration * 2f * Time.deltaTime) / currentSpeed;
                        m_MovementDirection.x *= speedRatio;
                        m_MovementDirection.z *= speedRatio;
                    }
                    else
                    {
                        // We've reached walking speed, stop the special handling
                        m_StopOnSprintRelease = false;
                    }
                }
            }

            // Update sprint state for next frame
            m_WasSprintingLastFrame = Input_Sprint;
        }
    }
}