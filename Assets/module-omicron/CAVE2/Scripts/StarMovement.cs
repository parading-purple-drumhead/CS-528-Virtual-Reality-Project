using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class StarMovement : MonoBehaviour
{
    public GameObject ConstellationLines;
    private StarDataParser starDataParser;
    Dictionary<string, StarData> starList = new Dictionary<string, StarData>();
    Dictionary<string, UnityEngine.Vector3> constellationPointsDict = new Dictionary<string, UnityEngine.Vector3>();
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

        if (constellationPointsDict.Count == 0)
        {
            constellationPointsDict = starDataParser.constellationPointsDict;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            moveStars = !moveStars;
        }

        if (moveStars)
        {
            MoveStar();
            //InvokeRepeating("MoveConstellations", 0f, 2f);
            MoveConstellations();
        }

    }

    void MoveStar()
    {
        // Get the camera's position

        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;

            // Calculate the distance from the star to the camera
            float distanceToCamera = UnityEngine.Vector3.Distance(star.position, cam.transform.position);

            UnityEngine.Vector3 velocity = new UnityEngine.Vector3(star.vx, star.vy, star.vz);

            // Update the position of the star data
            star.position += velocity * Time.deltaTime * 10000;

            constellationPointsDict[keyValuePair.Key] = star.position;

            // If the star is within 25 units of the camera, update the position of the star instance
            if (distanceToCamera <= 25f)
            {
                // Update the position of the star instance
                star.instance.transform.position = star.position;
            }
        }
    }

    void MoveConstellations()
    {
        foreach (Transform constellationLine in ConstellationLines.transform)
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
