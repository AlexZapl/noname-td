using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PacementFeatures : MonoBehaviour
{
    public GridGenerator gridGenerator;
    [SerializeField] List<PointFeature> gridDots;

    public LevelSettings levelSettings;
    [SerializeField] List<RectTransform> uiElements;

    Transform obstaclesParent;

    public GameObject WaterFlag;
    public GameObject RockFlag;
    public GameObject NothingFlag;

    bool isFlagged;
    bool flagDebug; // debug=false --> no flags showed, still exist in list
    bool clickDebug = true;

    [SerializeField] bool lastClickVerification;

    [SerializeField] bool isBuilding; //false means its not setting path nor towers
    [SerializeField] bool isSettingPath; //true - setting path; false - setting towers
    [SerializeField] Vector2 buildPressPoint;

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
            if (isBuilding && lastClickVerification)
            {
                buildPressPoint = mousepos;
            }
        }
        if (Input.GetMouseButton(0) && lastClickVerification)
        {
            Vector2 mousepos = Input.mousePosition;
            if (clickDebug)
            {
                testObj2.transform.position = mousepos;
                testObj3.transform.position = raycastMouseToWorld(mousepos);
                testObj3grid.transform.position = gridLock3D(raycastMouseToWorld(mousepos), 5f, 0.5f);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            lastClickVerification = false;
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

    public Vector3 raycastMouseToWorld(Vector2 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        else { return Vector3.zero; }
    }
}
