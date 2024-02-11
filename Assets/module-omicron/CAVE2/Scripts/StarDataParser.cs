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
        ParseStarData();

        GenerateStars();

        ParseConstellationData();
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
                star.spect = values[10];
                star.position = new Vector3(float.Parse(values[2]), float.Parse(values[3]), float.Parse(values[4]));
                    starList.Add(values[0].Substring(0, values[0].Length-2),star);
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
        constellationList = JsonUtility.FromJson<ConstellationList>(constellationDataSource.text); ;


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
        foreach (var keyValuePair in starList) {
            var star = keyValuePair.Value;
            GameObject instance = Instantiate(starPrefab, star.position, Quaternion.LookRotation(star.position)) as GameObject;
            instance.GetComponent<StarDataMonobehaviour>().data = star;
            instance.transform.localScale *= scaleRatioOfStars * star.relMag;
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
