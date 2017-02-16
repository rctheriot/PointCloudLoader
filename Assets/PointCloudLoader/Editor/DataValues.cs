public class DataValues{
    private float xMin;
    public float XMin {get;set;}
    private float xMax;
    public float XMax {get;set;}
    private float yMin;
    public float YMin{get;set;}
    private float yMax;
    public float YMax{get;set;}
    private float zMin;
    public float ZMin {get;set;}
    private float zMax;
    public float ZMax {get;set;}
    private int numPoints;
    public int NumPoints { get; set; }

    public DataValues(){
        xMin = float.MaxValue;
        xMax = float.MinValue;
        yMin = float.MaxValue;
        yMax = float.MinValue;
        zMin = float.MaxValue;
        zMax = float.MinValue;
        numPoints = 0;
    }
}