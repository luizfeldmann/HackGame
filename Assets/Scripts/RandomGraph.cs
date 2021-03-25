//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGraph// : MonoBehaviour
{
    int numPoints;
    float maxDistance;
    int maxdint;
    float minDistanceSquared;

    public class Vertex
    {
        public int x;
        public int y;
        public object tag;

        public List<Edge> edges = new List<Edge>();

        public float dist2to(int dx, int dy)
        {
            return Mathf.Pow(x -dx, 2) + Mathf.Pow(y-dy, 2);
        }

        public float dist2to(Vertex d)
        {
            return dist2to(d.x, d.y);
        }

        public Vector2 GetPosition()
        {
            return new Vector2(x, y);
        }
    }

    public class Edge
    {
        public Vertex A;
        public Vertex B;

        public Vertex GetOther(Vertex one)
        {
            if (one == A)
                return B;
            else if (one == B)
                return A;
            else
                return null;
        }

        public float dist2()
        {
            return A.dist2to(B);
        }
    }

    private List<Vertex> vertices = new List<Vertex>();
    private List<Edge> edges = new List<Edge>();

    public RandomGraph(int ammount, float dmax, float dmin)
    {
        numPoints = ammount;
        maxDistance = dmax;
        maxdint = Mathf.CeilToInt(dmax);
        minDistanceSquared = Mathf.Pow(dmin, 2);
    }

    public Vertex[] PopulateVertices()
    {
        vertices.Add(new Vertex(){ x = 0, y = 0 }); // add first

        Vertex parent;
        while (vertices.Count < numPoints)
                vertices.Add(CreateNew(out parent));

        return vertices.ToArray();
    }

    public Vertex CreateNew(out Vertex par)
    {
        Vertex v = null;
        par = null;

        while (v==null)
        {
            var parent = vertices[Random.Range(0, vertices.Count-1)];
            int _x = Random.Range(-maxdint, maxdint) + parent.x;
            int _y = Random.Range(-maxdint, maxdint) + parent.y;

            if (vertices.TrueForAll(test => test.dist2to(_x, _y) >= minDistanceSquared))
            {
                v = new Vertex(){ x = _x, y = _y };
                par = parent;
                break;
            }
        }

        return v;
    }
     
    private void CreateEdge(Vertex a, Vertex b)
    {
        var e = new Edge(){ A = a, B = b };
        a.edges.Add(e);
        b.edges.Add(e);
        edges.Add(e);
    }

    private void RemoveEdge(Edge e)
    {
        edges.Remove(e);
        e.A.edges.Remove(e);
        e.B.edges.Remove(e);
    }

    public Edge[] PopulateEdges(float option)
    {
        foreach (var v in vertices)
        {
            foreach (var w in vertices)
            {
                if (v == w)
                    continue;
                
                if (edges.Find(e => (e.A == v && e.B == w) || (e.A == w && e.B == v)) == null )
                    CreateEdge(v, w);
            }
        }

        List<Edge> important = new List<Edge>();
        while (edges.Count > 0)
        {
            edges.Sort((e1, e2) => e2.dist2().CompareTo(e1.dist2())); // ordena - mais longa primeiro
            var candidate = edges[0];
            RemoveEdge(candidate);

            var d = new Dijkstras();
            d.Initialize(vertices.ToArray(), candidate.A, candidate.B, gdh, gah);
            d.Run();

            if (d.FindDistance() > 1000 || candidate.dist2() < option) //  remover a conexão causou a disconexão do grafo
                important.Add(candidate);
        }

        foreach (var i in important)
            CreateEdge(i.A, i.B); // restitui apenas os importantes
        
        return edges.ToArray();
    }

    private float gdh(object a, object b)
    {
        return (a as Vertex).dist2to(b as Vertex);
    }
    private object[] gah(object v)
    {
        List<object> l = new List<object>();
        foreach (var c in (v as Vertex).edges)
            l.Add(c.GetOther(v as Vertex));

                return l.ToArray();
    }
}
