using UnityEngine;

namespace Gravity
{
	public class GravitySphere : GravitySource
	{
		[SerializeField, Min(0f)] private float innerFalloffRadius = 1f;
		[SerializeField, Min(0f)] private float innerRadius = 5f;
	
		[SerializeField, Min(0f)] private float outerFalloffRadius = 15f;
		[SerializeField, Min(0f)] private float outerRadius = 10f;

		[SerializeField] private float gravity = 9.81f;

		private float outerFalloffFactor;
		private float innerFalloffFactor;
	
		public override Vector3 GetGravity(Vector3 position)
		{
			Vector3 vector = transform.position - position;
			float distance = vector.magnitude;
		
			if (distance > outerFalloffRadius || distance < innerFalloffRadius) {
				return Vector3.zero;
			}
		
			float g = gravity / distance;

			if (distance > outerRadius)
			{
				g *= 1 - (distance - outerRadius) * outerFalloffFactor;
			}
			else if (distance < innerRadius)
			{
				g *= 1f - (innerRadius - distance) * innerFalloffFactor;
			}
		
			return vector.normalized * g;
		}

		private void Awake()
		{
			OnValidate();
		}

		private void OnValidate()
		{
			innerFalloffRadius = Mathf.Max(innerFalloffRadius, 0);
		
			innerRadius = Mathf.Max(innerRadius, innerFalloffRadius);
			outerRadius = Mathf.Max(outerRadius, innerRadius);
		
			outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);

			innerFalloffFactor = 1f / (innerRadius - innerFalloffRadius);
			outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
		}

		private void OnDrawGizmos()
		{
			Vector3 position = transform.position;

			if (innerFalloffRadius > 0f && innerFalloffRadius < innerRadius)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(position, innerFalloffRadius);
			}
		
			Gizmos.color = Color.yellow;
			if (innerRadius > 0f && innerRadius < outerRadius)
			{
				Gizmos.DrawWireSphere(position, innerRadius);
			}
		
			Gizmos.DrawWireSphere(position, outerRadius);
		
			if (outerFalloffRadius > outerRadius)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(position, outerFalloffRadius);
			}
		}
	}
}
