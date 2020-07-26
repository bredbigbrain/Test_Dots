using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DotsConnector
{
    public LineRenderer line;
    [NonSerialized]
    public List<Dot> connectedDots = new List<Dot>();

    public void Init()
    {
        line.enabled = false;
    }

    public void AddDot(Dot dot)
    {
        if (dot == null)
            return;
        connectedDots.Add(dot);

        if (connectedDots.Count == 1)
        {
            line.endColor = line.startColor = dot.spriteRenderer.color;
            line.enabled = true;
            ++line.positionCount;
        }
        line.SetPosition(line.positionCount - 1, dot.transform.position + Vector3.forward);
        ++line.positionCount;
    }

    public void UpdateLine(Vector3 position)
    {
        if (line.positionCount == 0)
            return;
        position.z = line.transform.position.z;
        line.SetPosition(line.positionCount - 1, position);
    }

    public void Clear()
    {
        connectedDots.Clear();
        line.enabled = false;
        line.positionCount = 0;
    }
}
