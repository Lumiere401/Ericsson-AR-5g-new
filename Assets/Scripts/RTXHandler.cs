using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RTXHandler : MonoBehaviour
{
    public ComputeShader rayTraceShader;
    private ComputeBuffer resultBuffer;
    private ComputeBuffer rayInputBuffer;
    private ComputeBuffer vertexBuffer;
    private ComputeBuffer indexBuffer;
    
    private Vector3[] results;
    private int kernelIndex;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] indices;
    
    struct RayData
    {
        public Vector3 origin;
        public Vector3 direction;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // Find the compute shader
        kernelIndex = rayTraceShader.FindKernel("RTXMain");

        // Initialize buffer to store results
        resultBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(RayData))); // Assuming 2 Vector3s
        rayTraceShader.SetBuffer(0, "HitDataBuffer", resultBuffer);
        
        // Get the mesh geometry
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        indices = mesh.GetIndices(0);
        int vertexCount = vertices.Length;
        int indexCount = indices.Length;
        
        // Initialize vertex and index buffers
        vertexBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);
        vertexBuffer.SetData(vertices);
        indexBuffer = new ComputeBuffer(indexCount, sizeof(int));
        indexBuffer.SetData(indices);
        rayTraceShader.SetInt("vertexCount", vertexCount);
        rayTraceShader.SetInt("indexCount", indexCount);
        
        // Send data to kernel/shader
        rayTraceShader.SetBuffer(kernelIndex, "Vertices", vertexBuffer);
        rayTraceShader.SetBuffer(kernelIndex, "Indices", indexBuffer);
    }

    void OnDestroy()
    {
        // Release the buffers when the script or GameObject is destroyed
        resultBuffer.Release();
        vertexBuffer.Release();
        indexBuffer.Release();
        
        // This one may not be initialized
        rayInputBuffer?.Release();

    }

    public Vector3[] getResult(Vector3 origin, Vector3 direction)
    {
        // If a prior buffer was sent
        rayInputBuffer?.Release();

        // Prepare input data for RTX
        RayData ray = new RayData
        {
            origin = origin,
            direction = direction
        };
        
        rayInputBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(RayData)));
        rayInputBuffer.SetData(new RayData[] { ray });

        // Set the input data buffer for the intended compute shader
        rayTraceShader.SetBuffer(kernelIndex, "RayDataBuffer", rayInputBuffer);
        
        // Dispatch shader
        rayTraceShader.Dispatch(kernelIndex, 1, 1, 1);

        // Retrieve data
        results = new Vector3[2];
        resultBuffer.GetData(results);
        
        return results;
    }
}
