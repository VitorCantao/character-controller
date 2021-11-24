using JetBrains.Annotations;
using UnityEngine;

namespace Controller
{
	public class ConnectedBodyHandler
	{
		public Vector3 Velocity { get; private set; }
		public Vector3 AngularVelocity => body != null ? body.angularVelocity : Vector3.zero;

		public bool IsConnected => body != null && body == previousBody;

		private Vector3 worldPosition;
		private Vector3 localPosition;

		[CanBeNull] private Rigidbody body;
		[CanBeNull] private Rigidbody previousBody;
	
		private readonly Rigidbody controllerBody;
	
		public ConnectedBodyHandler(Rigidbody controllerBody)
		{
			this.controllerBody = controllerBody;
		}
	
		public void Connect(Rigidbody newBody) => body = newBody;
	
		public void UpdateState()
		{
			if (body == null) return;

			if (!body.isKinematic && controllerBody.mass < body.mass) return;

			if (body == previousBody)
			{
				Vector3 movement = body.transform.TransformPoint(localPosition)
				                   - worldPosition;
				Velocity = movement / Time.deltaTime;
			}
			
			worldPosition = controllerBody.position;
			localPosition = body.transform.InverseTransformPoint(worldPosition);
		}	

		public void ClearState()
		{
			Velocity = Vector3.zero;
			previousBody = body;
			body = null;
		}
	}
}