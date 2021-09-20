using UnityEngine;
using System;
using System.Collections;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.Managers;

namespace BLINK.Controller
{
	[Serializable]
	public class MovementSettings
	{
		public float Acceleration = 25.0f; // In meters/second
		public float Decceleration = 25.0f;
		public float SprintAcceleration = 10.0f;
		public float SprintDecceleration = 60.0f;
		public float currentAcceleration;
		public float currentDecceleration;
		public float AccelerationLerpSpeed = 5;
		public float DeccelerationLerpSpeed = 5;
		public float AnimationSpeedSyncingSpeed = 4;
		public float DefaultMovementSpeed = 5;
		public float MaxHorizontalSpeed = 8.0f;
		public float SprintModifier = 1.5f; // In meters/second
		public float JumpSpeed = 10.0f; // In meters/second

		public float JumpAbortSpeed = 10.0f; // In meters/second
		//public LayerMask movingPlatformLayers;
	}

	[Serializable]
	public class GravitySettings
	{
		public float Gravity = 20.0f; // Gravity applied when the player is airborne
		public float GroundedGravity = 5.0f; // A constant gravity that is applied when the player is grounded
		public float MaxFallSpeed = 40.0f; // The max speed at which the player can fall
	}

	[Serializable]
	public class RotationSettings
	{
		[Header("Control Rotation")] public float MinPitchAngle = -45.0f;
		public float MaxPitchAngle = 75.0f;

		[Header("Character Orientation")] [SerializeField]
		private bool _useControlRotation = false;

		[SerializeField] private bool _orientRotationToMovement = true;
		public float MinRotationSpeed = 600.0f; // The turn speed when the player is at max speed (in degrees/second)
		public float MaxRotationSpeed = 1200.0f; // The turn speed when the player is stationary (in degrees/second)

		public bool UseControlRotation
		{
			get { return _useControlRotation; }
			set { SetUseControlRotation(value); }
		}

		public bool OrientRotationToMovement
		{
			get { return _orientRotationToMovement; }
			set { SetOrientRotationToMovement(value); }
		}

		private void SetUseControlRotation(bool useControlRotation)
		{
			_useControlRotation = useControlRotation;
			_orientRotationToMovement = !_useControlRotation;
		}

		private void SetOrientRotationToMovement(bool orientRotationToMovement)
		{
			_orientRotationToMovement = orientRotationToMovement;
			_useControlRotation = !_orientRotationToMovement;
		}
	}


	[Serializable]
	public class CameraSettings
	{
		public bool isAiming;
		public Vector3 normalPivot, aimingPivot;
		public float aimInSpeed, aimOutSpeed;

		public float NormalFOV = 60, SprintFOV = 70;
		public float FOVLerpSpeed = 5;
	}



	public class RPGBThirdPersonController : MonoBehaviour
	{
		public RPGBThirdPersonCharacterControllerEssentials ControllerEssentials;
		public Controller Controller; // The controller that controls the character
		public MovementSettings MovementSettings;
		public GravitySettings GravitySettings;
		public RotationSettings RotationSettings;
		public CameraSettings CameraSettings;

		private CharacterController _characterController; // The Unity's CharacterController

		public CharacterController getCharController()
		{
			return _characterController;
		}

		private CharacterAnimator _characterAnimator;

		private float _targetHorizontalSpeed; // In meters/second
		private float _horizontalSpeed; // In meters/second
		private float _verticalSpeed; // In meters/second

		private Vector2 _controlRotation; // X (Pitch), Y (Yaw)
		private Vector2 _controlXRotation; // X (Pitch), Y (Yaw)
		private Vector3 _movementInput;
		private Vector3 _lastMovementInput;
		private bool _hasMovementInput;
		private bool _jumpInput;

		public Vector3 Velocity => _characterController.velocity;
		public Vector3 HorizontalVelocity => _characterController.velocity.SetY(0.0f);
		public Vector3 VerticalVelocity => _characterController.velocity.Multiply(0.0f, 1.0f, 0.0f);
		public bool IsGrounded { get; private set; }

		public bool cameraCanRotate = true;
		public bool isSprinting;
		public bool isFlying;

