using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StarMovement : MonoBehaviour
{
    public Slider speedSlider;
    public Slider scaleSlider;
    private StarDataParser starDataParser;
    public TextMeshProUGUI timeElapsedText;

    private UnityEngine.Vector3 initialUserPosition;
    private UnityEngine.Quaternion initialUserRotation;
    private UnityEngine.Vector3 initialMenuPosition;
    private UnityEngine.Quaternion initialMenuRotation;

    public GameObject player;
    public GameObject cam;
    public GameObject menu;

    public GameObject ConstellationLines;

    Dictionary<string, StarData> starList = new Dictionary<string, StarData>();
    Dictionary<string, StarData> originalStarList = new Dictionary<string, StarData>();
    Dictionary<string, UnityEngine.Vector3> constellationPointsDict = new Dictionary<string, UnityEngine.Vector3>();

    public int timeSpeedFactor = 1000;
    private bool moveTime = false;
    private int moveDirection = 1;
    float yearsPassed = 0;
    float distanceToRender;

    void Start()
    {
        // Find the StarDataParser object
        starDataParser = FindObjectOfType<StarDataParser>();

        initialUserPosition = player.transform.position;
        initialUserRotation = cam.transform.rotation;

        initialMenuPosition = menu.transform.position;
        initialMenuRotation = menu.transform.rotation;
    }

    void Update()
    {
        if (starList.Count == 0)
        {
            starList = starDataParser.starList;
            originalStarList = new Dictionary<string, StarData>();
            foreach (KeyValuePair<string, StarData> entry in starDataParser.starList)
            {
                var star = entry.Value;
                StarData originalStarData = new StarData
                {
                    absMag = star.absMag,
                    relMag = star.relMag,
                    dist = star.dist,
                    position = new UnityEngine.Vector3(star.position.x, star.position.y, star.position.z),
                    spect = star.spect,
                    vx = star.vx,
                    vy = star.vy,
                    vz = star.vz,
                    instance = star.instance
                };
                originalStarList.Add(entry.Key, originalStarData);
            }
        }

        if (distanceToRender != starDataParser.distanceToRender)
        {
            distanceToRender = starDataParser.distanceToRender;
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

        player.transform.position = initialUserPosition;
        cam.transform.rotation = initialUserRotation;
        menu.transform.position = initialMenuPosition;
        menu.transform.rotation = initialMenuRotation;
        yearsPassed = 0;
        timeElapsedText.text = "Time Elapsed:\n" + (int)yearsPassed + " years";

    }

    void MoveStar(int direction)
    {
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;

            // Calculate the distance from the star to the camera
            float distanceToCamera = UnityEngine.Vector3.Distance(star.position, player.transform.position);

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

    public void MoveConstellations()
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

    public void UpdateTimeSpeed()
    {
        switch(speedSlider.value)
        {
            case 0:
            {
                    timeSpeedFactor = 500;
                    break;
            }
            case 1:
            {
                    timeSpeedFactor = 1000; 
                    break;

            }
            case 2:
            {
                timeSpeedFactor = 2000;
                break;
            }
            case 3:
            {
                timeSpeedFactor = 5000; 
                break;

            }
        }
    }

    //public void ScaleStarDistances()
    //{

    //    ////Compute the ratio of the new distance to the current maximum visible distance
    //    //float scaleRatio = (scaleSlider.value * 10) / distanceToRender;


    //    //// Iterate over each star in the list
    //    //foreach (var keyValuePair in starList)
    //    //{
    //    //    var star = keyValuePair.Value;

    //    //    // Compute the vector from the camera to the current star position
    //    //    UnityEngine.Vector3 offsetFromCamera = star.position - cam.transform.position;

    //    //    // Scale this offset by our computed ratio
    //    //    UnityEngine.Vector3 scaledOffset = offsetFromCamera * scaleRatio;

    //    //    // Compute the new position of the star by adding the scaled offset to the camera position
    //    //    UnityEngine.Vector3 updatedPosition = cam.transform.position + scaledOffset;

    //    //    // Update the position in both the star data and the corresponding GameObject
    //    //    star.position = updatedPosition;
    //    //    star.instance.transform.position = updatedPosition;
    //    //    constellationPointsDict[keyValuePair.Key] = updatedPosition;

    //    //}
    //    //distanceToRender = scaleSlider.value * 10;

    //    // Compute the ratio of the new distance to the current maximum visible distance
    //    float scaleRatio = (scaleSlider.value * 10) / distanceToRender;

    //    // Iterate over each star in the list
    //    foreach (var keyValuePair in starList)
    //    {
    //        var star = keyValuePair.Value;

    //        // Compute the vector from the camera to the current star position
    //        UnityEngine.Vector3 offsetFromCamera = star.position - cam.transform.position;

    //        // Scale this offset by our computed ratio
    //        UnityEngine.Vector3 scaledOffset = offsetFromCamera * scaleRatio;

    //        // Compute the new position of the star by adding the scaled offset to the camera position
    //        UnityEngine.Vector3 updatedPosition = cam.transform.position + scaledOffset;

    //        // Update the position in both the star data and the corresponding GameObject
    //        star.position = updatedPosition;
    //        star.instance.transform.position = updatedPosition;
    //    }

    //    // Update the maximum visible distance to the new distance
    //    distanceToRender = (scaleSlider.value * 10);

    //    //MoveConstellations();

    //    // Update the maximum visible distance to the new distance
        
    //}
}
