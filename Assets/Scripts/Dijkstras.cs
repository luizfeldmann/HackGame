using System;
using System.Collections;
using System.Collections.Generic;

public class Dijkstras
{
    private Object[] vertices;
    private Dictionary<object, float> dist;
    private Dictionary<object, object> prev;

    private Object source;
    private Object target;
    private List<object> Queue;
    private GetDistanceHandler GetDistance;
    private GetAdjacentHandler GetAdjecent;

    public delegate float GetDistanceHandler(object ofrom, object oto);
    public delegate object[] GetAdjacentHandler(object ofrom);

    public void Initialize(object[] V, object s, object t, GetDistanceHandler gdh, GetAdjacentHandler gah)
    {
        vertices = V;
        source = s;
        target = t;
        GetDistance = gdh;
        GetAdjecent = gah;

        Queue = new List<object>();
        dist = new Dictionary<object, float>();
        prev = new Dictionary<object, object>();
        foreach (var vert in V)
        {
            Queue.Add(vert);
            dist[vert] = float.MaxValue;
            prev[vert] = null;
        }

        dist[s] = 0;
    }

    public void Run()
    {
        while (Queue.Count > 0)
        {
            Queue.Sort((V1,V2)=> dist[V1].CompareTo(dist[V2]));
            var u = Queue[0];
            //if (u == target)
            //    break;

            Queue.Remove(u);

            foreach (var v in GetAdjecent(u))
            {
                if (!Queue.Contains(v))
                    continue;

                var alt = dist[u] + GetDistance(u, v);
                if (alt < dist[v])
                {
                    prev[v] = u;
                    dist[v] = alt;
                }
            }
        }
    }

    public float FindDistance()
    {
        return dist[target];
    }

    public object[] Backtrack()
    {
        List<object> seq = new List<object>();
        object u = target;
        while (prev[u] != null)
        {
            seq.Insert(0, u);
            u = prev[u];
        }
        seq.Insert(0, u);

        return seq.ToArray();
    }
}

