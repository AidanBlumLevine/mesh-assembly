using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Module : MonoBehaviour
{
    public float size = 2;
    public int[] ids = new int[6];
    public float weight = 1;
    public Face[] flatFaces = new Face[6];
    public int[] availableLayers = new int[0];
    const int gridsize = 10;
    void Start()
    {
        //CalculateFaces();
    }

    public bool canFit(List<int>[] neighbors, int y)
    {
        if(availableLayers.Length > 0 && !availableLayers.Contains(y)){
            return false;
        }
        for (int i = 0; i < 6; i++)
        {
            if (!neighbors[i].Contains(ids[i]))
            {
                return false;
            }
        }
        return true;
    }

    void CalculateFaces()
    {
        List<Vector3>[] faces = new List<Vector3>[6];
        for (int i = 0; i < 6; i++)
        {
            faces[i] = new List<Vector3>();
        }
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
        {
            for (int i = 0; i < 6; i++)
            {
                flatFaces[i] = new Face();
                flatFaces[i].grid = new bool[gridsize * gridsize];
            }
            return;
        }
        Vector3[] verts = mesh.vertices;
        Vector3[] tVerts = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            tVerts[i] = transform.rotation * verts[i];
        }
        foreach (Vector3 tVert in tVerts)
        {
            if (Approximately(tVert.x, size / 2))
            {
                faces[0].Add(tVert);
            }
            if (Approximately(tVert.x, -size / 2))
            {
                faces[1].Add(tVert);
            }
            if (Approximately(tVert.y, size / 2))
            {
                faces[2].Add(tVert);
            }
            if (Approximately(tVert.y, -size / 2))
            {
                faces[3].Add(tVert);
            }
            if (Approximately(tVert.z, size / 2))
            {
                faces[4].Add(tVert);
            }
            if (Approximately(tVert.z, -size / 2))
            {
                faces[5].Add(tVert);
            }
        }
        //flatten and gridify
        for (int i = 0; i < 6; i++)
        {
            //fetch
            List<Vector3[]> faceTriangles = new List<Vector3[]>();
            for (int t = 0; t < mesh.triangles.Length; t += 3)
            {
                if (faces[i].Contains(tVerts[mesh.triangles[t]]) &&
                faces[i].Contains(tVerts[mesh.triangles[t + 1]]) &&
                faces[i].Contains(tVerts[mesh.triangles[t + 2]]))
                {
                    int[] test = mesh.triangles;
                    faceTriangles.Add(new Vector3[]{
                        tVerts[mesh.triangles[t]],
                        tVerts[mesh.triangles[t + 1]],
                        tVerts[mesh.triangles[t + 2]]
                    });
                }
            }
            flatFaces[i] = new Face
            {
                triangles = new Vector2[faceTriangles.Count][],
                grid = new bool[gridsize * gridsize]
            };
            //flatted
            for (int j = 0; j < flatFaces[i].triangles.Length; j++)
            {
                flatFaces[i].triangles[j] = new Vector2[3];
                for (int v = 0; v < 3; v++)
                {
                    if (i == 0 || i == 1)
                    {
                        flatFaces[i].triangles[j][v].x = faceTriangles[j][v].y;
                        flatFaces[i].triangles[j][v].y = faceTriangles[j][v].z;
                    }
                    if (i == 2 || i == 3)
                    {
                        flatFaces[i].triangles[j][v].x = faceTriangles[j][v].x;
                        flatFaces[i].triangles[j][v].y = faceTriangles[j][v].z;
                    }
                    if (i == 4 || i == 5)
                    {
                        flatFaces[i].triangles[j][v].x = faceTriangles[j][v].x;
                        flatFaces[i].triangles[j][v].y = faceTriangles[j][v].y;
                    }
                }
            }
            //gridify
            for (int x = 0; x < gridsize; x++)
            {
                for (int y = 0; y < gridsize; y++)
                {
                    Vector2 realPoint = (new Vector2(x, y) * size / gridsize) + (Vector2.one * (-size / 2 + size / (2 * gridsize)));
                    flatFaces[i].grid[x + gridsize * y] = pointInAnyTriangle(realPoint, flatFaces[i].triangles);
                }
            }
        }
    }

    bool Approximately(float x, float y)
    {
        return Mathf.Abs(x - y) < .01;
    }

    bool pointInAnyTriangle(Vector2 p, Vector2[][] tris)
    {
        foreach (Vector2[] tri in tris)
        {
            if (pointInTriangle(p, tri[0], tri[1], tri[2]) || pointInTriangle(p + new Vector2(.001f, .001f), tri[0], tri[1], tri[2])  || pointInTriangle(p + new Vector2(-.001f, -.001f), tri[0], tri[1], tri[2]))
            {
                return true;
            }
        }
        return false;
    }

    bool pointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var dX = p.x - p2.x;
        var dY = p.y - p2.y;
        var dX21 = p2.x - p1.x;
        var dY12 = p1.y - p2.y;
        var D = dY12 * (p0.x - p2.x) + dX21 * (p0.y - p2.y);
        var s = dY12 * dX + dX21 * dY;
        var t = (p2.y - p0.y) * dX + (p0.x - p2.x) * dY;
        if (D < 0) return s <= 0 && t <= 0 && s + t >= D;
        return s >= 0 && t >= 0 && s + t <= D;
    }

    public Face getFlatFace(int i)
    {
        return flatFaces[i];
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < 6; i++)
            {
                Random.InitState(ids[i]);
                Color sideColor = new Color(
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f)
                );
                Gizmos.color = sideColor;
                // foreach (Vector2[] tri in flatFaces[i].triangles)
                // {
                //     Vector3[] tri3D = new Vector3[3];
                //     for (int j = 0; j < 3; j++)
                //     {
                //         if (i == 0 || i == 1)
                //         {
                //             tri3D[j].y = tri[j].x;
                //             tri3D[j].z = tri[j].y;
                //             tri3D[j].x = size / 2 * (i == 0 ? 1 : -1);
                //         }
                //         if (i == 2 || i == 3)
                //         {
                //             tri3D[j].x = tri[j].x;
                //             tri3D[j].z = tri[j].y;
                //             tri3D[j].y = size / 2 * (i == 2 ? 1 : -1);
                //         }
                //         if (i == 4 || i == 5)
                //         {
                //             tri3D[j].x = tri[j].x;
                //             tri3D[j].y = tri[j].y;
                //             tri3D[j].z = size / 2 * (i == 4 ? 1 : -1);
                //         }
                //     }
                //     tri3D[0] += transform.position;
                //     tri3D[1] += transform.position;
                //     tri3D[2] += transform.position;
                //     tri3D[0] = Vector3.MoveTowards(tri3D[0], (tri3D[0] + tri3D[1] + tri3D[2]) / 3, .03f);
                //     tri3D[1] = Vector3.MoveTowards(tri3D[1], (tri3D[0] + tri3D[1] + tri3D[2]) / 3, .03f);
                //     tri3D[2] = Vector3.MoveTowards(tri3D[2], (tri3D[0] + tri3D[1] + tri3D[2]) / 3, .03f);
                //     Gizmos.DrawLine(tri3D[0], tri3D[1]);
                //     Gizmos.DrawLine(tri3D[1], tri3D[2]);
                //     Gizmos.DrawLine(tri3D[2], tri3D[0]);
                //     //Gizmos.DrawSphere((tri3D[0] + tri3D[1] + tri3D[2]) / 3, .1f);
                // }

                for (int x = 0; x < gridsize; x++)
                {
                    for (int y = 0; y < gridsize; y++)
                    {
                        Vector2 realPoint = (new Vector2(x, y) * size / gridsize) + (Vector2.one * (-size / 2 + size / (2 * gridsize)));
                        Gizmos.color = flatFaces[i].grid[x + gridsize * y] ? sideColor : Color.white;
                        Vector3 point = new Vector3();
                        if (i == 0 || i == 1)
                        {
                            point.y = realPoint.x;
                            point.z = realPoint.y;
                            point.x = size / 2 * (i == 0 ? 1 : -1);
                        }
                        if (i == 2 || i == 3)
                        {
                            point.x = realPoint.x;
                            point.z = realPoint.y;
                            point.y = size / 2 * (i == 2 ? 1 : -1);
                        }
                        if (i == 4 || i == 5)
                        {
                            point.x = realPoint.x;
                            point.y = realPoint.y;
                            point.z = size / 2 * (i == 4 ? 1 : -1);
                        }
                        Gizmos.DrawSphere(point + transform.position, .05f);
                    }
                }
            }
        }
        else
        {
            Gizmos.color = new Color(0, 0, 1, 0.25f);
            Gizmos.DrawCube(transform.position, Vector3.one * size);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, Vector3.one * size);
    }

    public void init()
    {
        CalculateFaces();
    }
}

