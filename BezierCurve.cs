using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour {

    public List<BezierSegment> line;
    public Vector3 startUp;
    public int segments = 10;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Mesh extrudeMesh(List<Vector3> sourceVerts, List<int> capTris)
    {
        Mesh mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        //generate curve
        Vector3 lastPos = Vector3.zero;
        Vector3 lastUp = startUp;
        Vector3 lastNormUp = Vector3.zero;
        Vector3 lastNormLeft = Vector3.zero;
        Vector3 startNormleft = Vector3.zero;
        Vector3 startNormUp = Vector3.zero;
        for(int j = 0; j < line.Count; j++)
        {
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                float t2 = (i + 1f) / segments;
                float t3 = (i + 2f) / segments;
                Vector3 p1 = getPoint(t, lastPos, line[j]);
                Vector3 p2 = getPoint(t2, lastPos, line[j]);
                Vector3 forward = p2 - p1;
                if (lastNormUp == Vector3.zero) {
                    lastNormUp = getNormal(t, lastUp, line[j], p2 - p1);
                    lastNormLeft = Vector3.Cross(forward.normalized, lastNormUp);
                    startNormleft = lastNormLeft;
                    startNormUp = lastNormUp;
                }
                Vector3 nextUp;
                Vector3 nextLeft;
                if (t3 <= 1) {
                    Vector3 p3 = getPoint(t3, lastPos, line[j]);
                    Vector3 midpointForward = p3 - p1;
                    nextUp = getNormal(t2, lastUp, line[j], midpointForward);
                    nextLeft = Vector3.Cross(midpointForward.normalized, nextUp);
                } else
                {
                    nextUp = getNormal(t2, lastUp, line[j], p2 - p1);
                    nextLeft = Vector3.Cross(forward.normalized, nextUp);
                }

                for(int k = 0; k < sourceVerts.Count; k++)
                {
                    Vector3 vert1 = sourceVerts[k];
                    Vector3 vert2 = sourceVerts[(k + 1) % sourceVerts.Count];
                    verts.Add(p1 + vert1.y * lastNormUp + vert1.x * lastNormLeft);
                    verts.Add(p2 + vert1.y * nextUp + vert1.x * nextLeft);
                    verts.Add(p1 + vert2.y * lastNormUp + vert2.x * lastNormLeft);
                    verts.Add(p2 + vert2.y * nextUp + vert2.x * nextLeft);
                    uvs.Add(new Vector2(k / (float)sourceVerts.Count, t));
                    uvs.Add(new Vector2(k / (float)sourceVerts.Count, t2));
                    uvs.Add(new Vector2((k + 1) / (float)sourceVerts.Count, t));
                    uvs.Add(new Vector2((k + 1) / (float)sourceVerts.Count, t2));
                    tris.Add(verts.Count - 4);
                    tris.Add(verts.Count - 2);
                    tris.Add(verts.Count - 3);
                    tris.Add(verts.Count - 3);
                    tris.Add(verts.Count - 2);
                    tris.Add(verts.Count - 1);
                }

                lastNormUp = nextUp;
                lastNormLeft = nextLeft;
            }
            lastPos = lastPos + line[j].end;
            lastUp = line[j].up;
        }
        //generate start cap
        for(int i = 0; i < sourceVerts.Count; i++)
        {
            Vector3 v = sourceVerts[i];
            verts.Add(v.y * startNormUp + v.x * startNormleft);
            uvs.Add(new Vector2(v.x, v.y));
        }
        for(int i = 0; i < capTris.Count; i++)
        {
            tris.Add(capTris[i] + verts.Count - sourceVerts.Count);
        }
        //generate end cap
        for (int i = 0; i < sourceVerts.Count; i++)
        {
            Vector3 v = sourceVerts[i];
            verts.Add(lastPos + v.y * lastNormUp + v.x * lastNormLeft);
            uvs.Add(new Vector2(v.x, v.y));
        }
        for (int i = 0; i < capTris.Count; i++)
        {
            tris.Add(capTris[capTris.Count - 1 - i] + verts.Count - sourceVerts.Count);
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        return mesh;
    }

    private void OnDrawGizmos()
    {
        if (line == null) return;
        Vector3 lastPos = transform.position;
        startUp.Normalize();
        Vector3 lastUp = startUp;
        bool flipped = false;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(lastPos, .2f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(lastPos , lastPos + startUp * 2); //draw normal
        for (int j = 0; j < line.Count; j++)
        {
            //normalize user-entered values
            if(line[j].up == Vector3.zero)
            {
                line[j].up = Vector3.up;
            } else
            {
                line[j].up.Normalize();
            }

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                float t2 = (i + 1f) / segments;
                Vector3 p1 = getPoint(t, lastPos, line[j]);
                Vector3 p2 = getPoint(t2, lastPos, line[j]);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(p1, p2);

                Vector3 norm = getNormal(t, lastUp, line[j], p2 - p1);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(p1, p1 + norm);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(p1, p1 - norm);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(p1, p1 + Vector3.Cross(p2 - p1, norm).normalized);
            }
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(lastPos, .2f);
            Gizmos.DrawSphere(lastPos + line[j].end, .2f);
            //draw control points
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(lastPos + line[j].control1, .2f);
            Gizmos.DrawSphere(lastPos + line[j].end + line[j].control2, .2f);
            Gizmos.DrawLine(lastPos, lastPos + line[j].control1);
            Gizmos.DrawLine(lastPos + line[j].end, lastPos + line[j].end + line[j].control2);
            //draw normal
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(lastPos + line[j].end, lastPos + line[j].end + line[j].up * 2);
            lastPos = lastPos + line[j].end;
            lastUp = line[j].up;
        }
    }

    private Vector3 getNormal(float t, Vector3 start, BezierSegment segment, Vector3 line)
    {
        Vector3 lerpedUp = Vector3.Lerp(start, segment.up, t);
        return Vector3.ProjectOnPlane(lerpedUp.normalized, line.normalized).normalized;
    }

    private Vector3 getPoint(float t, Vector3 start, BezierSegment segment)
    {
        Vector3 p0 = start;
        Vector3 p1 = start + segment.control1;
        Vector3 p2 = start + segment.end + segment.control2;
        Vector3 p3 = start + segment.end;
        return Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1 + 3 * (1 - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3; 
    }
}

[System.Serializable]
public class BezierSegment
{
    public Vector3 control1;
    public Vector3 control2;
    public Vector3 end;
    public Vector3 up;
}