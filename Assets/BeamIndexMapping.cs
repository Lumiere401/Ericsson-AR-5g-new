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
    int beamIndex;

    


    public bool move;

        public void MoveRayToBeamIndex(int newBeamIndex)
    {
        beamIndex = newBeamIndex;
        float degreeIncreaseHorizontal = degreeRangeHorizontal / numCol;
        float degreeIncreaseVertical = degreeRangeVertical / numRows;

        int currentCol;
        int currentRow;

        // Calculate column and row based on beam index
        if (beamIndex >= 25 && beamIndex <= 312)
        {
            currentCol = (beamIndex - 25) % numCol;
            currentRow = Mathf.FloorToInt((beamIndex - 25) / (float)numCol);
        }
        // else if (beamIndex >= 209 && beamIndex <= 344)
        // {
        //     currentCol = (beamIndex - 208 - 1) % numCol;
        //     currentRow = Mathf.FloorToInt((beamIndex - 208 - 1) / (float)numCol) + 4;
        // }
        else
        {
            Debug.LogError("Invalid beam index");
            return;
        }

        // Calculate horizontal and vertical degrees
        degreeHorizontal = minDegHorizontal + currentCol * degreeIncreaseHorizontal;
        degreeVertical = minDegVertical + currentRow * degreeIncreaseVertical;

        // Calculate direction vector
        Vector3 direction = Quaternion.Euler(degreeVertical, degreeHorizontal, 0) * Vector3.forward;

        // Set beam position and direction
        transform.rotation = Quaternion.LookRotation(direction);

        // Optionally, visualize the beam with a LineRenderer or other component
        DrawBeam(direction);
    }

    // Method to visualize the beam
    private void DrawBeam(Vector3 direction)
    {
        // Create a new GameObject to represent the beam
        GameObject beam = new GameObject("Beam" + beamIndex);
        beam.transform.position = transform.position;

        // Attach a LineRenderer to visualize the beam
        LineRenderer lineRenderer = beam.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, beam.transform.position);
        lineRenderer.SetPosition(1, beam.transform.position + direction * 10); // Adjust the length as needed
        beam.transform.rotation = Quaternion.LookRotation(direction);

        // Destroy the previous beam GameObject to only show the current one
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        beam.transform.parent = transform;
    }
}


    // Start is called before the first frame update
//     void Start()
//     {
//         move = true;

//         StartCoroutine(MoveRayEverySecond());


//     }


//     IEnumerator MoveRayEverySecond()
//     {
//         if (move)
//         {


//             for (beamIndex = 37; beamIndex <= 344; beamIndex++)
//             {

//                 if (beamIndex > 172 && beamIndex < 209) continue;
//                 MoveRay(beamIndex);
//                 yield return new WaitForSecondsRealtime(1);
//                 // yield return new WaitForFixedUpdate();
//             }


//         }
//         move = false; // Set move to false once coroutine completes


//     }



//     void MoveRay(int beamIndex)
//     {

//         float degreeIncreaseHorizontal = degreeRangeHorizontal / (float)numCol;
//         float degreeIncreaseVertical = degreeRangeVertical / (float)numRows;



//         if (beamIndex >= 37 && beamIndex <= 172)
//         {
//             currentCol = (beamIndex - 36 - 1) % numCol;
//             currentRow = (int)Mathf.Floor((beamIndex - 36 - 1) / (float)numCol);

//         }

//         if (beamIndex >= 209 && beamIndex <= 344)
//         {
//             currentCol = (beamIndex - 208 - 1) % numCol;
//             currentRow = (int)Mathf.Floor((beamIndex - 208 - 1) / (float)numCol) + 4;

//         }

//         degreeHorizontal = minDegHorizontal + currentCol * degreeIncreaseHorizontal;
//         degreeVertical = minDegVertical + currentRow * degreeIncreaseVertical;




//         Vector3 direction = Quaternion.Euler(degreeVertical, degreeHorizontal, 0) * Vector3.forward;
//         Quaternion worldRotation = transform.rotation;
//         direction = worldRotation * direction; // Convert the direction to world space




//         // Create a new GameObject to represent the beam
//         GameObject beam = new GameObject("Beam" + beamIndex);
//         beam.transform.position = transform.position;

//         // Debug the cube's forward vector
//         //  Debug.DrawRay(this.transform.position, transform.forward, Color.green, 100000f, false);

//         // Attach a LineRenderer to visualize the beam
//         LineRenderer lineRenderer = beam.AddComponent<LineRenderer>();
//         lineRenderer.startWidth = 0.1f;
//         lineRenderer.endWidth = 0.1f;
//         lineRenderer.positionCount = 2;
//         lineRenderer.SetPosition(0, beam.transform.position);
//         lineRenderer.SetPosition(1, beam.transform.position + direction * 10);
//         beam.transform.rotation = Quaternion.LookRotation(direction);



//     }
// }



// // debug the cone's forward vector


// Debug.DrawRay(this.transform.position,transform.forward,Color.green,100000f,false);