[System.Serializable]
public struct Face
{
    public Vector2[][] triangles;
    public bool[] grid;
}

[CustomEditor(typeof(Module))]
[CanEditMultipleObjects]
public class ModuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Module mod = (Module)target;
        if (GUILayout.Button("create rotated versions"))
        {
            Instantiate(mod, mod.transform.position + new Vector3(0, -3, 0), Quaternion.AngleAxis(90, Vector3.right));
            Instantiate(mod, mod.transform.position + new Vector3(0, -6, 0), Quaternion.AngleAxis(180, Vector3.right));
            Instantiate(mod, mod.transform.position + new Vector3(0, -9, 0), Quaternion.AngleAxis(270, Vector3.right));
            Instantiate(mod, mod.transform.position + new Vector3(0, -12, 0), Quaternion.AngleAxis(90, Vector3.up));
            Instantiate(mod, mod.transform.position + new Vector3(0, -15, 0), Quaternion.AngleAxis(270, Vector3.up));
            Instantiate(mod, mod.transform.position + new Vector3(0, -18, 0), Quaternion.AngleAxis(90, Vector3.forward));
            Instantiate(mod, mod.transform.position + new Vector3(0, -21, 0), Quaternion.AngleAxis(270, Vector3.forward));
        }

        if (GUILayout.Button("flat rotation"))
        {
            Instantiate(mod, mod.transform.position + new Vector3(0, -3, 0), Quaternion.AngleAxis(90, Vector3.up)).GetComponent<Module>();
            Instantiate(mod, mod.transform.position + new Vector3(0, -6, 0), Quaternion.AngleAxis(180, Vector3.up)).GetComponent<Module>();
            Instantiate(mod, mod.transform.position + new Vector3(0, -9, 0), Quaternion.AngleAxis(270, Vector3.up)).GetComponent<Module>();
        }
        if (GUILayout.Button("delete rotated versions"))
        {
            GameObject[] gs = GameObject.FindObjectsOfType<GameObject>().Where(obj => obj.name.Equals(mod.gameObject.name + "(Clone)")).ToArray();
            for (int i = 0; i < gs.Length; i++)
            {
                DestroyImmediate(gs[i]);
            }
        }
    }
}