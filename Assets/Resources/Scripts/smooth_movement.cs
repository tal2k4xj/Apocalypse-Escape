using UnityEngine;
using System.Collections;

public class smooth_movement : MonoBehaviour {

    public GameObject ARcam;
    public Transform target;
    private float x = 0;
    private float y = 0;
    private float z = 0;

    void FixedUpdate()
    {
        Vector3 pos = ARcam.transform.position;
        x = x + ((pos.x - x) / 10);
        y = y + ((pos.y - y) / 10);
        z = z + ((pos.z - z) / 10);
        Vector3 cam = transform.position;
        cam.x = x;
        cam.z = z;
        cam.y = y;
        transform.position = cam;
    
        transform.LookAt(target);
        cam = transform.eulerAngles;
        pos = ARcam.transform.eulerAngles;
        cam.z = pos.z;
        transform.eulerAngles = cam;
    }
}
