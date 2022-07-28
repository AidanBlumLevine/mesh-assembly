using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class Cell : MonoBehaviour
{
    public Vector3 size, location;
    float voptions = 1;
    public Cell[] neighbors;
    public List<Module> possibleModules = new List<Module>();
    List<Module> frameBackup;
    public int totalModules;
    bool r, giz = true;
    void Start()
    {
        calculatePossibleModules();
        entropy();
    }

    void Update()
    {
        frameBackup = new List<Module>(possibleModules);
        r = false;
    }

    public bool SetRandom()
    {
        possibleModules = new List<Module> { PickRandom() };
        SetVisibly();
        return AlertNeighbors();
    }

    bool AlertNeighbors()
    {
        r = true;
        bool failed = false;
        foreach (Cell c in neighbors)
        {
            if (c != null)
            {
                if (c.Recalculate())
                {
                    failed = true;
                }
            }
        }
        return failed;
    }

    public bool Recalculate()
    {
        if (!r && possibleModules.Count > 1)
        {
            int len = possibleModules.Count;
            calculatePossibleModules();
            entropy();
            if (possibleModules.Count < len)
            {
                if (possibleModules.Count == 0)
                {
                    return true;
                }
                return AlertNeighbors();
            }
        }
        return false;
    }

    void SetVisibly()
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = possibleModules[0].gameObject.GetComponent<MeshFilter>().mesh;
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.materials = possibleModules[0].gameObject.GetComponent<MeshRenderer>().materials;
        transform.rotation = possibleModules[0].transform.rotation;
        giz = false;
    }

    Module PickRandom()
    {
        float sum = 0;
        foreach (Module m in possibleModules)
        {
            sum += m.weight;
        }
        float random = Random.Range(0, sum);
        sum = 0;
        foreach (Module m in possibleModules)
        {
            sum += m.weight;
            if (sum > random)
            {
                return m;
            }
        }
        return null;
    }

    public float entropy()
    {
        voptions = possibleModules.Count / (float)totalModules;
        if (possibleModules.Count < 2)
        {
            return -1;
        }
        float weightsum = 0;
        foreach (Module m in possibleModules)
        {
            weightsum += m.weight;
        }
        float e = Mathf.Log(weightsum + .000001f);
        foreach (Module m in possibleModules)
        {
            e -= m.weight * Mathf.Log(m.weight + .000001f) / (weightsum + .000001f);
        }
        return e + Random.Range(0, 0.0000001f);
    }

    void calculatePossibleModules()
    {
        List<int>[] shapeLists = availableHere();
        for (int i = 0; i < possibleModules.Count; i++)
        {
            if (!possibleModules[i].canFit(shapeLists, (int)location.y))
            {
                possibleModules.RemoveAt(i);
                i--;
            }
        }
        if (possibleModules.Count == 1)
        {
            SetVisibly();
        }
    }

    List<int>[] availableHere()
    {
        List<int>[] sides = new List<int>[6];
        for (int i = 0; i < 6; i++)
        {
            int opposite = i + (i % 2 == 0 ? 1 : -1);
            if (neighbors[i] == null)
            {
                sides[i] = new List<int> { 0 }; //edge of area
            }
            else
            {
                sides[i] = neighbors[i].availableShapes(opposite);
            }
        }
        return sides;
    }

    public List<int> availableShapes(int side)
    {
        List<int> shapeIDs = new List<int>();
        for (int i = 0; i < possibleModules.Count; i++)
        {
            shapeIDs.Add(possibleModules[i].ids[side]);
        }
        return shapeIDs;
    }

    public void Set(int s, Cell[] n, Module[] m)
    {
        possibleModules.AddRange(m);
        totalModules = m.Length;
        neighbors = n;
        size = new Vector3(s - .05f, s - .05f, s - .05f);
    }
    void OnDrawGizmos()
    {
        if (giz)
        {
            Gizmos.color = new Color(0, 0, 0, .11f);
            Gizmos.DrawWireCube(transform.position, size * voptions);
        }
    }
}

[CustomEditor(typeof(Cell))]
public class CellEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Cell cell = (Cell)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("show options"))
        {
            foreach(Module m in cell.possibleModules){
                Instantiate(m,cell.transform.position,m.transform.rotation);
            }
        }
    }
}