using System.Collections.Generic;
using UnityEngine;

namespace Gravity
{
	public static class CustomGravity
	{
		private static readonly List<GravitySource> Sources = new List<GravitySource>();

		public static void Register(GravitySource source)
		{
			Debug.Assert(
				!Sources.Contains(source),
				"Duplicate registration of gravity source!", 
				source);
		
			Sources.Add(source);
		}

		public static void Unregister(GravitySource source)
		{
			Debug.Assert(
				Sources.Contains(source),
				"Deregistration of unknown gravity source!", 
				source);

			Sources.Remove(source);
		}
	
		public static Vector3 GetGravity(Vector3 position)
		{
			Vector3 gravity = Vector3.zero;
			
			foreach (GravitySource source in Sources)
			{
				gravity += source.GetGravity(position);
			}
		
			return gravity;
		}

		public static Vector3 GetUpAxis(Vector3 position)
		{
			return -GetGravity(position).normalized;
		}
	
		public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
		{
			Vector3 gravity = GetGravity(position);
			upAxis = -gravity.normalized;
		
			return gravity;
		}
	}
}
