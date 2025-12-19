using System.Collections.Generic;
using UnityEngine;

public class PacementFeatures : MonoBehaviour
{
    public GridGenerator gridGenerator;
    [SerializeField] List<PointFeature> gridDots;
    public LevelSettings levelSettings;
    Transform obstaclesParent;

    public GameObject WaterFlag;
    public GameObject RockFlag;
    public GameObject NothingFlag;

    bool isFlagged;
    bool debug; // debug=false --> no flags showed, still exist in list

    void Start()
    {
        if (!levelSettings)
        {
            levelSettings = gameObject.GetComponent<LevelSettings>();
        }

        if (!gridGenerator)
        {
            gridGenerator = gameObject.GetComponent<GridGenerator>();
            if (!gridGenerator)
            {
                gridGenerator = gameObject.AddComponent<GridGenerator>();
            }
        }

        GameObject levelRootObject = levelSettings.levelRootObject;
        debug = levelSettings.obstacleDebug;

        obstaclesParent = levelRootObject.transform.Find("obstacles");
        if (!obstaclesParent)
        {
            GameObject obj = new GameObject("obstacles"); //Instantiate(new GameObject(), new Vector3(), new Quaternion());;
            obj.transform.SetParent(levelRootObject.transform);
            obstaclesParent = obj.transform;
        }

        gridDots = gridGenerator.gridDots;
    }



    void Update()
    {
        if (gridDots.Count > 0 && !isFlagged && debug)
        {
            GenerateObstacles();
            isFlagged = true;
        }
    }



    void GenerateObstacles()
    {
        print(gridDots.Count);
        foreach (PointFeature p in gridDots)
        {
            if (p.pointType == PointType.Water)
            {
                GameObject obj = Instantiate(WaterFlag, position: gridGenerator.XYtoXZ(p.position, gridGenerator.goY), new Quaternion());
                obj.transform.SetParent(obstaclesParent);
            }
            else if (p.pointType == PointType.Rock)
            {
                GameObject obj = Instantiate(RockFlag, position: gridGenerator.XYtoXZ(p.position, gridGenerator.goY), new Quaternion());
                obj.transform.SetParent(obstaclesParent);
            }
            else if (p.pointType == PointType.Nothing)
            {
                GameObject obj = Instantiate(NothingFlag, position: gridGenerator.XYtoXZ(p.position, gridGenerator.goY), new Quaternion());
                obj.transform.SetParent(obstaclesParent);
            }
        }
    }
}
