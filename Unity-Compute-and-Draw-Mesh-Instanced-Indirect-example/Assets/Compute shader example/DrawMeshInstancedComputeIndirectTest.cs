using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EXPToolkit;

public class DrawMeshInstancedComputeIndirectTest : MonoBehaviour
{
    
    public int m_InstCount = 1000;
    public Mesh m_InstMesh;
    public Material m_InstMat;

    // Transform pattern is used to define how the transforms are positioned 
    public TransformPattern m_Pattern;

    int ThreadGroupCount { get { return m_InstCount; } }
    int kernel;

    #region Compute buffers  
    // The struct we will be passing to the shaders
    struct ComputeTransform
    {
        public Vector4 position;
        public Vector4 rotation;
        public Vector4 scale;
    }
    public ComputeShader _compute;

    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    // List of compute buffers so its easy to release all at once
    List<ComputeBuffer> m_ComputeBuffers = new List<ComputeBuffer>();

    private ComputeBuffer m_BaseTransformBuffer;
    private ComputeBuffer m_ModifiedTransformBuffer;

    private ComputeBuffer m_PosCurveBuffer;
    private ComputeBuffer m_RotCurveBuffer;
    private ComputeBuffer m_ScaleCurveBuffer;
    #endregion
        
    #region Curves
    [Header("Stroke approximation curves offsets")]
    public AnimationCurve m_RotCurve;
    public AnimationCurve m_ScaleCurve;
    public Transform m_Anchor0, m_Anchor1;

    [Header("Position offsets")]
    public AnimationCurve m_PosXCurve;
    public AnimationCurve m_PosYCurve;
    public AnimationCurve m_PosZCurve;

    [Header("Rotation offsets")]
    public AnimationCurve m_RotXCurve;
    public AnimationCurve m_RotYCurve;
    public AnimationCurve m_RotZCurve;

    [Header("Scale offsets")]
    public AnimationCurve m_ScaleXCurve;
    public AnimationCurve m_ScaleYCurve;
    public AnimationCurve m_ScaleZCurve;

    [Header("Curve offset speedss")]
    public Vector3 m_PosCurveSpeeds = Vector3.one;
    public Vector3 m_RotCurveSpeeds = Vector3.one;
    public Vector3 m_ScaleCurveSpeeds = Vector3.one;

    // whether to update thte curves every frame or just once at start
    public bool m_UpdateCurvesEachFrame = false;
    #endregion
    
    void Awake()
    {
        _compute = Instantiate(_compute);

      //  m_Pattern = GetComponent<TransformPattern>();

        if (m_Pattern)
        {
            m_Pattern.CreateTransforms();
            m_InstCount = m_Pattern.Transforms.Count;
        }

        UpdateBuffers();

        #region Compute shader
        // Get the compute kernal and set the position buffer reference
        kernel = _compute.FindKernel("TransformUpdate");
        _compute.SetBuffer(kernel, "BaseTransformBuffer", m_BaseTransformBuffer);
        _compute.SetBuffer(kernel, "ModifiedTransformBuffer", m_ModifiedTransformBuffer);
        _compute.SetBuffer(kernel, "PositionCurve", m_PosCurveBuffer);
        _compute.SetBuffer(kernel, "RotationCurve", m_RotCurveBuffer);
        _compute.SetBuffer(kernel, "ScaleCurve", m_ScaleCurveBuffer);

        _compute.SetFloat("InstanceCount", (float)m_InstCount);

        _compute.Dispatch(kernel, ThreadGroupCount, 1, 1);
        #endregion

        #region Instance shader    
        m_InstMat = new Material(m_InstMat);      
        m_InstMat.name += "(Clone)";
        m_InstMat.SetBuffer("ModifiedTransformBuffer", m_ModifiedTransformBuffer);
        m_InstMat.SetBuffer("BaseTransformBuffer", m_BaseTransformBuffer);
        #endregion
    }

