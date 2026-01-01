using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    float gridSpacing;
    float placementGridSpacing;

    public GameObject WaterFlag;
    public GameObject RockFlag;
    public GameObject NothingFlag;

    bool isFlagged;
    bool flagDebug; // debug=false --> no flags showed, still exist in list
    bool clickDebug = true;
    bool astarDebug;

    [SerializeField] bool lastClickVerification;

    public bool isBuilding; //false means its not setting path nor towers
    [SerializeField] bool isSettingPath = true; //true - setting path; false - setting towers
    [SerializeField] List<Vector3> pathBuildingPoints = new();
    Vector3 lastDeletedVector;


    [SerializeField] GameObject testObj2;
    [SerializeField] GameObject testObj3;
    [SerializeField] GameObject testObj3grid;

    [SerializeField] Vector3 start, end;
    [SerializeField] LineRenderer pathAStarRenderer;

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
        astarDebug = levelSettings.astarDebug;
        uiElements = levelSettings.uiElements;
        gridSpacing = levelSettings.gridSpacing;
        placementGridSpacing = gridSpacing / levelSettings.placementGridSpacingMultiplier;


        obstaclesParent = levelRootObject.transform.Find("obstacles");
        if (!obstaclesParent)
        {
            GameObject obj = new GameObject("obstacles"); //Instantiate(new GameObject(), new Vector3(), new Quaternion());;
            obj.transform.SetParent(levelRootObject.transform);
            obstaclesParent = obj.transform;
        }

        if (clickDebug && !testObj3grid)
        { testObj3grid = Instantiate(testObj3); }


        //grid generating
        gridDots = gridGenerator.GenerateGrid();

        int regenCount = 0;
        List<Vector3> shortestPath = FindAStarPath(start,end);
        while (shortestPath.Count < 1)
        {
            if (regenCount > 10) // if more than 3 regens then reload scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name); 

            if (regenCount < 4) //first regen on same seed
            {
                gridDots = gridGenerator.GenerateGrid();
                regenCount++;
            }
            else //second and third on different seeds
            {
                gridDots = gridGenerator.RegenerateGrid();
                regenCount++;
            }

            shortestPath = FindAStarPath(start, end);
        }
        pathAStarRenderer.positionCount = shortestPath.Count;
        for (int i = 0; i < shortestPath.Count; i++)
        {
            pathAStarRenderer.SetPosition(i, shortestPath[i]);
        }
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
                // Внутри Update, где обрабатывается Input.GetMouseButton(0)
                if (TryRaycastMouseToWorld(mousepos, out Vector3 result))
                {
                    Vector3 pathPoint = gridLock3D(result, 5f, 0.5f);
                    Vector3 pathPoint_ = new Vector3(pathPoint.x, pathY, pathPoint.z);
                                        
                    if (pathBuildingPoints.Count >= 2 && pathPoint_ == pathBuildingPoints[pathBuildingPoints.Count - 2])
                    {
                        RemoveLastPathPoint();
                        lastDeletedVector = pathPoint_;
                    }
                    else if (pathBuildingPoints.Count > 0 && pathPoint_ != pathBuildingPoints[pathBuildingPoints.Count - 1] && pathPoint_ != lastDeletedVector)
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
        if (pathBuildingPoints.Count > 0)
        {
            Vector3 targetPos = new Vector3(pos.x, pathY, pos.z);

            // Цикл "достраивания": пока мы не дотянулись до целевой точки соседа
            while (pathBuildingPoints[pathBuildingPoints.Count - 1] != targetPos)
            {
                Vector3 last = pathBuildingPoints[pathBuildingPoints.Count - 1];
                Vector3 nextStep = last;

                // Определяем направление шага (только по одной оси за раз, чтобы не было диагоналей)
                if (Mathf.Abs(targetPos.x - last.x) > 0.1f)
                {
                    nextStep.x += Mathf.Sign(targetPos.x - last.x) * 5f; // 5f - ваш spacing
                }
                else if (Mathf.Abs(targetPos.z - last.z) > 0.1f)
                {
                    nextStep.z += Mathf.Sign(targetPos.z - last.z) * 5f;
                }

                pathBuildingPoints.Add(nextStep);

                lineRenderer.positionCount = pathBuildingPoints.Count;
                lineRenderer.SetPosition(pathBuildingPoints.Count - 1, nextStep);

                if (pathBuildingPoints.Count > 50) break;
            }
        }
    }
    void RemoveLastPathPoint()
    {
        if (pathBuildingPoints != null && pathBuildingPoints.Count > 0)
        {
            pathBuildingPoints.RemoveAt(pathBuildingPoints.Count - 1);
            lineRenderer.positionCount = pathBuildingPoints.Count;
        }
    }



    //--------------------       A* pathfinding           --------------------
    public class Node
    {
        public Vector3 position;
        public float gCost; // Distance from start
        public float hCost; // Distance from end (эвристика)
        public float fCost => gCost + hCost; // Both distances
        public Node parent;

        public Node(Vector3 pos) => position = pos;
    }

    public List<Vector3> FindAStarPath(Vector3 startPos, Vector3 targetPos)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Vector3> closedSet = new HashSet<Vector3>();

        if (astarDebug)
        {
            Debug.DrawRay(startPos, Vector3.up * 10, Color.yellow, 10);
            Debug.DrawRay(targetPos, Vector3.up * 10, Color.yellow, 10);
        }

        Node startNode = new Node(new Vector3(startPos.x, 0.5f, startPos.z));
        Node targetNode = new Node(new Vector3(targetPos.x, 0.5f, targetPos.z));
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {;
            // finds least fCost
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    currentNode = openSet[i];
            }

            if (astarDebug) Debug.DrawRay(currentNode.position, Vector3.up*6, Color.red, 10);

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.position);

            // if current node pos == target pos then retrace and return path
            if (currentNode.position == new Vector3(targetPos.x, 0.5f, targetPos.z))
                return RetracePath(startNode, currentNode);

            // checks 4 neighbours (no diagonals)
            foreach (Vector3 neighborPos in GetNeighbors(currentNode.position))
            {
                if (astarDebug) Debug.DrawRay(neighborPos, Vector3.up*3, Color.green, 10);
                if (closedSet.Contains(neighborPos) || IsObstacle(neighborPos)) continue;

                float newMovementCostToNeighbor = currentNode.gCost + 5f;
                Node neighbor = openSet.Find(n => n.position == neighborPos) ?? new Node(neighborPos);

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = Vector3.Distance(neighbor.position, targetNode.position);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                    else openSet.Remove(currentNode);
                }
            }
        }
        print("a* found nothing");
        return new List<Vector3>(); // no path
    }

    private List<Vector3> GetNeighbors(Vector3 pos)
    {
        return new List<Vector3> {
        pos + new Vector3(gridSpacing, 0, 0),  
        pos + new Vector3(-gridSpacing, 0, 0),
        pos + new Vector3(0, 0, gridSpacing),  
        pos + new Vector3(0, 0, -gridSpacing)
    };
    }

    private List<Vector3> RetracePath(Node start, Node end)
    {
        print("retracing");
        List<Vector3> path = new List<Vector3>();
        Node curr = end;
        while (curr != null) { path.Add(curr.position); curr = curr.parent; }
        path.Reverse();
        print(path.ToString());
        return path;
    }

    private bool IsObstacle(Vector3 pointPos)
    {
        Vector2 targetPos = new(pointPos.x, pointPos.z);
        PointFeature dot = gridDots.Find(p => Vector2.Distance(p.position, targetPos) < 0.1f);

        if (dot == null) return true; //if dot isnt on board

        return dot.pointType != PointType.Land; //if it is then check point type
    }
}
