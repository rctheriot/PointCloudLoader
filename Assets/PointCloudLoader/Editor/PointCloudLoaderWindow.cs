using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

public class PointCloudLoaderWindow : EditorWindow {
    //Number of elements per data line from input file
    private static int elementsPerLine = 0;

    //Position of XYZ and RGB elements in data line
    private static int rPOS, gPOS, bPOS, xPOS, yPOS, zPOS;

    //Enumerator for PointCloud color range
    //None = No Color, Normalized = 0-1.0f, RGB = 0-255
    private enum ColorRange {
        NONE = 0,
        NORMALIZED = 1,
        RGB = 255
    }
    private static ColorRange colorRange;

    //Data line delimiter  
    public static string dataDelimiter;

    //Maximum vertices a mesh can have in Unity
    static int limitPoints = 65000;

    [MenuItem("Window/PointClouds/LoadCloud")]
    private static void ShowEditor() {
        EditorWindow window = GetWindow(typeof(PointCloudLoaderWindow), true, "Point Cload Loader");
        window.maxSize = new Vector2(385f, 355f);
        window.minSize = window.maxSize;
    }

    //GUI Window Stuff - NO COMMENTS
    private void OnGUI() {

        GUIStyle help = new GUIStyle(GUI.skin.label);
        help.fontSize = 12;
        help.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("How To Use", help);

        EditorGUILayout.HelpBox("1. Set the number of elements that exist on each data line. \n" +
                                "2. Set the delimiter between each element. (Leave blank for white space) \n" +
                                "3. Set the index of the XYZ elements on the data line. (First element = 1) \n" +
                                "4. Select the range of the color data: \n" +
                                "       None: No Color Data \n" +
                                "       Normalized: 0.0 - 1.0 \n" +
                                "       RGB : 0 - 255 \n" +
                                "5. Set the index of the RGB elements on the data line. (First element = 1) \n" +
                                "6. Click \"Load Point Cloud File\"", MessageType.None);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        elementsPerLine = EditorGUILayout.IntField(new GUIContent("Elements Per Data Line", "The Number of Elements in the data line"), elementsPerLine);
        dataDelimiter = EditorGUILayout.TextField(new GUIContent("Data Line Delimiter", "Leave blank for white space between elements"), dataDelimiter);
        xPOS = EditorGUILayout.IntField(new GUIContent("X Index", "Index of X in data line"), xPOS);
        yPOS = EditorGUILayout.IntField(new GUIContent("Y Index", "Index of Y in data line"), yPOS);
        zPOS = EditorGUILayout.IntField(new GUIContent("Z Index", "Index of Z in data line"), zPOS);

        colorRange = (ColorRange)EditorGUILayout.EnumPopup(new GUIContent("Color Range", "None(No Color), Normalized (0.0-1.0f), RGB(0-255)"), colorRange);

        if (colorRange == ColorRange.NORMALIZED || colorRange == ColorRange.RGB) {
            rPOS = EditorGUILayout.IntField(new GUIContent("Red Index", "Index of Red color in data line"), rPOS);
            gPOS = EditorGUILayout.IntField(new GUIContent("Green Index", "Index of Green color in data line"), gPOS);
            bPOS = EditorGUILayout.IntField(new GUIContent("Blue Index", "Index of Blue color in data line"), bPOS);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 12;
        buttonStyle.fontStyle = FontStyle.Bold;
        if (GUILayout.Button("Load Point Cloud File", buttonStyle, GUILayout.Height(50))) {
            LoadCloud();
        }

    }


    private void LoadCloud() {
        //Get path to file with EditorUtility
        string path = EditorUtility.OpenFilePanel("Load Point Cloud File", "", "*");

        //If path doesn't exist of user exits dialog exit function
        if (path.Length == 0) return;

        //Set data delimiter
        char delimiter = ' ';
        try {
            if (dataDelimiter.Length != 0) delimiter = dataDelimiter.ToCharArray()[0];

        }
        catch (NullReferenceException) {
        }

        //Create string to name future asset creation from file's name
        string filename = null;
        try { filename = Path.GetFileName(path).Split('.')[0]; }
        catch (ArgumentOutOfRangeException e) { Debug.LogError("PointCloudLoader: File must have an extension. (.pts, .xyz....etc)" + e); }

        //Create PointCloud Directories
        if (!Directory.Exists(Application.dataPath + "/PointClouds/"))
            AssetDatabase.CreateFolder("Assets", "PointClouds");

        if (!Directory.Exists(Application.dataPath + "/PointClouds/" + filename))
            UnityEditor.AssetDatabase.CreateFolder("Assets/PointClouds", filename);

        //Setup Progress Bar 
        float progress = 0.0f;
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayProgressBar("Progress", "Percent Complete: " + (int)(progress * 100) + "%", progress);

        //Setup variables so we can use them to center the PointCloud at origin
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMin = float.MaxValue;
        float yMax = float.MinValue;
        float zMin = float.MaxValue;
        float zMax = float.MinValue;

        //Streamreader to read data file
        StreamReader sr = new StreamReader(path);
        string line;

        //Could use a while loop but then cant show progress bar progression
        String[] allLines = File.ReadAllLines(path);
        int numberOfLines = allLines.Length;
        int numPoints = 0;


        List<Thread> myThreads = new List<Thread>();
        List<DataValues> myData = new List<DataValues>();

        for (int i = 0; i < 4; i++) {
            myData.Add(new DataValues());
            myThreads.Add(new Thread(() => DataValuesGetMinMax(i, 4, allLines, myData[i])));
            myThreads[i].Start();
        }

        for (int i = 0; i < 4; i++) {
            myThreads[i].Join();
        }

        for (int i = 0; i < 4; i++) {
            if (xMin > myData[i].XMin) {
                xMin = myData[i].XMin;
            }
            if (xMax < myData[i].XMax) {
                xMax = myData[i].XMax;
            }
            if (yMin > myData[i].YMin) {
                yMin = myData[i].YMin;
            }
            if (yMax < myData[i].YMax) {
                yMax = myData[i].YMax;
            }
            if (zMin > myData[i].ZMin) {
                zMin = myData[i].ZMin;
            }
            if (zMax < myData[i].ZMax) {
                zMax = myData[i].ZMax;
            }
            numPoints += myData[i].NumPoints;

        }

        //Update progress bar -Only updates every 10,000 lines - DisplayProgressBar is not efficient and slows progress
        //progress = i * 1.0f / (numberOfLines - 1) * 1.0f;
        //if (i % 10000 == 0)
        //    EditorUtility.DisplayProgressBar("Progress", "Percent Complete: " + (int)((progress * 100) / 3) + "%", progress / 3);



        //Calculate origin of point cloud to shift cloud to unity origin
        float xAvg = (xMin + xMax) / 2;
        float yAvg = (yMin + yMax) / 2;
        float zAvg = (zMin + zMax) / 2;

        //Setup array for the points and their colors
        Vector3[] points = new Vector3[numPoints];
        Color[] colors = new Color[numPoints];

        //Reset Streamreader
        sr = new StreamReader(path);

        //For loop to create all the new vectors from the data points
        for (int i = 0; i < numPoints; i++) {
            line = sr.ReadLine();
            string[] words = line.Split(delimiter);

            //Only read data lines
            while (words.Length != elementsPerLine) {
                line = sr.ReadLine();
                words = line.Split(' ');
            }

            //Read data line for XYZ and RGB
            float x = float.Parse(words[xPOS - 1]) - xAvg;
            float y = float.Parse(words[yPOS - 1]) - yAvg;
            float z = (float.Parse(words[zPOS - 1]) - zAvg) * -1; //Flips to Unity's Left Handed Coorindate System
            float r = 1.0f;
            float g = 1.0f;
            float b = 1.0f;

            //If color range has been set also get color from data line
            if (colorRange == ColorRange.NORMALIZED || colorRange == ColorRange.RGB) {
                r = float.Parse(words[rPOS - 1]) / (int)colorRange;
                g = float.Parse(words[gPOS - 1]) / (int)colorRange;
                b = float.Parse(words[bPOS - 1]) / (int)colorRange;
            }

            //Save new vector to point array
            //Save new color to color array
            points[i] = new Vector3(x, y, z);
            colors[i] = new Color(r, g, b, 1.0f);

            //Update Progress Bar
            progress = i * 1.0f / (numPoints - 1) * 1.0f;
            if (i % 10000 == 0)
                EditorUtility.DisplayProgressBar("Progress", "Percent Complete: " + (int)(((progress * 100) / 3) + 33) + "%", progress / 3 + .33f);


        }

        //Close Stream reader
        sr.Close();


        // Instantiate Point Groups
        //Unity limits the number of points per mesh to 65,000.  
        //For large point clouds the complete mesh wil be broken down into smaller meshes
        int numMeshes = Mathf.CeilToInt(numPoints * 1.0f / limitPoints * 1.0f);

        //Create the new gameobject
        GameObject cloudGameObject = new GameObject(filename);

        //Create an new material using the point cloud shader
        Material newMat = new Material(Shader.Find("PointCloudShader"));
        //Save new Material
        AssetDatabase.CreateAsset(newMat, "Assets/PointClouds/" + filename + "Material" + ".mat");

        //Create the sub meshes of the point cloud
        for (int i = 0; i < numMeshes - 1; i++) {
            CreateMeshGroup(i, limitPoints, filename, cloudGameObject, points, colors, newMat);

            progress = i * 1.0f / (numMeshes - 2) * 1.0f;
            if (i % 2 == 0)
                EditorUtility.DisplayProgressBar("Progress", "Percent Complete: " + (int)(((progress * 100) / 3) + 66) + "%", progress / 3 + .66f);

        }
        //Create one last mesh from the remaining points
        int remainPoints = (numMeshes - 1) * limitPoints;
        CreateMeshGroup(numMeshes - 1, numPoints - remainPoints, filename, cloudGameObject, points, colors, newMat);

        progress = 100.0f;
        EditorUtility.DisplayProgressBar("Progress", "Percent Complete: " + progress + "%", 1.0f);

        //Store PointCloud
        UnityEditor.PrefabUtility.CreatePrefab("Assets/PointClouds/" + filename + ".prefab", cloudGameObject);
        EditorUtility.DisplayDialog("Point Cloud Loader", filename + " Saved to PointClouds folder", "Continue", "");
        EditorUtility.ClearProgressBar();

        return;
    }

    private void CreateMeshGroup(int meshIndex, int numPoints, string filename, GameObject pointCloud, Vector3[] points, Color[] colors, Material mat) {

        //Create GameObject and set parent
        GameObject pointGroup = new GameObject(filename + meshIndex);
        pointGroup.transform.parent = pointCloud.transform;

        //Add mesh to gameobject
        Mesh mesh = new Mesh();
        pointGroup.AddComponent<MeshFilter>();
        pointGroup.GetComponent<MeshFilter>().mesh = mesh;

        //Add Mesh Renderer and material
        pointGroup.AddComponent<MeshRenderer>();
        pointGroup.GetComponent<Renderer>().material = mat;

        //Create points and color arrays
        int[] indecies = new int[numPoints];
        Vector3[] meshPoints = new Vector3[numPoints];
        Color[] meshColors = new Color[numPoints];

        for (int i = 0; i < numPoints; ++i) {
            indecies[i] = i;
            meshPoints[i] = points[meshIndex * limitPoints + i];
            meshColors[i] = colors[meshIndex * limitPoints + i];
        }

        //Set all points and colors on mesh
        mesh.vertices = meshPoints;
        mesh.colors = meshColors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);

        //Create bogus uv and normals
        mesh.uv = new Vector2[numPoints];
        mesh.normals = new Vector3[numPoints];

        // Store Mesh
        UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/PointClouds/" + filename + @"/" + filename + meshIndex + ".asset");
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();

        return;
    }

    public void DataValuesGetMinMax(int threadID, int totalThreads, String[] allLines, DataValues data) {
        int partialLines = allLines.Length / totalThreads;
        for (int i = threadID * partialLines; i < (threadID + 1) * partialLines; i++) {

            string[] words = allLines[i].Split(' ');

            //Only read data lines
            if (words.Length == elementsPerLine) {
                data.NumPoints++;

                if (data.XMin > float.Parse(words[xPOS - 1]))
                    data.XMin = float.Parse(words[xPOS - 1]);
                if (data.XMax < float.Parse(words[xPOS - 1]))
                    data.XMax = float.Parse(words[xPOS - 1]);

                if (data.YMin > float.Parse(words[yPOS - 1]))
                    data.YMin = float.Parse(words[yPOS - 1]);
                if (data.YMax < float.Parse(words[yPOS - 1]))
                    data.YMax = float.Parse(words[yPOS - 1]);

                if (data.ZMin > float.Parse(words[zPOS - 1]))
                    data.ZMin = float.Parse(words[zPOS - 1]);
                if (data.ZMax < float.Parse(words[zPOS - 1]))
                    data.ZMax = float.Parse(words[zPOS - 1]);

            }
        }
    }
}


