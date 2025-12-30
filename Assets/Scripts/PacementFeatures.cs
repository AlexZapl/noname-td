using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class PacementFeatures : MonoBehaviour
{
    public GridGenerator gridGenerator;
    [SerializeField] List<PointFeature> gridDots;

    public LevelSettings levelSettings;
    [SerializeField] List<RectTransform> uiElements;

    Transform obstaclesParent;

    public List<Vector3> pathPoints;
    public float pathY = 0.5f;
    public LineRenderer lineRenderer;

    public GameObject WaterFlag;
    public GameObject RockFlag;
    public GameObject NothingFlag;

    bool isFlagged;
    bool flagDebug; // debug=false --> no flags showed, still exist in list
    bool clickDebug = true;

    [SerializeField] bool lastClickVerification;

    [SerializeField] bool isBuilding; //false means its not setting path nor towers
    [SerializeField] bool isSettingPath; //true - setting path; false - setting towers
    [SerializeField] List<Vector3> pathBuildingPoints = new();


    [SerializeField] GameObject testObj2;
    [SerializeField] GameObject testObj3;
    [SerializeField] GameObject testObj3grid;


    void Start()
    {

        // generating essential objects
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

        //getting settings from level settings
        GameObject levelRootObject = levelSettings.levelRootObject;
        flagDebug = levelSettings.obstacleDebug;
        uiElements = levelSettings.uiElements;


        obstaclesParent = levelRootObject.transform.Find("obstacles");
        if (!obstaclesParent)
        {
            GameObject obj = new GameObject("obstacles"); //Instantiate(new GameObject(), new Vector3(), new Quaternion());;
            obj.transform.SetParent(levelRootObject.transform);
            obstaclesParent = obj.transform;
        }

        gridDots = gridGenerator.gridDots;

        if (clickDebug && !testObj3grid)
        { testObj3grid = Instantiate(testObj3); }
    }



    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousepos = Input.mousePosition;
            lastClickVerification = VerifyClicks(mousepos);
            //print(lastClickVerification);
            if (lastClickVerification && isBuilding && isSettingPath) //settingPath
            {
                if (TryRaycastMouseToWorld(mousepos, out Vector3 result))
                {
                    StartPath(gridLock3D(result, 5f, 0.5f));
                }
            }
        }
        if (Input.GetMouseButton(0) && lastClickVerification)
        {
            Vector2 mousepos = Input.mousePosition;

            if (lastClickVerification && isBuilding && isSettingPath) //settingPath
            {
                if (TryRaycastMouseToWorld(mousepos, out Vector3 result))
                {
                    Vector3 pathPoint = gridLock3D(result, 5f, 0.5f);
                    if (pathPoint != pathBuildingPoints[pathBuildingPoints.Count - 1])
                    {
                        AddPathPoint(pathPoint);
                    }
                }
            }

            if (clickDebug)
            {
                testObj2.transform.position = mousepos;
                if (TryRaycastMouseToWorld(mousepos, out Vector3 debugresult) && lastClickVerification)
                {
                    testObj3.transform.position = debugresult;
                    testObj3grid.transform.position = gridLock3D(debugresult, 5f, 0.5f);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            lastClickVerification = false;

            if (isBuilding && isSettingPath)
            {
                EndPath();
            }
        }

        if (gridDots.Count > 0 && !isFlagged && flagDebug)
        {
            GenerateObstacles();
            isFlagged = true;
        } // debug flag placement
    }



    void GenerateObstacles()
    {
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

    bool VerifyClicks(Vector2 clickpos)
    {
        foreach (RectTransform rt in uiElements)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rt, clickpos))
            {
                return false;
            }
        }
        return true;
    }

    public Vector3 gridLock3D(Vector3 position, float gridSpacing, float gridSpacingY = 1, Vector3 gridCenter = new Vector3())
    {
        // return = round((pos - center) / spacing) * spacing + center

        float x = Mathf.Round((position.x - gridCenter.x) / gridSpacing) * gridSpacing + gridCenter.x;
        float y = Mathf.Round((position.y - gridCenter.y) / gridSpacingY) * gridSpacingY + gridCenter.y;
        float z = Mathf.Round((position.z - gridCenter.z) / gridSpacing) * gridSpacing + gridCenter.z;

        return new(x, y, z);
    }

    public Vector2 gridLock2D(Vector2 position, float gridSpacing, Vector2 gridCenter = new Vector2())
    {
        return gridGenerator.XZtoXY(
            gridLock3D(gridGenerator.XYtoXZ(position), gridSpacing, 1, gridGenerator.XYtoXZ(gridCenter)));
    }

    /*public Vector3 raycastMouseToWorld(Vector2 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        else { return Vector3.positiveInfinity; }
    }*/
    public bool TryRaycastMouseToWorld(Vector3 mousePos, out Vector3 result)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            result = hit.point;
            return true;
        }

        result = Vector3.zero; // Нужно что-то присвоить в любом случае
        return false;
    }


    void StartPath(Vector3 pos)
    {
        Vector3 pos_ = new(pos.x, pathY, pos.z);

        if (pathPoints == null || pathBuildingPoints.Count < 1)
        {
            pathBuildingPoints = new List<Vector3>();
            pathBuildingPoints.Add(pos_);


            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, pos_);
            lineRenderer.SetPosition(1, pos_);
        }
        else
        {
            AddPathPoint(pos_);
        }
    }

    void EndPath()
    {
        pathPoints = pathBuildingPoints;
        isBuilding = false;
        isSettingPath = false;
    }

    void AddPathPoint(Vector3 pos)
    {
        if (pathPoints != null && pathBuildingPoints.Count > 0)
        {
            Vector3 pos_ = new(pos.x, pathY, pos.z);

            if (pathBuildingPoints[pathBuildingPoints.Count - 1] != pos_)
            {
                pathBuildingPoints.Add(pos_);

                lineRenderer.positionCount = pathBuildingPoints.Count;
                lineRenderer.SetPosition(pathBuildingPoints.Count - 1, pos_);
            }
        }
    }
    void RemoveLastPathPoint()
    {
        if (pathPoints != null && pathBuildingPoints.Count > 0)
        {
            pathBuildingPoints.RemoveAt(pathBuildingPoints.Count - 1);
            lineRenderer.positionCount = pathBuildingPoints.Count;
        }
    }
}
