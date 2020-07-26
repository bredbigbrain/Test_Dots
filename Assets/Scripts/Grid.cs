using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grid
{
    [SerializeField]
    protected float offset = 1f;
    [SerializeField]
    protected int size = 6;
    
    public Vector3[,] Positions { get; protected set; }
    public float Offset { get => offset; }
    public int Size { get => size; }

    public void Init(Vector3 center)
    {
        if(Positions == null)
            Positions = new Vector3[size, size];

        float sideSize = (size - 1) * offset;
        var leftBottom = center - new Vector3(sideSize / 2, sideSize / 2, 0);

        for (int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
                Positions[x, y] = leftBottom + new Vector3(x * offset, y * offset, 0);
        }
    }

    public void Update(int size, float offset, Vector3 center)
    {
        if (Positions != null && this.size == size && this.offset == offset)
            return;

        if (this.size != size)
            Positions = null;

        this.size = size;
        this.offset = offset;

        Init(center);
    }
}
