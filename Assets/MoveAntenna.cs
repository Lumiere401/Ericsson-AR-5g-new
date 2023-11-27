using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAntenna : MonoBehaviour
{
    public GameObject antenna;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Up()
    {
        antenna.transform.position = antenna.transform.position + new Vector3(0, 0.2f, 0);
    }

    public void Right()
    {
        antenna.transform.position = antenna.transform.position + new Vector3(0.2f, 0, 0);
    }

    public void Farther()
    {
        antenna.transform.position = antenna.transform.position + new Vector3(0, 0, 0.2f);
    }

    public void Down()
    {
        antenna.transform.position = antenna.transform.position - new Vector3(0, 0.2f, 0);
    }

    public void Left()
    {
        antenna.transform.position = antenna.transform.position - new Vector3(0.2f, 0, 0);
    }

    public void Closer()
    {
        antenna.transform.position = antenna.transform.position - new Vector3(0, 0, 0.2f);
    }
}
