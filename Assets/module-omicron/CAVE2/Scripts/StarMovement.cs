using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using TMPro;

public class StarMovement : MonoBehaviour
{
    public GameObject ConstellationLines;
    private StarDataParser starDataParser;
    public TextMeshProUGUI timeElapsedText;
    public GameObject cam;

    Dictionary<string, StarData> starList = new Dictionary<string, StarData>();
    Dictionary<string, UnityEngine.Vector3> constellationPointsDict = new Dictionary<string, UnityEngine.Vector3>();

    public int timeSpeedFactor = 1000;
    private bool moveTimeForward = false;
    private bool moveTimeBackward = false;
    float yearsPassed = 0;

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

        if (constellationPointsDict.Count == 0)
        {
            constellationPointsDict = starDataParser.constellationPointsDict;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            moveTimeForward = !moveTimeForward;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            moveTimeBackward = !moveTimeBackward;
        }

        if (moveTimeForward)
        {
            moveTimeBackward = false;
            MoveStar(1);
            MoveConstellations();
        }

        if (moveTimeBackward)
        {
            moveTimeForward = false;
            MoveStar(-1);
            MoveConstellations();
        }

    }

    public void MoveTimeForward()
    {
        moveTimeForward = !moveTimeForward;
    }

    public void MoveTimeBackward()
    {
        moveTimeBackward = !moveTimeBackward;
    }

    void MoveStar(int direction)
    {
        // Get the camera's position

        

        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;

            // Calculate the distance from the star to the camera
            float distanceToCamera = UnityEngine.Vector3.Distance(star.position, cam.transform.position);

            UnityEngine.Vector3 velocity = new UnityEngine.Vector3(star.vx, star.vy, star.vz) * direction;

            // Update the position of the star data
            star.position += velocity * Time.deltaTime * timeSpeedFactor;

            constellationPointsDict[keyValuePair.Key] = star.position;

            // If the star is within 25 units of the camera, update the position of the star instance
            if (distanceToCamera <= 25f)
            {
                // Update the position of the star instance
                star.instance.transform.position = star.position;
            }
        }
        yearsPassed += timeSpeedFactor * Time.deltaTime * direction;

        timeElapsedText.text = "Time Elapsed:\n" + (int) yearsPassed + " years";
    }

    void MoveConstellations()
    {
        

        foreach (Transform constellation in ConstellationLines.transform)
        {
            //Debug.Log(constellationLine.gameObject.name);
            //string lineName = constellationLine.gameObject.name;
            //var stars = lineName.Split('-'); 
            //var starHip1 = stars[0];
            //var starHip2 = stars[1];

            //UnityEngine.Vector3 star1Pos = constellationPointsDict[starHip1];
            //UnityEngine.Vector3 star2Pos = constellationPointsDict[starHip2];

            //constellationLine.gameObject.GetComponent<LineRenderer>().SetPosition(0, star1Pos);
            //constellationLine.gameObject.GetComponent<LineRenderer>().SetPosition(1, star2Pos);
            if (constellation.gameObject.activeInHierarchy)
            {
                foreach (Transform constellationLine in constellation.gameObject.transform)
                {
                    string lineName = constellationLine.gameObject.name;
                    var stars = lineName.Split('-');
                    var starHip1 = stars[0];
                    var starHip2 = stars[1];

                    UnityEngine.Vector3 star1Pos = constellationPointsDict[starHip1];
                    UnityEngine.Vector3 star2Pos = constellationPointsDict[starHip2];

                    constellationLine.gameObject.GetComponent<LineRenderer>().SetPosition(0, star1Pos);
                    constellationLine.gameObject.GetComponent<LineRenderer>().SetPosition(1, star2Pos);
                }
                
            }
        }
    }
}
