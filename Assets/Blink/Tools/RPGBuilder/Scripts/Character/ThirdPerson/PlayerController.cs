using BLINK.RPGBuilder.Character;
using UnityEngine;

namespace BLINK.Controller
{
//[CreateAssetMenu(fileName = "PlayerController", menuName = "NaughtyCharacter/PlayerController")]
	public class PlayerController : Controller
	{
		public float ControlRotationSensitivity = 3.0f;

		private PlayerInput _playerInput;
		private PlayerCamera _playerCamera;

		public override void Init()
		{
			_playerInput = FindObjectOfType<PlayerInput>();
			_playerCamera = FindObjectOfType<PlayerCamera>();
		}

		public override void OnCharacterUpdate()
		{
			if (!RpgbThirdPersonController.ControllerEssentials.HasMovementRestrictions())
			{
				_playerInput.UpdateInput();
			}
			else
			{
				_playerInput.JumpInput = false;
				RpgbThirdPersonController.SetJumpInput(false);
			}

			if (RpgbThirdPersonController.cameraCanRotate &&
			    !RpgbThirdPersonController.ControllerEssentials.HasRotationRestrictions())
			{
				UpdateControlRotation();
			}

			if (!RpgbThirdPersonController.ControllerEssentials.HasMovementRestrictions())
			{
				RpgbThirdPersonController.SetMovementInput(GetMovementInput());
				RpgbThirdPersonController.SetJumpInput(_playerInput.JumpInput);
			}
		}

		public override void OnCharacterFixedUpdate()
		{
			_playerCamera.SetPosition(RpgbThirdPersonController.transform.position);
			if (!RpgbThirdPersonController.cameraCanRotate) return;
			_playerCamera.SetControlRotation(RpgbThirdPersonController.GetControlRotation());
		}

		private void UpdateControlRotation()
		{
			Vector2 camInput = _playerInput.CameraInput;
			Vector2 controlRotation = RpgbThirdPersonController.GetControlRotation();

			// Adjust the pitch angle (X Rotation)
			float pitchAngle = controlRotation.x;
			pitchAngle -= camInput.y * ControlRotationSensitivity;

			// Adjust the yaw angle (Y Rotation)
			float yawAngle = controlRotation.y;
			yawAngle += camInput.x * ControlRotationSensitivity;

			controlRotation = new Vector2(pitchAngle, yawAngle);
			RpgbThirdPersonController.SetControlRotation(controlRotation);
		}

		private Vector3 GetMovementInput()
		{
			// Calculate the move direction relative to the character's yaw rotation
			Quaternion yawRotation = new Quaternion();
			if (!RpgbThirdPersonController.isFlying)
			{
				yawRotation = Quaternion.Euler(0.0f, RpgbThirdPersonController.GetControlRotation().y, 0.0f);
			}
			else
			{
				yawRotation = Quaternion.Euler(RpgbThirdPersonController._playerCamera.Pivot.localEulerAngles.x,
					RpgbThirdPersonController.GetControlRotation().y, 0.0f);
			}

			Vector3 forward = yawRotation * Vector3.forward;
			Vector3 right = yawRotation * Vector3.right;
			Vector3 movementInput = (forward * _playerInput.MoveInput.y + right * _playerInput.MoveInput.x);

			if (movementInput.sqrMagnitude > 1f)
			{
				movementInput.Normalize();
			}

			return movementInput;
		}
	}
}