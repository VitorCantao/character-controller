using UnityEngine;

namespace Gravity
{
    public class GravityPlane : GravitySource
    {
        [SerializeField] private Vector3 size;
        [SerializeField] private float gravity = 9.81f;

        public override Vector3 GetGravity(Vector3 position)
        {
            position -= transform.position;
        
            if (Mathf.Abs(position.x) > size.x / 2 
                || Mathf.Abs(position.y) > size.y * 2 
                || Mathf.Abs(position.z) > size.z / 2)
                return Vector3.zero;
        
            Vector3 up = transform.up;

            float g = -gravity;

            return g * up;
        }
    

        private void OnDrawGizmos()
        {
            Vector3 scale = transform.localScale;
            scale.y = size.y;

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
            Gizmos.color = Color.yellow;
        
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
}
