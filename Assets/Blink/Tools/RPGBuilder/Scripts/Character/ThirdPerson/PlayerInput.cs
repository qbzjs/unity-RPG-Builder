using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.Controller
{
	public class PlayerInput : MonoBehaviour
	{
		public float MoveAxisDeadZone = 0.2f;
		public bool useNewKeys;
		public float smoothTime = 1, dampenTime = 0.2f;
		public float smoothInput = 1;

		public Vector2 MoveInput { get; private set; }
		public Vector2 LastMoveInput { get; private set; }
		public Vector2 CameraInput { get; private set; }
		public bool JumpInput { get; set; }
		public bool HasMoveInput { get; private set; }
		public Vector2 lastMoveInput;

		public void UpdateInput()
		{
			// Update MoveInput
			Vector2 moveInput = new Vector2(0, 0);
			if (!useNewKeys)
			{
				moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			}
			else
			{
				KeyCode moveForwardKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveForward");
				KeyCode moveBackwardKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveBackward");
				KeyCode moveLeftKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveLeft");
				KeyCode moveRightKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveRight");


				if (Input.GetKey(moveForwardKey))
				{
					moveInput.y = 1;
				}

				if (Input.GetKey(moveBackwardKey))
				{
					moveInput.y = -1;
				}

				if (Input.GetKey(moveLeftKey))
				{
					moveInput.x = 1;
				}

				if (Input.GetKey(moveRightKey))
				{
					moveInput.x = -1;
				}
			}

			if (Mathf.Abs(moveInput.x) < MoveAxisDeadZone)
			{
				moveInput.x = 0.0f;
			}

			if (Mathf.Abs(moveInput.y) < MoveAxisDeadZone)
			{
				moveInput.y = 0.0f;
			}

			if (useNewKeys && moveInput.sqrMagnitude > 0.0f)
			{
				moveInput = Vector2.Lerp(lastMoveInput, moveInput, Time.deltaTime * smoothInput);
			}

			lastMoveInput = moveInput;

			bool hasMoveInput = moveInput.sqrMagnitude > 0.0f;

			if (HasMoveInput && !hasMoveInput)
			{
				LastMoveInput = MoveInput;
			}

			if (!useNewKeys)
			{
				CombatManager.playerCombatNode.playerControllerEssentials.anim.SetFloat("MoveDirectionX", moveInput.x);
				CombatManager.playerCombatNode.playerControllerEssentials.anim.SetFloat("MoveDirectionY", moveInput.y);
			}
			else
			{
				CombatManager.playerCombatNode.playerControllerEssentials.anim.SetFloat("MoveDirectionX", moveInput.x,
					dampenTime, Time.deltaTime * smoothTime);
				CombatManager.playerCombatNode.playerControllerEssentials.anim.SetFloat("MoveDirectionY", moveInput.y,
					dampenTime, Time.deltaTime * smoothTime);
			}

			MoveInput = moveInput;
			HasMoveInput = hasMoveInput;

			// Update other inputs
			CameraInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

			JumpInput = Input.GetKey(RPGBuilderUtilities.GetCurrentKeyByActionKeyName("Jump"));
		}
	}
}
