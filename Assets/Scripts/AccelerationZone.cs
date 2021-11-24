using Controller;
using Controller.States.SuperStates;
using UnityEngine;

public class AccelerationZone : MonoBehaviour
{
	[SerializeField, Min(0f)] private float acceleration, speed = 10f;

	private void OnTriggerEnter(Collider other)
	{
		Rigidbody body = other.attachedRigidbody;
		
		if (body)
		{
			Accelerate(body);
		}
	}
	
	private void OnTriggerStay(Collider other)
	{
		Rigidbody body = other.attachedRigidbody;
		
		if (body)
		{
			Accelerate(body);
		}
	}

	private void Accelerate(Rigidbody body)
	{
		Vector3 velocity = transform.InverseTransformDirection(body.velocity);

		if (velocity.y >= speed) return;

		if (acceleration > 0f)
		{
			velocity.y = Mathf.MoveTowards(velocity.y, speed, acceleration * Time.deltaTime);
		}
		else
		{
			velocity.y = speed;
		}
		
		if (body.TryGetComponent(out SphereController sphere))
		{
			sphere.PreventSnapToGround();
		}
		
		body.velocity = transform.TransformDirection(velocity);
	}
}
