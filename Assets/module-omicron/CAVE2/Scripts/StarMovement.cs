using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarMovement : MonoBehaviour
{
    private StarDataParser starDataParser;
    Dictionary<string, StarData> starList = new Dictionary<string, StarData>();
    private bool moveStars = false;

    public GameObject cam;

    void Start()
    {
        // Find the StarDataParser object
        starDataParser = FindObjectOfType<StarDataParser>();
    }

    void Update()
    {
        if (starList.Count == 0) { 
            starList = starDataParser.starList;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            moveStars = !moveStars;
        }

        if (moveStars)
        {
            MoveStar();
        }
    }

    //void MoveStar()
    //{
    //    foreach (var keyValuePair in starList) {
    //        var star = keyValuePair.Value;

    //        // Assuming star.instance is of type GameObject and star.vx, star.vy, star.vz are of type float
    //        Vector3 velocity = new Vector3(star.vx, star.vy, star.vz);

    //        // Update the position of the star instance
    //        star.instance.transform.position += velocity * Time.deltaTime * 1000;
    //    }
    //}

    void MoveStar()
    {
        // Get the camera's position

        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;

            // Calculate the distance from the star to the camera
            float distanceToCamera = Vector3.Distance(star.instance.transform.position, cam.transform.position);

            // Only move and render the star if it's within 25 units of the camera
            if (distanceToCamera <= 25)
            {
                // Assuming star.instance is of type GameObject and star.vx, star.vy, star.vz are of type float
                Vector3 velocity = new Vector3(star.vx, star.vy, star.vz);

                // Update the position of the star instance
                star.instance.transform.position += velocity * Time.deltaTime * 1000;

                // Enable the MeshRenderer
                star.instance.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                // Disable the MeshRenderer
                star.instance.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

}
