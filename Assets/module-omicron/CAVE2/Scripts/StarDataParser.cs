using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StarDataParser : MonoBehaviour

{
    private StarMovement starMovement;

    public GameObject legend;
    public AudioSource ambientMusic;
    public Slider scaleSlider;
    public Text scaleValueText;
    public TextAsset[] constellationFiles = new TextAsset[6];
    public GameObject ConstellationLines;
    public GameObject cam;
    Vector3 lastRenderPosition;
    public float distanceToRender = 50f;
    //public Toggle showFeaturedConstellationToggle;
    //public Toggle modernConstellationsToggle;

    // for stars
    public TextAsset starDataSource;
    public GameObject starPrefab;
    public Dictionary<string, StarData> starList = new Dictionary<string, StarData>();
    public float scaleRatioOfStars = 0.05f;
    public float scaleDistanceOfStars = 1.0f;

    // for constellations
    //public TextAsset constellationDataSource;
    public ConstellationList constellationList = new ConstellationList();
    public Dictionary<string, Vector3> constellationPointsDict = new Dictionary<string, Vector3>();
    public LineRenderer linePrefab;
    public GameObject FeaturedConstellation;
    public GameObject FeaturedConstellationInfo;

    // for exoplanet
    public TextAsset exoplanetDataSource;
    public Dictionary<string,int> exoplanetList = new Dictionary<string,int>();

    private bool usePlanetColorScheme = false;
    public bool scaleChanged = false;

    [System.Serializable]
    public class Pair
    {
        public int[] pair;
    }

    [System.Serializable]
    public class Constellation
    {
        public string name;
        public int numOfPairs;
        public Pair[] pairs;
    }

    [System.Serializable]
    public class ConstellationList
    {
        public Constellation[] constellations;
    }

    void Start()
    {
        // Find the StarDataParser object
        starMovement = FindObjectOfType<StarMovement>();

        InvokeRepeating("LookAtMe", 0f, 2f);

        // Initialize lastRenderPosition to the player's current position
        lastRenderPosition = cam.transform.position;

        ParseExoplanetData();

        ParseStarData();

        GenerateStars();

        parseAllConstellations();
        
    }

    void Update()
    {

        if (Vector3.Distance(cam.transform.position, lastRenderPosition) > 10)
        {
            // Call SelectiveRender and update lastRenderPosition
            SelectiveRender();
            lastRenderPosition = cam.transform.position;
        }

    }

    void ParseStarData()
    {
        string fullText = starDataSource.text;
        string[] textLines = fullText.Split('\n');

        bool isFirstLine = true;

        foreach (string line in textLines)
        {
            // skipping csv header line
            if (isFirstLine)
            {
                isFirstLine = false;
            }
            else
            {
                    string[] values = line.Split(',');
                    StarData star = new StarData();
                try
                {
                star.absMag = float.Parse(values[5]);
                star.relMag = float.Parse(values[6]);
                star.dist = float.Parse(values[1]);
                star.position = new Vector3(float.Parse(values[2]), float.Parse(values[3]), float.Parse(values[4]));
                star.vx = float.Parse(values[7]) * 1.02269E-6f;
                star.vy = float.Parse(values[8]) * 1.02269E-6f;
                star.vz = float.Parse(values[9]) * 1.02269E-6f;
                star.spect = values[10].Trim();
                starList.Add(values[0].Substring(0, values[0].Length - 2), star);
                }
                
                catch (System.IndexOutOfRangeException)
                {
                    continue;
                }
            }
        }
    }

    void LookAtMe()
    {
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;
            float distanceToCamera = Vector3.Distance(star.instance.transform.position, cam.transform.position);

            //Only render the star if it's within 25 units of the camera
            if(distanceToCamera <= distanceToRender)
                star.instance.transform.LookAt(cam.transform);
        }
    }

    void SelectiveRender()
    {
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;
            float distanceToCamera = Vector3.Distance(star.instance.transform.position, cam.transform.position);

            // Only render the star if it's within 25 units of the camera
            star.instance.GetComponent<MeshRenderer>().enabled = distanceToCamera <= distanceToRender;
        }
    }

    private void parseAllConstellations()
    {
        for (int i = 0; i < 6; i++)
        {
            ParseConstellationData(i, i == 0);
        }

        ScaleStarDistances();
    }

    void ParseConstellationData(int index, bool setActive = false)
    {
        constellationList = JsonUtility.FromJson<ConstellationList>(constellationFiles[index].text);

        GameObject constellationParent = new GameObject(constellationFiles[index].name.Substring(0, constellationFiles[index].name.Length-4));
        constellationParent.transform.parent = ConstellationLines.transform;
        foreach (Constellation constellation in constellationList.constellations)
        {
            foreach (Pair pair in constellation.pairs)
            {
                Vector3 position1 = GetStarPosition(pair.pair[0].ToString());
                Vector3 position2 = GetStarPosition(pair.pair[1].ToString());

                constellationPointsDict[pair.pair[0].ToString()] = position1;
                constellationPointsDict[pair.pair[1].ToString()] = position2;

                // Create a line between the stars
                GameObject lineObject = new GameObject();
                //lineObject.transform.parent = ConstellationLines.transform; // Set the parent to constellationLines
                lineObject.transform.parent = constellationParent.transform;
                lineObject.name = pair.pair[0] + "-" + pair.pair[1];
                LineRenderer line = lineObject.AddComponent<LineRenderer>();
                line.SetPosition(0, position1);
                line.SetPosition(1, position2);
                line.useWorldSpace = true;
                line.startWidth = line.endWidth = 0.1f;
                line.material.color = Color.white;

                if (constellation.name == "UMa")
                {
                    GameObject featuredLineObject = new GameObject();
                    featuredLineObject.transform.parent = FeaturedConstellation.transform;
                    featuredLineObject.name = pair.pair[0] + "-" + pair.pair[1];
                    LineRenderer featuredLine = featuredLineObject.AddComponent<LineRenderer>();
                    featuredLine.SetPosition(0, position1);
                    featuredLine.SetPosition(1, position2);
                    featuredLine.useWorldSpace = true;
                    featuredLine.startWidth = featuredLine.endWidth = 0.1f;
                    featuredLine.material.color = Color.white;
                }
            }
        }
        constellationParent.SetActive(setActive);
        FeaturedConstellation.SetActive(false);
    }

    public void SwitchConstellationSet(int indexToShow)
    {
        for(int i = 0; i <= 5; i++)
        {
            ConstellationLines.transform.GetChild(i).gameObject.SetActive(i == indexToShow);
        }
    }

    Vector3 GetStarPosition(string starId)
    {
        Vector3 starPosition = Vector3.zero;
        if (starList.ContainsKey(starId))
            starPosition = starList[starId].position;
        return starPosition;
    }

    void GenerateStars()
    {
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;
            star.instance = Instantiate(starPrefab, star.position, Quaternion.LookRotation(star.position)) as GameObject;
            star.instance.transform.localScale *= scaleRatioOfStars * star.relMag;
            star.instance.GetComponent<Renderer>().material.color = GetStarColor(spect: star.spect);

            float distanceToCamera = Vector3.Distance(star.instance.transform.position, cam.transform.position);

            // Only render the star if it's within 25 units of the camera
            star.instance.GetComponent<MeshRenderer>().enabled = distanceToCamera <= distanceToRender;
        }
    }

    void ParseExoplanetData()
    {
        string fullText = exoplanetDataSource.text;
        string[] dataRows = fullText.Split('\n');

        bool isFirstRow = true;

        foreach (string row in dataRows)
        {
            // skipping csv header line
            if (isFirstRow)
            {
                isFirstRow = false;
            }
            else
            {
                string[] values = row.Split(',');
                try
                {
                    exoplanetList[values[0].Trim()] = int.Parse(values[1]);
                }

                catch (System.IndexOutOfRangeException)
                {
                    continue;
                }
            }
        }
    }

    void UpdateStarColors()
    {
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;

            star.instance.GetComponent<Renderer>().material.color = usePlanetColorScheme? GetStarColor(starId: keyValuePair.Key) : GetStarColor(spect: star.spect);
        }
    }

    Color GetStarColor(string starId = "", string spect = "")
    {
        if (usePlanetColorScheme)
        {
            legend.gameObject.SetActive(true);
            // Define your color scheme based on the number of planets here
            Color[] planetColorScheme = new Color[] { Color.white, Color.yellow, Color.green, Color.blue, Color.magenta, Color.red };

            int numOfPlanets = 0;

            if (exoplanetList.ContainsKey(starId))
            {
                numOfPlanets = exoplanetList[starId];
            }

            // Limit the number of planets to the number of colors in the color scheme
            numOfPlanets = Mathf.Min(numOfPlanets, planetColorScheme.Length - 1);

            return planetColorScheme[numOfPlanets];
        }
        else
        {
            legend.gameObject.SetActive(false);
            Color orange = new Color(1.0f, 0.64f, 0.0f); // RGB for Orange

            if (spect == "O")
            {
                return Color.blue;
            }
            else if (spect == "B" || spect == "A")
            {
                return Color.white;
            }
            else if (spect == "F" || spect == "G")
            {
                return Color.yellow;
            }
            else if (spect == "K")
            {
                return orange; // Use the custom orange color
            }
            else if (spect == "M")
            {
                return Color.red;
            }
            else
            {
                return Color.white; // Default color
            }
        }
    }

    public void ScaleStarDistances() { 

        scaleValueText.text = (int)scaleSlider.value - 2 + "x";

        float newDistance = scaleSlider.value * 10f;

        // Compute the ratio of the new distance to the current maximum visible distance
        float scaleRatio = newDistance / distanceToRender;

        // Iterate over each star in the list
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;

            // Compute the vector from the camera to the current star position
            Vector3 offsetFromCamera = star.position - cam.transform.position;

            // Scale this offset by our computed ratio
            Vector3 scaledOffset = offsetFromCamera * scaleRatio;

            // Compute the new position of the star by adding the scaled offset to the camera position
            Vector3 updatedPosition = cam.transform.position + scaledOffset;

            // Update the position in both the star data and the corresponding GameObject
            star.position = updatedPosition;
            star.instance.transform.position = updatedPosition;
            constellationPointsDict[keyValuePair.Key] = updatedPosition;
            scaleChanged = true;
        }

        // Update the maximum visible distance to the new distance
        distanceToRender = newDistance;
    }

    public void UpdateDistanceToRender(float distance)
    {
        distanceToRender = distance;
    }

    public void ChangeColorScheme()
    {
        // Toggle the color scheme
        usePlanetColorScheme = !usePlanetColorScheme;

        // Update the colors of the stars
        UpdateStarColors();
    }

    //public void ShowFeaturedConstellation(bool show, int index)
    public void ShowFeaturedConstellation(string args)
    {

        bool show = bool.Parse(args.Split('|')[0]);
        int index = int.Parse(args.Split('|')[1]);

        starMovement.ResetTime();

        //showFeaturedConstellation = !showFeaturedConstellation;

        SwitchConstellationSet(show ? -1 : index);
        FeaturedConstellation.SetActive(show);
        FeaturedConstellationInfo.SetActive(show);

        ambientMusic.volume = show ? 0.2f: 1f;

        //showFeaturedConstellationToggle.isOn = showFeaturedConstellation;
        //modernConstellationsToggle.isOn = !showFeaturedConstellation;
    }
}