		public PlayerCamera _playerCamera;
		private static readonly int moveSpeedModifier = Animator.StringToHash("MoveSpeedModifier");

		private void Start()
		{
			ControllerEssentials = GetComponent<RPGBThirdPersonCharacterControllerEssentials>();
			_playerCamera = FindObjectOfType<PlayerCamera>();

			Controller.Init();
			Controller.RpgbThirdPersonController = this;

			_characterController = GetComponent<CharacterController>();
			_characterAnimator = GetComponent<CharacterAnimator>();
		}

		private void Update()
		{
			if (CombatManager.playerCombatNode == null || !ControllerEssentials.controllerIsReady) return;
			Controller.OnCharacterUpdate();
		}

		/*private void FixedUpdate()
		{
			if(CombatManager.playerCombatInfo == null) return;
		}*/

		private void LateUpdate()
		{
			if (CombatManager.playerCombatNode == null || !ControllerEssentials.controllerIsReady) return;
			UpdateState();
			Controller.OnCharacterFixedUpdate();
		}

		private void UpdateState()
		{
			Vector3 movement;
			if (!ControllerEssentials.HasMovementRestrictions())
			{
				UpdateHorizontalSpeed();
				movement = _horizontalSpeed * GetMovementDirection() + _verticalSpeed * Vector3.up;
			}
			else
			{
				movement = _verticalSpeed * Vector3.up;
			}

			UpdateVerticalSpeed();
			_characterController.Move(movement * Time.deltaTime);
			OrientToTargetRotation(movement.SetY(0.0f));

			if (isFlying && !IsGrounded) RotateFlyingCharacter(_playerCamera.Pivot.localEulerAngles.x);

			IsGrounded = _characterController.isGrounded;

			_characterAnimator.UpdateState(ControllerEssentials.HasMovementRestrictions());
		}

		/*private GameObject isOnMovingPlatform()
		{
			RaycastHit hit;
			return Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z),
				-transform.up, out hit, Mathf.Infinity,
				MovementSettings.movingPlatformLayers) ? hit.collider.gameObject : null;
		}*/

		public void SetSpeed(float newSpeed)
		{
			MovementSettings.MaxHorizontalSpeed = newSpeed;
		}

		public void SetMovementInput(Vector3 movementInput)
		{
			bool hasMovementInput = movementInput.sqrMagnitude > 0.0f;

			if (_hasMovementInput && !hasMovementInput)
			{
				_lastMovementInput = _movementInput;
			}

			_movementInput = movementInput;
			_hasMovementInput = hasMovementInput;
		}

		public void SetJumpInput(bool jumpInput)
		{
			_jumpInput = jumpInput;
		}

		public Vector2 GetControlRotation()
		{
			return _controlRotation;
		}

		public void SetControlRotation(Vector2 controlRotation)
		{
			// Adjust the pitch angle (X Rotation)
			float pitchAngle = controlRotation.x;
			pitchAngle %= 360.0f;
			pitchAngle = Mathf.Clamp(pitchAngle, RotationSettings.MinPitchAngle, RotationSettings.MaxPitchAngle);

			// Adjust the yaw angle (Y Rotation)
			float yawAngle = controlRotation.y;
			yawAngle %= 360.0f;

			_controlRotation = new Vector2(pitchAngle, yawAngle);
		}

		private void UpdateHorizontalSpeed()
		{
			Vector3 movementInput = _movementInput;
			if (movementInput.sqrMagnitude > 1.0f)
			{
				movementInput.Normalize();
			}

			float currentSpeed = GetCurrentSpeed();
			_targetHorizontalSpeed = movementInput.magnitude * currentSpeed;

			UpdateAcceleration();
			UpdateDecceleration();
			float acceleration = _hasMovementInput
				? MovementSettings.currentAcceleration
				: MovementSettings.currentDecceleration;

			float speedMod = currentSpeed / MovementSettings.DefaultMovementSpeed;
			ControllerEssentials.anim.SetFloat(moveSpeedModifier,
				Mathf.Lerp(ControllerEssentials.anim.GetFloat(moveSpeedModifier), speedMod,
					Time.deltaTime * (acceleration / MovementSettings.AnimationSpeedSyncingSpeed)));

			_horizontalSpeed =
				Mathf.MoveTowards(_horizontalSpeed, _targetHorizontalSpeed, acceleration * Time.deltaTime);
		}

