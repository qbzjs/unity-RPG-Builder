using BLINK.RPGBuilder.Character;
using UnityEngine;

namespace BLINK.Controller
{
	public abstract class Controller : ScriptableObject
	{
		public RPGBThirdPersonController RpgbThirdPersonController { get; set; }
		public abstract void Init();
		public abstract void OnCharacterUpdate();
		public abstract void OnCharacterFixedUpdate();
	}
}
