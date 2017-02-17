using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DataValues {
    private float xMin;
    public float XMin {
        get { return xMin; }
        set { xMin = value; }
    }
    private float xMax;
    public float XMax {
        get { return xMax; }
        set { xMax = value; }
    }
    private float yMin;
    public float YMin {
        get { return yMin; }
        set { yMin = value; }
    }
    private float yMax;
    public float YMax {
        get { return yMax; }
        set { yMax = value; }
    }
    private float zMin;
    public float ZMin {
        get { return zMin; }
        set { zMin = value; }
    }
    private float zMax;
    public float ZMax {
        get { return zMax; }
        set { zMax = value; }
    }
    private int numPoints;
    public int NumPoints {
        get { return numPoints; }
        set { numPoints = value; }
    }

    public DataValues() {
        xMin = float.MaxValue;
        xMax = float.MinValue;
        yMin = float.MaxValue;
        yMax = float.MinValue;
        zMin = float.MaxValue;
        zMax = float.MinValue;
        numPoints = 0;


    }
}