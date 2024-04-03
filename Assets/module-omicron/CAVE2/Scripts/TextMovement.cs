using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMovement : MonoBehaviour
{
    public Transform player; // Assign in inspector
    public Transform cam;

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position;
        transform.rotation = cam.rotation;
    }
}
