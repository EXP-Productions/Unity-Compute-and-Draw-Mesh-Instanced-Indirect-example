using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EXPToolkit
{
    /// <summary>
    /// Creates a pattern of transforms. i.e circle, sphere, grid, 
    /// </summary>
    public class TransformPattern_Circle : TransformPattern
    {
        public float m_Radius = 1;
        public int m_Count = 20;
        public float m_StartAngle = 0;
        public float m_EndAngle = 360;        

        [ContextMenu("Create")]
        public override void CreateTransforms()
        {
            base.CreateTransforms();

            bool fullCircle = Mathf.Abs(m_StartAngle - m_EndAngle) == 360;

            for (int i = 0; i < m_Count; i++)
            {
                float norm = i / (float)(m_Count - 1);
                if(fullCircle) norm = i / (float)m_Count;

                Vector3 pos = Vector3.zero;
                float angle = Mathf.Lerp(m_StartAngle, m_EndAngle, norm) * Mathf.Deg2Rad;
                pos.x = Mathf.Sin(angle) * m_Radius;
                pos.y = Mathf.Cos(angle) * m_Radius;

                Transform newT = new GameObject().transform;
                newT.name = i.ToString();
                newT.SetParent(transform);
                newT.localPosition = pos + transform.position;

                Quaternion rotation = Quaternion.Euler(0, 0, -angle * Mathf.Rad2Deg);
                newT.localRotation = rotation;

                newT.localScale = Vector3.one * m_BaseScale;

                m_Transforms.Add(newT);
            }
        }
    }
}
