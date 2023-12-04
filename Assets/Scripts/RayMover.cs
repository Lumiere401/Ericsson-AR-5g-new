using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMover : MonoBehaviour
{
    public GameObject FiveGBox;
    Vector3 bottomLeft;

    // Declare variables here so no new declarations are performed per fixed time update
    int currentColumn = 0;
    int currentRow = 0;
    int numColumns = 50;
    int numRows = 6;
    float minDegHorizontal = -Mathf.PI / 6;
    float minDegVertical = -Mathf.PI / 12;
    float degreeHorizontal = 0f;
    float degreeVertical = 0f;
    Vector3 localEulerAngles;
    Quaternion horizontalRotation;
    Quaternion verticalRotation;
    Quaternion finalRotation;

    public float widthUnit;
    public float heightUnit;
    public bool move;
    public int beamIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Allow object to rotate according to beamIndex
        move = true;

        // Get current object rotation
        var rot = gameObject.transform.localRotation.eulerAngles;
        // Make sure it´s aligned with antennae [Please note: 5G antennae are rotated downwards 17 degrees, hence the weird value]
        rot.Set(0f, -30f, -73f);
        // Update object with rotation
        gameObject.transform.localRotation = Quaternion.Euler(rot);

        //widthUnit = 1.8f / columns;
        //heightUnit = 2.5f / rows;
        //bottomLeft = transform.position;
        StartCoroutine(MoveRayEverySecond());       
    }

    // Update is called once per frame
    void Update()
    {
        //this.transform.Rotate(1, 0, 0);
        MoveRay(beamIndex);
    }

    IEnumerator MoveRayEverySecond()
    {
        // Perform check here to prevent checking every update
        if (move)
        {
            for (beamIndex = 0; beamIndex <= 300; beamIndex++)
            {
                MoveRay(beamIndex);
                yield return new WaitForFixedUpdate();
            }
        }
    }
    void MoveRay(int beamIndex)
    {
        minDegHorizontal = -Mathf.PI / 6;
        minDegVertical = -Mathf.PI / 12;
        currentColumn = beamIndex % 50;
        currentRow = (int) Mathf.Floor((float) beamIndex / 50.0f);

        degreeHorizontal = minDegHorizontal + currentColumn * (Mathf.PI / 3) / numRows;
        degreeVertical = minDegVertical + currentRow * (Mathf.PI / 6) / numColumns;

        // Step 1: Convert radians to local Euler angles.
        localEulerAngles = new Vector3(degreeHorizontal, degreeVertical, 0.0f);
        
        // Step 2: Create quaternions from the local Euler angles.
        horizontalRotation = Quaternion.Euler(0, localEulerAngles.y * Mathf.Rad2Deg, 0);
        verticalRotation = Quaternion.Euler(localEulerAngles.x * Mathf.Rad2Deg, 0, 0);

        // Step 3: Combine the quaternions to obtain the final world-space quaternion.
        finalRotation = horizontalRotation * verticalRotation;

        // Step 4: Apply the rotation to the GameObject at the correct framepacing
        transform.Rotate(finalRotation.eulerAngles * Time.deltaTime, Space.Self);

        //this.transform.rotation = Quaternion.LookRotation(new Vector3());
        //this.transform.position = bottomLeft + new Vector3(0,heightUnit*row,widthUnit*column);
    }
}