		private float GetCurrentSpeed()
		{
			return isSprinting
				? MovementSettings.MaxHorizontalSpeed * MovementSettings.SprintModifier
				: MovementSettings.MaxHorizontalSpeed;
		}

		private void UpdateAcceleration()
		{
			MovementSettings.currentAcceleration = Mathf.Lerp(
				MovementSettings.currentAcceleration,
				isSprinting ? MovementSettings.SprintAcceleration : MovementSettings.Acceleration,
				Time.deltaTime * MovementSettings.AccelerationLerpSpeed);
		}

		private void UpdateDecceleration()
		{
			MovementSettings.currentDecceleration = Mathf.Lerp(
				MovementSettings.currentDecceleration,
				isSprinting ? MovementSettings.SprintDecceleration : MovementSettings.Decceleration,
				Time.deltaTime * MovementSettings.DeccelerationLerpSpeed);
		}

		private void UpdateVerticalSpeed()
		{
			if (!isFlying)
			{
				if (IsGrounded)
				{
					_verticalSpeed = -GravitySettings.GroundedGravity;

					if (_jumpInput)
					{
						_verticalSpeed = MovementSettings.JumpSpeed;
						IsGrounded = false;
					}
				}
				else
				{
					if (!_jumpInput && _verticalSpeed > 0.0f)
					{
						// This is what causes holding jump to jump higher than tapping jump.
						_verticalSpeed = Mathf.MoveTowards(_verticalSpeed, -GravitySettings.MaxFallSpeed,
							MovementSettings.JumpAbortSpeed * Time.deltaTime);
					}

					_verticalSpeed = Mathf.MoveTowards(_verticalSpeed, -GravitySettings.MaxFallSpeed,
						GravitySettings.Gravity * Time.deltaTime);
				}
			}
			else
			{
				if (IsGrounded)
				{
					_verticalSpeed = -GravitySettings.GroundedGravity;

					if (_jumpInput)
					{
						_verticalSpeed = MovementSettings.JumpSpeed;
						IsGrounded = false;
					}
				}
				else
				{
					if (_jumpInput)
					{
						_verticalSpeed = GetCurrentSpeed();
					}
					else
					{
						_verticalSpeed = 0;
					}
				}
			}
		}

		private Vector3 GetMovementDirection()
		{
			Vector3 moveDir = _hasMovementInput ? _movementInput : _lastMovementInput;
			if (moveDir.sqrMagnitude > 1f)
			{
				moveDir.Normalize();
			}

			return moveDir;
		}

		private void OrientToTargetRotation(Vector3 horizontalMovement)
		{
			if (RotationSettings.OrientRotationToMovement && horizontalMovement.sqrMagnitude > 0.0f)
			{
				float rotationSpeed = Mathf.Lerp(
					RotationSettings.MaxRotationSpeed, RotationSettings.MinRotationSpeed,
					_horizontalSpeed / _targetHorizontalSpeed);

				Quaternion targetRotation = Quaternion.LookRotation(horizontalMovement, Vector3.up);

				transform.rotation =
					Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
			}
			else if (RotationSettings.UseControlRotation)
			{
				Quaternion targetRotation = Quaternion.Euler(0.0f, _controlRotation.y, 0.0f);
				transform.rotation = targetRotation;
			}
		}

		private void RotateFlyingCharacter(float RotX)
		{
			if (!RotationSettings.OrientRotationToMovement) return;
			float rotationSpeed = Mathf.Lerp(
				RotationSettings.MaxRotationSpeed, RotationSettings.MinRotationSpeed,
				_horizontalSpeed / _targetHorizontalSpeed);

			var localEulerAngles = transform.localEulerAngles;
			float newXRot = Mathf.Lerp(localEulerAngles.x, RotX, rotationSpeed * Time.deltaTime);
			localEulerAngles = new Vector3(newXRot, localEulerAngles.y, localEulerAngles.z);
			if (localEulerAngles.sqrMagnitude > 0.0f) transform.localEulerAngles = localEulerAngles;
		}
	}
}