// // Attach a LineRenderer to visualize the beam
// LineRenderer lineRenderer = beam.AddComponent<LineRenderer>();
// lineRenderer.startWidth = 0.1f;
// lineRenderer.endWidth = 0.1f;
// lineRenderer.positionCount = 2;
// lineRenderer.SetPosition(0, beam.transform.position);

// // Set the rotation of the beam GameObject based on the angles
// beam.transform.rotation = Quaternion.Euler(degreeVertical, degreeHorizontal, 0);

// // Update the LineRenderer positions using the beam's forward vector
// lineRenderer.SetPosition(1, beam.transform.position + transform.forward * 10);
// Attach a LineRenderer to visualize the beam
// LineRenderer lineRenderer = beam.AddComponent<LineRenderer>();
// lineRenderer.startWidth = 0.1f;
// lineRenderer.endWidth = 0.1f;
// lineRenderer.positionCount = 2;
//     // Set the rotation of the beam GameObject based on the angles
// beam.transform.rotation = Quaternion.Euler(degreeVertical, degreeHorizontal, 0);
// lineRenderer.SetPosition(1, beam.transform.position + beam.transform.forward * 10);
// lineRenderer.SetPosition(0, beam.transform.position); 
// lineRenderer.SetPosition(1, beam.transform.position + direction * 10); 
// // Set the rotation of the beam GameObject based on the angles
//  beam.transform.rotation = Quaternion.Euler(degreeVertical, degreeHorizontal, 0);



// lineRenderer = gameObject.AddComponent<LineRenderer>();
//  int totalBeams = (172 - 37 + 1) + (344 - 209 + 1); // total beams without skips
//  lineRenderer.positionCount = totalBeams * 2; // minus 2 to remove redundancy at connecting points

// int adjustedIndex;
// if (beamIndex < 173)
// {
//     adjustedIndex = (beamIndex - 37) * 2;
// }
// else if (beamIndex >= 209)
// {
//     adjustedIndex = ((172 - 37 + 1) + (beamIndex - 209)) * 2;
// }
// else
// {
//     return; // Skip the indices between 173 and 208
// }

// Vector3 startPosition = transform.position;
// Debug.Log(startPosition);

// if (adjustedIndex != 0)
// {
//     // Getting the last endpoint of the previous beam as the start of the current
//     startPosition = lineRenderer.GetPosition(adjustedIndex - 2);
// }

// Vector3 endPosition = startPosition + direction; 

// Debug.Log("BeamIndex: " + beamIndex + ", AdjustedIndex: " + adjustedIndex);


// // Set the positions in the LineRenderer
// lineRenderer.SetPosition(adjustedIndex, startPosition); 
// lineRenderer.SetPosition(adjustedIndex + 1, endPosition);



// startRotation = transform.rotation;


// Debug.Log(startRotation);

// Vector3 newEulerRotation = new Vector3(degreeVertical,degreeHorizontal,0);

// Quaternion newRotation = Quaternion.Euler(newEulerRotation);
// Vector3 direction = newRotation * Vector3.forward; //works well, except more rows on bottom half
// Vector3 up = newRotation * Vector3.up;
// float desiredLength = 10.0f; 
// Vector3 extendedDirection = direction.normalized * desiredLength;
// Debug.DrawRay(this.transform.position,extendedDirection,Color.green,100000f,false);
// transform.rotation = newRotation;

//     
//    

//  transform.rotation = newRotation;
// Quaternion correction = Quaternion.LookRotation(transform.forward, -transform.up);
// Quaternion rotation = Quaternion.LookRotation(direction);
// Quaternion finalRotation =  newRotation * transform.rotation;
//  Vector3 direction1 = finalRotation * Vector3.forward;
//  Debug.DrawRay(this.transform.position,direction1,Color.red,100000f,false);
// transform.rotation = finalRotation;




//DEBUG Cone forward
//Debug.DrawRay(this.transform.position, transform.forward, Color.blue,100000f,false);


//  float verticalRad = degreeVertical * Mathf.Deg2Rad;
//  float horizontalRad = degreeHorizontal * Mathf.Deg2Rad;

// // // Calculate the direction vector 

// Vector3 direction = new Vector3(
//      Mathf.Sin(verticalRad) * Mathf.Cos(horizontalRad),   // x
//      Mathf.Cos(verticalRad),                              // y
//      -Mathf.Sin(verticalRad) * Mathf.Sin(horizontalRad)   // z
//  );

//  direction = direction.normalized;
// // Debug.DrawLine(this.transform.position, this.transform.position + direction * 10, Color.red);
//  Debug.DrawRay(this.transform.position,direction,Color.green,100000f,false);


// // Create a Quaternion rotation from the direction vector
// Quaternion rotation = Quaternion.LookRotation(direction);

// Vector3 initialEulerRotation = new Vector3(0,0,90);

// Quaternion initialRotation = Quaternion.Euler(initialEulerRotation);


// // Combine the rotations by multiplying the quaternions
// Quaternion finalRotation = initialRotation * rotation;

// // Apply the final rotation to the object
// transform.rotation = finalRotation;