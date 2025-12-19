using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class GridGenerator : MonoBehaviour
{
    public int GridSizeX = 5;
    public int GridSizeY = 5;
    public int GridSpacing = 5;

    [Space(15)]
    [SerializeField] GameObject gridObject;
    [Tooltip("Vector2")]
    public List<Vector2> gridDots = new List<Vector2>();

    [Header("Debug")]

    public GameObject objectForGridPoints;
    public bool debug;


    //private
    float goY = 1;

    void Start()
    {
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
                Vector2 dot = new Vector2(got.position.x+(ix * GridSpacing - (GridSizeX - 1) * GridSpacing * 0.5f), got.position.y + (iy * GridSpacing - (GridSizeY - 1) * GridSpacing * 0.5f));
                gridDots.Add(dot);
            }
        }
        if(debug && objectForGridPoints)
        {
            foreach (Vector2 dot in gridDots)
            {
                GameObject obj = Instantiate(objectForGridPoints, position: XYtoXZ(dot, got.position.y), new Quaternion());
                obj.transform.parent = got;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (debug)
        {
            for (int iy = 0; iy < GridSizeY; iy++) // Horizontal lines
            {
                Debug.DrawLine(XYtoXZ(gridDots[iy * GridSizeY], goY), XYtoXZ(gridDots[iy * GridSizeY + GridSizeX - 1], goY), Color.HSVToRGB(1, 0, Mathf.InverseLerp(0, GridSizeY+6, iy)), 0.1f);
            }
            for (int ix = 0; ix < GridSizeX; ix++) // Vertical lines
            {
                Debug.DrawLine(XYtoXZ(gridDots[ix], goY), XYtoXZ(gridDots[GridSizeY * (GridSizeY - 1) + ix], goY), Color.HSVToRGB(1, Mathf.InverseLerp(0, GridSizeX, ix), 1), 0.1f);            
            }
        }
    }

    Vector3 XYtoXZ(Vector3 point, float ylevel = 1)
    {
        return new Vector3(point.x, ylevel, point.y);
    }

}
