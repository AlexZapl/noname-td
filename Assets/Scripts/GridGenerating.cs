using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class GridGenerator : MonoBehaviour
{
    LevelSettings levelSettings;


    public int GridSizeX = 5;
    public int GridSizeY = 5;
    public int GridSpacing = 5;

    [Space(15)]
    public GameObject gridObject;
    public List<PointFeature> gridDots = new List<PointFeature>();

    [Header("Noise features")]

    [SerializeField] float NoiseFrequency = 0.1f; //Perlin noise frequency
    [SerializeField] int seed; //                   Perlin noise seed

    [SerializeField] float RockTreshold = 0.9f; //from max (1-0)

    [SerializeField] float nothingChances = 0.05f; //random percentage

    [SerializeField] float WaterTreshold = 0.3f; //from min (0-1)

    [Header("Debug")]

    public GameObject objectForGridPoints;
    public bool debug;

    public float goY = 1;

    void Start()
    {
        if (!levelSettings)
        {
            levelSettings = gameObject.GetComponent<LevelSettings>();
        }

        NoiseFrequency = levelSettings.NoiseFrequency;
        seed = levelSettings.seed;
        RockTreshold = levelSettings.RockTreshold;
        nothingChances = levelSettings.nothingChances;
        WaterTreshold = levelSettings.WaterTreshold;

        GridSizeX = levelSettings.gridSizeX;
        GridSizeY = levelSettings.gridSizeY;
        GridSpacing = levelSettings.gridSpacing;

        gridObject = levelSettings.levelRootObject;

        if (!gridObject)
        {
            gridObject = gameObject;
        }

        Transform got = gridObject.transform;
        goY = got.position.y+0.5f;

        for (int iy = 0; iy < GridSizeY; iy++)
        {
            for (int ix = 0; ix < GridSizeY; ix++)
            {
                Vector2 dotPos = new Vector2(got.position.x+(ix * GridSpacing - (GridSizeX - 1) * GridSpacing * 0.5f), // x
                    got.position.y + (iy * GridSpacing - (GridSizeY - 1) * GridSpacing * 0.5f)); //                       y

                float noiseValue = Mathf.PerlinNoise(ix * NoiseFrequency + seed, iy * NoiseFrequency + seed);
                PointType type = new PointType();
                if (noiseValue < WaterTreshold)
                {
                    type = PointType.Water;
                } else if (noiseValue > RockTreshold)
                {
                    type = PointType.Rock;
                } else if (Random.Range(0f,1f) < nothingChances)
                {
                    type = PointType.Nothing;
                } else
                {
                    type = PointType.Land;
                }

                PointFeature point = new PointFeature();
                point.position = dotPos;
                point.pointType = type;
                gridDots.Add(point);
            }
        }
        if(debug && objectForGridPoints)
        {
            foreach (var dot in gridDots)
            {
                GameObject obj = Instantiate(objectForGridPoints, position: XYtoXZ(dot.position, got.position.y), new Quaternion());
                obj.transform.parent = got;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        debug = levelSettings.gridDebug;

        if (debug)
        {
            for (int iy = 0; iy < GridSizeY; iy++) // Horizontal lines
            {
                Debug.DrawLine(XYtoXZ(gridDots[iy * GridSizeY].position, goY), XYtoXZ(gridDots[iy * GridSizeY + GridSizeX - 1].position, goY), Color.HSVToRGB(1, 0, Mathf.InverseLerp(0, GridSizeY+6, iy)), 0.1f);
            }
            for (int ix = 0; ix < GridSizeX; ix++) // Vertical lines
            {
                Debug.DrawLine(XYtoXZ(gridDots[ix].position, goY), XYtoXZ(gridDots[GridSizeY * (GridSizeY - 1) + ix].position, goY), Color.HSVToRGB(1, Mathf.InverseLerp(0, GridSizeX, ix), 1), 0.1f);            
            }
        }
    }

    public Vector3 XYtoXZ(Vector3 point, float ylevel = 1)
    {
        return new Vector3(point.x, ylevel, point.y);
    }

}
