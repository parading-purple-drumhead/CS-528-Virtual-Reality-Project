using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    public TextMeshProUGUI distanceText;
    private float distance;
    private float previousDistance;

    // Start is called before the first frame update
    void Start()
    {
        previousDistance = Vector3.Distance(gameObject.transform.position, new Vector3(0, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(gameObject.transform.position, new Vector3(0, 0, 0));

        if (Mathf.Abs(distance - previousDistance) > 0.01f)
        {
            distanceText.text = "Distance:\n" + distance.ToString("F2") + " parsecs";
            previousDistance = distance;
        }
    }
}
