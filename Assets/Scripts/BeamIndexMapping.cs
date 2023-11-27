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
    [SerializeField] private float oldDegreeHorizontal = 0f;
    [SerializeField] private float oldDegreeVertical = 0f;
    [SerializeField] private Material beamMaterial; 
    public bool move;
    private Coroutine currentTransitionCoroutine;

    private GameObject beam; //the beam GameObject
    private LineRenderer lineRenderer; //  the reference to tge LineRenderer
    private RTXHandler handle;
    private int beamIndex;
    private bool isTransitionActive = false;

    // Queue to store pending beam indices
    private Queue<int> beamIndexQueue = new Queue<int>();



    void Start()
    {
        CreateBeamGameObject(); 
        handle = GameObject.Find("cone").GetComponent<RTXHandler>();
    }

    private void Update()
    {
        // Create a ray from the specified position and direction
        Ray ray = new Ray(transform.position, transform.forward);
        Debug.Log("Shoot out: " + transform.position);
        // Visualize the ray in the Scene view
        Debug.DrawRay(transform.position, transform.forward * 20.0f, Color.red);

        // Create a RaycastHit variable to store information about the hit
        RaycastHit hit;

        // Check if the ray hits any collider
        if (Physics.Raycast(ray, out hit))
        {
            // The hit variable now contains information about the object hit
            GameObject hitObject = hit.collider.gameObject;

            // Do something with the hit object
            Debug.Log("Hit Object: " + hitObject.name);
        }

        /*Vector3 nextRay = handle.getResult(transform.position, startingDirection);
        Debug.Log("nextRay pos: " + nextRay);*/
    }
    private void CreateBeamGameObject()
    {
        beam = new GameObject("Beam");
        beam.transform.parent = transform;
        beam.transform.position = transform.position;
        
        lineRenderer = beam.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0f;
        lineRenderer.endWidth = 1f;
        lineRenderer.numCapVertices = 20;
        lineRenderer.numCornerVertices = 5;
        lineRenderer.material = beamMaterial;

        // Initially disable the beam
        beam.SetActive(false);
    }

public void StartSmoothTransition(int newBeamIndex)
{
    
    if (!isTransitionActive) // Check if a transition is not already active
    {
        // Start the transition immediately
        BeginTransition(newBeamIndex);
    }
    else
    {
        // Queue the new beam index for later processing
        beamIndexQueue.Enqueue(newBeamIndex);
       // Debug.Log($"Transition is currently active. Queued new transition to index {newBeamIndex}.");
    }
}

private void BeginTransition(int newBeamIndex)
{
    isTransitionActive = true; // Mark the transition as active
    if (currentTransitionCoroutine != null)
    {
        StopCoroutine(currentTransitionCoroutine);
    }
    currentTransitionCoroutine = StartCoroutine(MoveRayToBeamIndexSmooth(newBeamIndex, 0.15f)); 


}

        
 private IEnumerator MoveRayToBeamIndexSmooth(int newBeamIndex, float transitionTime)
    
    {
         isTransitionActive = true; // Transition starts

        float degreeIncreaseHorizontal = degreeRangeHorizontal / numCol;
        float degreeIncreaseVertical = degreeRangeVertical / numRows;

        int oldCol = (beamIndex - 25) % numCol;
        int oldRow = Mathf.FloorToInt((beamIndex - 25) / (float)numCol);


          // Calculate the horizontal and vertical angels
        oldDegreeHorizontal = minDegHorizontal + oldCol * degreeIncreaseHorizontal;
        oldDegreeVertical = minDegVertical + oldRow * degreeIncreaseVertical;

        Vector3 startingDirection = transform.rotation * Quaternion.Euler(oldDegreeVertical, oldDegreeHorizontal, 0) * Vector3.forward;

        beamIndex = newBeamIndex;
      

        int currentCol = (beamIndex - 25) % numCol;
        int currentRow = Mathf.FloorToInt((beamIndex - 25) / (float)numCol);

        // Calculate the horizontal and vertical angels
        degreeHorizontal = minDegHorizontal + currentCol * degreeIncreaseHorizontal;
        degreeVertical = minDegVertical + currentRow * degreeIncreaseVertical;

        // Calculate the target direction
        Vector3 targetDirection = transform.rotation * Quaternion.Euler(degreeVertical, degreeHorizontal, 0) * Vector3.forward;

        //float startTime = Time.time;
        //Debug.Log($"Starting transition at: {startTime}");

        //  Debug.Log($"Beam Index: {beamIndex}, Horizontal Angle: {degreeHorizontal}, Vertical Angle: {degreeVertical}");


        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
           // Debug.Log("in the while looop");
            elapsedTime += Time.deltaTime;
            float lerpFactor = elapsedTime / transitionTime;
           // lerpFactor = Mathf.Sin(lerpFactor * Mathf.PI * 0.5f); // Ease out


            // Smoothly interpolate the beam's direction
            Vector3 smoothedDirection = Vector3.Slerp(startingDirection, targetDirection, lerpFactor);

            // Apply the smoothed direction to draw the beam
            DrawBeam(smoothedDirection);

           yield return null;
      

        }
        isTransitionActive = false; // Transition ends

            //  float endTime = Time.time;
   // Debug.Log($"Ending transition at: {endTime}");
    //Debug.Log($"Total transition time: {endTime - startTime} seconds");


  

    DrawBeam(targetDirection);

          if (beamIndexQueue.Count > 0)
    {
        // Dequeue the next index and begin the transition
        BeginTransition(beamIndexQueue.Dequeue());
    }

//     if (beamIndexQueue.Count > 0)
// {
//     int nextIndex = beamIndexQueue.Dequeue();
//     // Check if the next index is the same as the current one before transitioning
//     if (nextIndex != beamIndex)
//     {
//         BeginTransition(nextIndex);
//     }
//     else if (beamIndexQueue.Count > 0)
//     {
//         // If the next index is the same, check the queue for a different index
//         BeginTransition(beamIndexQueue.Dequeue());
//     }
//}



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
