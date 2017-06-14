using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EXPToolkit
{
    /// <summary>
    /// Creates a pattern of transforms. i.e circle, sphere, grid, 
    /// </summary>
    public class TransformPattern : MonoBehaviour
    {
        protected List<Transform> m_Transforms = new List<Transform>();
        public List<Transform> Transforms { get { return m_Transforms; } }

        public float m_BaseScale = .1f;

        public virtual void CreateTransforms()
        {
            // remove any existing children
            GameObjectExtensions.DestroyAllChildren(gameObject);

            m_Transforms.Clear();
        }

        void ReorderByY()
        {

        }
        
        private void OnDrawGizmos()
        {
            for (int i = 0; i < m_Transforms.Count; i++)
            {
                Gizmos.DrawSphere(m_Transforms[i].position, m_BaseScale/2f);
                Gizmos.DrawLine(m_Transforms[i].position, m_Transforms[i].position + ( m_Transforms[i].TransformDirection(Vector3.up) * m_BaseScale * 3 ));
                i++;
                i++;
            }            
        }
    }
}
