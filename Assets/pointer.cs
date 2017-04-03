using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointer : MonoBehaviour {

    LineRenderer lineRenderer;
    public RaycastHit hit;
    public float distance;
    public bool drawline = true;
    // Use this for initialization
    void Start () {
        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.numPositions = 2;
    }
	
	// Update is called once per frame
	void Update () {

        if (drawline)
        {
            Physics.Raycast(transform.position, transform.up, out hit, 300f);
            if (hit.collider)
            {
                distance = hit.distance / 4.8f;
                lineRenderer.SetPosition(1, new Vector3(0, distance, 0));
            }
            else
            {
                lineRenderer.SetPosition(1, new Vector3(0, 300, 0));
            }
        }
        else
        {
            lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        }
    }
}
