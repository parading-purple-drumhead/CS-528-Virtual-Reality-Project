using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class StarDataParser : MonoBehaviour

{
    // for stars
    public TextAsset starDataSource;
    public GameObject starPrefab;
    public Dictionary<string, StarData> starList = new Dictionary<string, StarData>();
    public int numberOfStarsToGenerate = 100000;
    public float scaleRatioOfStars = 0.05f;

    // for constellations
    public TextAsset constellationDataSource;
    public ConstellationList constellationList = new ConstellationList();
    public LineRenderer linePrefab;

    // for exoplanet
    public TextAsset exoplanetDataSource;
    public Dictionary<string,int> exoplanetList = new Dictionary<string,int>();

    private bool usePlanetColorScheme = false;

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
        ParseExoplanetData();

        ParseStarData();

        GenerateStars();

        ParseConstellationData();
    }

    void Update()
    {
        // Check if the space bar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Toggle the color scheme
            usePlanetColorScheme = !usePlanetColorScheme;

            // Update the colors of the stars
            UpdateStarColors();
        }
    }

    void ParseStarData()
    {
        string fullText = starDataSource.text;
        string[] textLines = fullText.Split('\n');

        bool isFirstLine = true;

        foreach (string line in textLines)
            //for (int i = 0; i < numberOfStarsToGenerate; i++)
        {
            // skipping csv header line
            if (isFirstLine)
            {
                isFirstLine = false;
            }
            else
            {
                    string[] values = line.Split(',');
                //string[] values = textLines[i].Split(',');
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

    void ParseConstellationData()
    {
        constellationList = JsonUtility.FromJson<ConstellationList>(constellationDataSource.text);

        foreach (Constellation constellation in constellationList.constellations)
        {
            foreach (Pair pair in constellation.pairs)
            {
                Vector3[] pairPositions = new Vector3[2];

                pairPositions[0] = GetStarPosition(pair.pair[0].ToString());
                pairPositions[1] = GetStarPosition(pair.pair[1].ToString());

                RenderLine(pairPositions);
            }
        }
    }

    Vector3 GetStarPosition(string starId)
    {
        Vector3 starPosition = starList[starId].position;

        return starPosition;
    }

    void GenerateStars()
    {
        foreach (var keyValuePair in starList)
        {
            //var star = keyValuePair.Value;
            //GameObject instance = Instantiate(starPrefab, star.position, Quaternion.LookRotation(star.position)) as GameObject;
            //star.gameObject = instance;
            //instance.transform.localScale *= scaleRatioOfStars * star.relMag;
            //instance.GetComponent<Renderer>().material.color = GetStarColor(star.spect);
            var star = keyValuePair.Value;
            star.instance = Instantiate(starPrefab, star.position, Quaternion.LookRotation(star.position)) as GameObject;
            star.instance.transform.localScale *= scaleRatioOfStars * star.relMag;
            star.instance.GetComponent<Renderer>().material.color = GetStarColor(spect: star.spect);
        }
    }

    void ParseExoplanetData()
    {
        string fullText = exoplanetDataSource.text;
        string[] dataRows = fullText.Split('\n');

        Debug.Log(dataRows.Length);

        bool isFirstRow = true;

        foreach (string row in dataRows)
        //for (int i = 0; i < numberOfStarsToGenerate; i++)
        {
            // skipping csv header line
            if (isFirstRow)
            {
                isFirstRow = false;
            }
            else
            {
                string[] values = row.Split(',');
                //string[] values = textLines[i].Split(',');
                //StarData star = new StarData();
                try
                {
                    //star.absMag = float.Parse(values[5]);
                    //star.relMag = float.Parse(values[6]);
                    //star.dist = float.Parse(values[1]);
                    //star.position = new Vector3(float.Parse(values[2]), float.Parse(values[3]), float.Parse(values[4]));
                    //star.vx = float.Parse(values[7]) * 1.02269E-6f;
                    //star.vy = float.Parse(values[8]) * 1.02269E-6f;
                    //star.vz = float.Parse(values[9]) * 1.02269E-6f;
                    //star.spect = values[10].Trim();
                    //starList.Add(values[0].Substring(0, values[0].Length - 2), star);
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

    //Color GetStarColor(string spect)
    //{
    //    Color orange = new Color(1.0f, 0.64f, 0.0f); // RGB for Orange

    //    if (spect == "O")
    //    {
    //        return Color.blue;
    //    }
    //    else if (spect == "B" || spect == "A")
    //    {
    //        return Color.white;
    //    }
    //    else if (spect == "F" || spect == "G")
    //    {
    //        return Color.yellow;
    //    }
    //    else if (spect == "K")
    //    {
    //        return orange; // Use the custom orange color
    //    }
    //    else if (spect == "M")
    //    {
    //        return Color.red;
    //    }
    //    else
    //    {
    //        return Color.white; // Default color
    //    }
    //}

    Color GetStarColor(string starId = "", string spect = "")
    {
        if (usePlanetColorScheme)
        {
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


    void RenderLine(Vector3[] positions)
    {
        LineRenderer lineRenderer = Instantiate(linePrefab, transform);
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
        lineRenderer.useWorldSpace = true;
    }
}
