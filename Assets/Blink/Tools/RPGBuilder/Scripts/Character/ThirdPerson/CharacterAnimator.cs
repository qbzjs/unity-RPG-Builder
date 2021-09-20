using UnityEngine;
using UnityEngine.SceneManagement;

	public static class CharacterAnimatorParamId
	{
		public static readonly int HorizontalSpeed = Animator.StringToHash("HorizontalSpeed");
		public static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
		public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
	}

namespace BLINK.Controller
{
	public class CharacterAnimator : MonoBehaviour
	{
		private Animator _animator;
		private RPGBThirdPersonController rpgbThirdPersonController;

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			rpgbThirdPersonController = GetComponent<RPGBThirdPersonController>();
		}

		public void UpdateState(bool hasMovementRestriction)
		{
			if (hasMovementRestriction)
			{
				_animator.SetFloat(CharacterAnimatorParamId.HorizontalSpeed, 0);
			}
			else
			{
				float normHorizontalSpeed = rpgbThirdPersonController.HorizontalVelocity.magnitude /
				                            rpgbThirdPersonController.MovementSettings.MaxHorizontalSpeed;
				_animator.SetFloat(CharacterAnimatorParamId.HorizontalSpeed, normHorizontalSpeed);
			}

			float jumpSpeed = rpgbThirdPersonController.MovementSettings.JumpSpeed;
			float normVerticalSpeed =
				rpgbThirdPersonController.VerticalVelocity.y.Remap(-jumpSpeed, jumpSpeed, -1.0f, 1.0f);
			_animator.SetFloat(CharacterAnimatorParamId.VerticalSpeed, normVerticalSpeed);
			_animator.SetBool(CharacterAnimatorParamId.IsGrounded, rpgbThirdPersonController.IsGrounded);
		}
	}
}

