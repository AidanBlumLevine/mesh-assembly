using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Manager : MonoBehaviour
{
    public Cell[,,] cells;
    public int cellSize = 2;
    public Vector3 size = new Vector3(5, 5, 5);
    bool running = true;
    void Start()
    {
        cells = new Cell[(int)size.x, (int)size.y, (int)size.z];
        foreach (Cell c in Object.FindObjectsOfType<Cell>())
        {
            cells[(int)c.location.x, (int)c.location.y, (int)c.location.z] = c;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            running = !running;
        }
        if (running)
        {
            Cell min = null;
            float minEntropy = float.MaxValue;
            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    for (int z = 0; z < cells.GetLength(2); z++)
                    {
                        float temp = cells[x, y, z].entropy();
                        if (temp != -1 && temp < minEntropy)
                        { //0 means it doesnt need to be updated because its locked
                            minEntropy = temp;
                            min = cells[x, y, z];
                        }
                    }
                }
            }
            if (min != null)
            {
                if (min.SetRandom())
                {
                    Reset();
                }
            }
        }
    }
    void Reset()
    {
        ClearCells();
        CreateCells();
    }

    public void ClearCells()
    {
        Cell[] cells = Object.FindObjectsOfType<Cell>();
        for (int i = 0; i < cells.Length; i++)
        {
            DestroyImmediate(cells[i].gameObject);
        }
    }
    public void CreateCells()
    {
        Module[] modules = Object.FindObjectsOfType<Module>();
        Manager manager = this;
        manager.cells = new Cell[(int)manager.size.x, (int)manager.size.y, (int)manager.size.z];
        for (int x = 0; x < manager.size.x; x++)
        {
            for (int y = 0; y < manager.size.y; y++)
            {
                for (int z = 0; z < manager.size.z; z++)
                {
                    GameObject g = new GameObject();
                    g.transform.position = new Vector3(x, y, z) * manager.cellSize;
                    Cell c = g.AddComponent<Cell>();
                    manager.cells[x, y, z] = c;
                    c.location = new Vector3(x, y, z);
                }
            }
        }
        for (int x = 0; x < manager.size.x; x++)
        {
            for (int y = 0; y < manager.size.y; y++)
            {
                for (int z = 0; z < manager.size.z; z++)
                {
                    Cell xplus = x + 1 < manager.size.x ? manager.cells[x + 1, y, z] : null;
                    Cell xminus = x > 0 ? manager.cells[x - 1, y, z] : null;
                    Cell yplus = y + 1 < manager.size.y ? manager.cells[x, y + 1, z] : null;
                    Cell yminus = y > 0 ? manager.cells[x, y - 1, z] : null;
                    Cell zplus = z + 1 < manager.size.z ? manager.cells[x, y, z + 1] : null;
                    Cell zminus = z > 0 ? manager.cells[x, y, z - 1] : null;
                    Cell[] neighbors = new Cell[]{
                            xplus,
                            xminus,
                            yplus,
                            yminus,
                            zplus,
                            zminus,
                        };
                    manager.cells[x, y, z].Set(manager.cellSize, neighbors, modules);
                }
            }
        }
    }
}



[CustomEditor(typeof(Manager))]
public class ManagerEditor : Editor
{
    int nextID = 1;
    List<ShapeGroup> shapeGroups = new List<ShapeGroup>();


    public override void OnInspectorGUI()
    {
        Manager manager = (Manager)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("create groups"))
        {
            CreateShapeGroups();
        }
        if (GUILayout.Button("clear cells"))
        {
            manager.ClearCells();
        }
        if (GUILayout.Button("instantiate cells"))
        {
            manager.CreateCells();
        }
    }

    void CreateShapeGroups()
    {
        Module[] modules = Object.FindObjectsOfType<Module>();
        foreach (Module m in modules)
        {
            m.init();
            int[] moduleGroups = new int[6];
            for (int i = 0; i < 6; i++)
            {
                Face face = m.getFlatFace(i);
                ShapeGroup found = shapeGroups.Find((x) => matchShapes(face, x));
                if (!found.exists)
                {
                    ShapeGroup group = new ShapeGroup
                    {
                        face = face,
                        id = face.triangles.Length == 0 ? 0 : nextID++,
                        exists = true
                    };
                    shapeGroups.Add(group);
                    moduleGroups[i] = group.id;
                }
                else
                {
                    moduleGroups[i] = found.id;
                }
            }
            m.ids = moduleGroups;
        }
    }

    bool matchShapes(Face face, ShapeGroup shape)
    {
        for (int i = 0; i < face.grid.Length; i++)
        {
            if (face.grid[i] != shape.face.grid[i])
            {
                return false;
            }
        }
        return true;
    }
}

struct ShapeGroup
{
    public Face face;
    public int id;
    public bool exists;
}