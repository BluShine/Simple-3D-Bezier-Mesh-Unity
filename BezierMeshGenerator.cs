using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BezierMeshGenerator : MonoBehaviour
{
    public List<Vector3> crossSection; //the vertices that are used to extrude a mesh.
    public List<int> capTris; //the triangle indices for the caps at the two ends of the curve.

    public void GenerateMesh()
    {
        BezierCurve curve = GetComponent<BezierCurve>();
        Mesh m = curve.extrudeMesh(crossSection, capTris);
        MeshFilter mFilter = GetComponent<MeshFilter>();
        mFilter.mesh = m;
        MeshCollider mCol = GetComponent<MeshCollider>();
        mCol.sharedMesh = m;
    }
}

[CustomEditor(typeof(BezierMeshGenerator))]
public class BezierMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        BezierMeshGenerator myScript = (BezierMeshGenerator)target;
        if(GUILayout.Button("Generate Mesh"))
        {
            myScript.GenerateMesh();
        }
    }
}