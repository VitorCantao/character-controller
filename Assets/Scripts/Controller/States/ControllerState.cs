using UnityEngine;

namespace Controller.States
{
	public abstract class ControllerState : MonoBehaviour
	{
		protected SphereController Controller;
		protected InputHandler InputHandler;
		protected StatesSettings Settings;

		public void Initialize(SphereController controller, InputHandler inputHandler, StatesSettings settings)
		{
			Controller = controller;
			InputHandler = inputHandler;
			Settings = settings;
		}

		/// <summary> Check if the State transition should occur. </summary>
		public virtual bool CheckStateEnter() => true;
		public virtual void OnStateEnter() {}
		public virtual void OnStateUpdate() {}
		
		/// <summary> Called every fixed update after the controller internal state update. </summary>
		public virtual void OnStateFixedUpdate() {}
		public virtual void OnStateLateUpdate() {}
		public virtual void OnStateExit() {}
	}
}