    void Update()
    {
        if (m_UpdateCurvesEachFrame) UpdateCurves();

        // Update the position buffer.
        _compute.SetFloat("Time", Time.time);
        var kernel = _compute.FindKernel("TransformUpdate");        

        _compute.Dispatch(kernel, ThreadGroupCount, 1, 1);

        // Render
        Graphics.DrawMeshInstancedIndirect(m_InstMesh, 0, m_InstMat, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }



    void UpdateBuffers()
    {
        m_ComputeBuffers.Clear();

        m_InstMat.SetFloat("_count", (float)m_InstCount);

        ReleaseBuffers();

        #region Create buffers
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        // compute pos buffer     
        m_BaseTransformBuffer = new ComputeBuffer(m_InstCount, 16 * 3);
        m_ModifiedTransformBuffer = new ComputeBuffer(m_InstCount, 16 * 3);
        
        m_PosCurveBuffer = new ComputeBuffer(m_InstCount, 16);
        m_RotCurveBuffer = new ComputeBuffer(m_InstCount, 16);
        m_ScaleCurveBuffer = new ComputeBuffer(m_InstCount, 16);
        
        m_ComputeBuffers.Add(m_BaseTransformBuffer);      
        m_ComputeBuffers.Add(m_ModifiedTransformBuffer);     
        m_ComputeBuffers.Add(m_PosCurveBuffer);
        m_ComputeBuffers.Add(m_RotCurveBuffer);
        m_ComputeBuffers.Add(m_ScaleCurveBuffer);
        #endregion

        #region Fill initial transforms
        Vector4[] positions = new Vector4[m_InstCount];
        Vector4[] rotations = new Vector4[m_InstCount];

        ComputeTransform[] computeTforms = new ComputeTransform[m_InstCount];

        for (int i = 0; i < m_InstCount; i++)
        {
            float norm = (float)i / (float)m_InstCount;
            
            Vector3 pos;
            if (m_Pattern != null)            
                pos = m_Pattern.Transforms[i].position;            
            else
                pos = Vector3.Lerp(m_Anchor0.position, m_Anchor1.position, norm);

            // Scale
            float scale = m_ScaleCurve.Evaluate(norm);

            if (m_Pattern != null)
            {
                scale = m_Pattern.Transforms[i].localScale.x;
            }

            positions[i] = new Vector4(pos.x, pos.y, pos.z, scale);

            // Rot         
            Quaternion q = Quaternion.identity;       
           
            if (m_Pattern != null)            
                q = m_Pattern.Transforms[i].rotation;            
            else
                q = Quaternion.LookRotation(m_Anchor1.position - m_Anchor0.position);

            Vector4 rot = new Vector4(q.w, q.x, q.y, q.z);
          

            computeTforms[i] = new ComputeTransform();
            computeTforms[i].position = pos;
            computeTforms[i].rotation = rot;
            computeTforms[i].scale = Vector3.one * scale;

        }
        #endregion
  
        // Set buffers
        m_BaseTransformBuffer.SetData(computeTforms);
        m_ModifiedTransformBuffer.SetData(computeTforms);        

        UpdateCurves();

        // indirect args
        uint numIndices = (m_InstMesh != null) ? (uint)m_InstMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)m_InstCount;
        argsBuffer.SetData(args);
    }
    
    
    void UpdateCurves()
    {
        Vector4[] m_posCurves = new Vector4[m_InstCount];
        Vector4[] m_rotCurves = new Vector4[m_InstCount];
        Vector4[] m_scaleCurves = new Vector4[m_InstCount];        

        for (int i = 0; i < m_InstCount; i++)
        {
            float norm = (float)i / (float)m_InstCount;

            // Offset curves
            m_posCurves[i] = new Vector4(m_PosXCurve.Evaluate(norm), m_PosYCurve.Evaluate(norm), m_PosZCurve.Evaluate(norm), 0);
            m_rotCurves[i] = new Vector4(m_RotXCurve.Evaluate(norm) * 360 * Mathf.Deg2Rad, m_RotYCurve.Evaluate(norm) * 360 * Mathf.Deg2Rad, m_RotZCurve.Evaluate(norm) * 360 * Mathf.Deg2Rad, 0);
            m_scaleCurves[i] = new Vector4(m_ScaleXCurve.Evaluate(norm), m_ScaleYCurve.Evaluate(norm), m_ScaleZCurve.Evaluate(norm), 0);
        }

        // Store speeds in spare axis
        m_posCurves[0].w = m_PosCurveSpeeds.x;
        m_posCurves[1].w = m_PosCurveSpeeds.y;
        m_posCurves[2].w = m_PosCurveSpeeds.z;

        m_rotCurves[0].w = m_RotCurveSpeeds.x;
        m_rotCurves[1].w = m_RotCurveSpeeds.y;
        m_rotCurves[2].w = m_RotCurveSpeeds.z;

        m_scaleCurves[0].w = m_ScaleCurveSpeeds.x;
        m_scaleCurves[1].w = m_ScaleCurveSpeeds.y;
        m_scaleCurves[2].w = m_ScaleCurveSpeeds.z;


        m_PosCurveBuffer.SetData(m_posCurves);
        m_RotCurveBuffer.SetData(m_rotCurves);
        m_ScaleCurveBuffer.SetData(m_scaleCurves);

        _compute.SetBuffer(kernel, "PositionCurve", m_PosCurveBuffer);
        _compute.SetBuffer(kernel, "RotationCurve", m_RotCurveBuffer);
        _compute.SetBuffer(kernel, "ScaleCurve", m_ScaleCurveBuffer);
    }
    
    void ReleaseBuffers()
    {
        for (int i = 0; i < m_ComputeBuffers.Count; i++)
            m_ComputeBuffers[i].Release();
    }

    void OnEnable()
    {
        UpdateBuffers();
    }

    void OnDisable()
    {
        ReleaseBuffers();  
    }
}

