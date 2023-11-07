using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


public class BeamIndexMapping : MonoBehaviour
{

    int numCol = 36;
    int numRows = 8;
    float minDegHorizontal = -60;
    float minDegVertical = -30;
    float degreeRangeHorizontal = 120;
    float degreeRangeVertical = 60;
    [SerializeField] private float degreeHorizontal = 0f;
    [SerializeField] private float degreeVertical = 0f;
    [SerializeField] private Material beamMaterial; 
    public bool move;
    private Coroutine currentTransitionCoroutine;

    private GameObject beam; //the beam GameObject
    private LineRenderer lineRenderer; //  the reference to tge LineRenderer

    private int beamIndex;

    void Start()
    {
        CreateBeamGameObject(); 
    }

     private void CreateBeamGameObject()
    {
        beam = new GameObject("Beam");
        beam.transform.parent = transform;
        beam.transform.position = transform.position;
        
        lineRenderer = beam.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 3f;
        lineRenderer.numCapVertices = 20;
        lineRenderer.material = beamMaterial;

        // Initially disable the beam
        beam.SetActive(false);
    }


    //     void Start()
    // {
    //     move = true;
    //     StartCoroutine(MoveRayEverySecond());
    // }

    // IEnumerator MoveRayEverySecond()
    // {
    //     if(move)
    //     {
    //         for (beamIndex = 25; beamIndex <= 312; beamIndex++)
    //         {
    //             MoveRayToBeamIndex(beamIndex);
    //             yield return new WaitForFixedUpdate();
    //         }
    //     }
    // }

    public void StartSmoothTransition(int newBeamIndex)
    {
        if (currentTransitionCoroutine != null)
    {
        StopCoroutine(currentTransitionCoroutine);
    }
        StartCoroutine(MoveRayToBeamIndexSmooth(newBeamIndex, 1f)); // 1 second transition
    }

        
 private IEnumerator MoveRayToBeamIndexSmooth(int newBeamIndex, float transitionTime)
    
    {
        Vector3 startingDirection = transform.rotation * Quaternion.Euler(degreeVertical, degreeHorizontal, 0) * Vector3.forward;

        beamIndex = newBeamIndex;
        float degreeIncreaseHorizontal = degreeRangeHorizontal / numCol;
        float degreeIncreaseVertical = degreeRangeVertical / numRows;

        int currentCol = (beamIndex - 25) % numCol;
        int currentRow = Mathf.FloorToInt((beamIndex - 25) / (float)numCol);

        // Calculate the horizontal and vertical angels
        degreeHorizontal = minDegHorizontal + currentCol * degreeIncreaseHorizontal;
        degreeVertical = minDegVertical + currentRow * degreeIncreaseVertical;

        // Calculate the target direction
        Vector3 targetDirection = transform.rotation * Quaternion.Euler(degreeVertical, degreeHorizontal, 0) * Vector3.forward;

        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpFactor = elapsedTime / transitionTime;

            // Smoothly interpolate the beam's direction
            Vector3 smoothedDirection = Vector3.Slerp(startingDirection, targetDirection, lerpFactor);

            // Apply the smoothed direction to draw the beam
            DrawBeam(smoothedDirection);

            yield return null;
        }

    DrawBeam(targetDirection);
}
    
    private void DrawBeam(Vector3 direction)
    {
        if (beam == null)
        {
            CreateBeamGameObject();
        }

        beam.SetActive(true);

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + direction * 10); 
        beam.transform.rotation = Quaternion.LookRotation(direction);
    }

    public void HideBeam()
    {
        if (beam != null)
        {
            beam.SetActive(false);
        }
    }

}
