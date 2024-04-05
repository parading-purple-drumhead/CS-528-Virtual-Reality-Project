﻿using System;
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
    private UnityEngine.Vector3 initialUserPosition;
    private UnityEngine.Quaternion initialUserRotation;
    public GameObject cam;

    Dictionary<string, StarData> starList = new Dictionary<string, StarData>();
    Dictionary<string, StarData> originalStarList = new Dictionary<string, StarData>();
    Dictionary<string, UnityEngine.Vector3> constellationPointsDict = new Dictionary<string, UnityEngine.Vector3>();

    public int timeSpeedFactor = 1000;
    private bool moveTime = false;
    private int moveDirection = 1;
    float yearsPassed = 0;

    void Start()
    {
        // Find the StarDataParser object
        starDataParser = FindObjectOfType<StarDataParser>();

        initialUserPosition = cam.transform.position;
        initialUserRotation = cam.transform.rotation;
    }

    void Update()
    {
        if (starList.Count == 0)
        {
            starList = starDataParser.starList;
            originalStarList = new Dictionary<string, StarData>();
            foreach (KeyValuePair<string, StarData> entry in starDataParser.starList)
            {
                StarData originalStarData = new StarData
                {
                    absMag = entry.Value.absMag,
                    relMag = entry.Value.relMag,
                    dist = entry.Value.dist,
                    position = new UnityEngine.Vector3(entry.Value.position.x, entry.Value.position.y, entry.Value.position.z),
                    spect = entry.Value.spect,
                    vx = entry.Value.vx,
                    vy = entry.Value.vy,
                    vz = entry.Value.vz,
                    instance = entry.Value.instance // Assuming GameObject instance can be shared
                };
                originalStarList.Add(entry.Key, originalStarData);
            }
        }

        if (constellationPointsDict.Count == 0)
        {
            constellationPointsDict = starDataParser.constellationPointsDict;
        }

        if (moveTime)
        {
            MoveStar(moveDirection);
            MoveConstellations();
        }
    }

    public void MoveTime(int direction) {
        moveTime = true;
        moveDirection = direction;
    }

    public void PauseTime()
    {
        moveTime = false;
    }

    public void ResetTime()
    {
        moveTime = false;

        foreach (var keyValuePair in starList)
        {
            
            var star = keyValuePair.Value;
            star.instance.transform.position = originalStarList[keyValuePair.Key].position;
        }

        cam.transform.position = initialUserPosition;
        cam.transform.rotation = initialUserRotation;
        yearsPassed = 0;
        timeElapsedText.text = "Time Elapsed:\n" + (int)yearsPassed + " years";
    }

    void MoveStar(int direction)
    {
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
