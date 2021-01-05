using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System; 
//using System.Math;

[System.Serializable]
public class NoiseLayer {
    public float noiseParam = 0.01f;
    public float height = 10f;

    public int numPasses = 8;
    public float roughness = 2f;
    public float persistence = 0.5f;
}

public class Terrian : MonoBehaviour {
    public float width = 1f;
    public int N = 150;

    public static float offsetX;
    public static float offsetZ;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    List<Vector3> verts;
    List<int> indices;
    List<Color> colors;
    public List<NoiseLayer> noiseLayers;

    public Gradient gradient;
    public float maxHeight = float.MinValue;
    public float minHeight = float.MaxValue;
    static float averageHeight = 90f;
    static float roughness = 2f;
    //1,2 for spring; 3,4 for summer; 5,6 for autumn; 7,8 for winter
    static int season = 1;

    static float lowRate = 0.99f;
    static float middleRate = 0.99f;
    static float topRate = 0.99f;
    static float grassRate = 0.99f;
    static int updateTimespan = 100;
    static float lastUpdateTime;
    static int shapeofterrain;
    public GameObject Snow;

    List<GameObject> treeList = new List<GameObject> ();
    List<GameObject> grassList = new List<GameObject> ();

    //setter for height
    public void adjustHeight (float h) {
        updateTimespan = 0;
        averageHeight = h;
        
    }

    //setter for roughness of the terrain
    public void adjustRoughness (float r) {
        updateTimespan = 0;
        roughness = r;
    }

    //setter for tree rate on the bottom
    public void setLowrate (float lr) {
        updateTimespan = 0;
        lowRate = 1f - lr;
    }

    //setter for tree rate on the middle
    public void setMidrate (float mr) {
        updateTimespan = 0;
        middleRate = 1f - mr;      
    }

    //setter for tree rate on the top
    public void setToprate (float tr) {
        updateTimespan = 0;
        topRate = 1f - tr;
    }

    //setter for tree rate
    public void setAverageTreerate (float r) {
        updateTimespan = 0;
        lowRate = 1f - r;
        middleRate = 1f - r;
        topRate = 1f - r;
    }

    //setter for grass rate
    public void setGrassrate (float r) {
        updateTimespan = 0;
        grassRate = 1f - r;
    }

    //setter for random terrain
    public void setOffset () {
        offsetX = Random.Range (-10000, 10000);
        offsetZ = Random.Range (-10000, 10000);
        updateTimespan = 0;
    }

    //setter for season
    public void setSeason (float seasonFlag) {
        season = (int) (Mathf.Floor (seasonFlag / 0.125f)) + 1;
        updateTimespan = 0;
    }

    //this function will be called first
    private void Awake () {
        noiseLayers[0].height = averageHeight;
        noiseLayers[1].roughness = roughness;
        shapeofterrain = 0;
        SnowJudge();
    }

    //initialize data and start to generate terrain 
    private void Start () {
        verts = new List<Vector3> ();
        indices = new List<int> ();
        colors = new List<Color> ();

        meshRenderer = GetComponent<MeshRenderer> ();
        meshFilter = GetComponent<MeshFilter> ();
        meshCollider = GetComponent<MeshCollider> ();

        Generate ();
        PlantTrees ();
        lastUpdateTime = Time.time;
        PlantGrass ();
    }

    //generate terrain
    public void Generate () {
        ClearMeshData ();
        AddMeshData ();

        Mesh mesh = new Mesh ();
        mesh.vertices = verts.ToArray ();
        mesh.triangles = indices.ToArray ();
        mesh.colors = colors.ToArray ();

        mesh.RecalculateNormals ();
        mesh.RecalculateBounds ();

        meshFilter.mesh = mesh;

        meshCollider.sharedMesh = mesh;
    }

    //planting grass
    public void PlantGrass () {
        foreach (GameObject grass in grassList) {
            Destroy (grass);
        }

        //there is no grass in winter
        if (season >= 7) {
            return;
        }

        var mainpanel = GameObject.FindWithTag ("MainPanel");

        float heightRange = maxHeight - minHeight;
        float heightLow = minHeight;
        float heightMedium = minHeight + (heightRange / 3) * 1;
        float heightHigh = minHeight + (heightRange / 3) * 2;

        GameObject greengrass = GameObject.FindWithTag ("greengrass");
        GameObject yellowgrass = GameObject.FindWithTag ("yellowgrass");

        for (int i = 0; i < N * N; i++) {
            Vector3 p = verts[i];
            float plantRatio;

            int rotation = Random.Range (0, 360); //plants random rotation angle

            Vector3 grasses = p; //trees location

            if (p.x > verts[0].x && p.z > verts[0].z && p.x < verts[N * N - 1].x && p.z < verts[N * N - 1].z && p.y > 53) {
                plantRatio = (float) (Random.Range (0, 100) * 0.01f);

                if (plantRatio > grassRate) {
                    GameObject grass;
                    if (season <= 4) {
                        grass = Instantiate (greengrass) as GameObject;
                    } else {
                        grass = Instantiate (yellowgrass) as GameObject;
                    }

                    grass.transform.position = grasses;
                    grass.transform.eulerAngles = new Vector3 (0, rotation, 0);

                    grassList.Add (grass);
                    grass.transform.SetParent (mainpanel.transform, false);
                }

            }
        }
    }

    //trees planting
    public void PlantTrees () {

        foreach (GameObject tree in treeList) {
            Destroy (tree);
        }

        var mainpanel = GameObject.FindWithTag ("MainPanel");

        float heightRange = maxHeight - minHeight;
        float heightLow = minHeight;
        float heightMedium = minHeight + (heightRange / 3) * 1;
        float heightHigh = minHeight + (heightRange / 3) * 2;

        // add trees here
        GameObject tree1 = GameObject.FindWithTag ("treeSpring1");
        GameObject tree2 = GameObject.FindWithTag ("treeSpring2");

        GameObject tree3 = GameObject.FindWithTag ("treeSpring2");
        GameObject tree4 = GameObject.FindWithTag ("treeSpring3");

        GameObject tree5 = GameObject.FindWithTag ("treeSummer1");
        GameObject tree6 = GameObject.FindWithTag ("treeSummer2");

        GameObject tree7 = GameObject.FindWithTag ("treeSummer3");
        GameObject tree8 = GameObject.FindWithTag ("treeSummer2");

        GameObject tree9 = GameObject.FindWithTag ("treeAutumn1");
        GameObject tree10 = GameObject.FindWithTag ("treeAutumn2");

        GameObject tree11 = GameObject.FindWithTag ("treeAutumn3");
        GameObject tree12 = GameObject.FindWithTag ("treeAutumn4");

        GameObject tree13 = GameObject.FindWithTag ("treeWinter1");
        GameObject tree14 = GameObject.FindWithTag ("treeWinter1");

        GameObject tree15 = GameObject.FindWithTag ("treeWinter2");
        GameObject tree16 = GameObject.FindWithTag ("treeWinter2");

        List<GameObject> plantList = new List<GameObject> ();
        plantList.Add (tree1);
        plantList.Add (tree2);
        plantList.Add (tree3);
        plantList.Add (tree4);
        plantList.Add (tree5);
        plantList.Add (tree6);
        plantList.Add (tree7);
        plantList.Add (tree8);
        plantList.Add (tree9);
        plantList.Add (tree10);
        plantList.Add (tree11);
        plantList.Add (tree12);
        plantList.Add (tree13);
        plantList.Add (tree14);
        plantList.Add (tree15);
        plantList.Add (tree16);

        for (int i = 0; i < N * N; i++) {
            Vector3 p = verts[i];
            float plantRatio;

            int rotation = Random.Range (0, 360); //plants random rotation angle

            Vector3 trees = p; //trees location

            if (p.x > verts[0].x && p.z > verts[0].z && p.x < verts[N * N - 1].x && p.z < verts[N * N - 1].z) {
                for (int j = 0; j < 2; j++) {

                    /* plant trees */
                    plantRatio = (float) (Random.Range (0, 100) * 0.01f);

                    if (j == 0) {
                        p.x = p.x + (float) (Random.Range (-10, 10) * 0.1f);
                        p.z = p.z + (float) (Random.Range (-10, 10) * 0.1f);
                        p.y = p.y - 0.1f;
                    }

                    if (p.y > (heightLow + 0.1f * heightRange) && p.y <= heightMedium && plantRatio > lowRate) {
                        GameObject tree = Instantiate (plantList[season * 2 - j - 1]) as GameObject;
                        tree.transform.position = trees;
                        tree.transform.eulerAngles = new Vector3 (0, rotation, 0);
                        treeList.Add (tree);
                        tree.transform.SetParent (mainpanel.transform, false);
                    } else if (p.y > heightMedium && p.y <= heightHigh && plantRatio > middleRate) {
                        GameObject tree = Instantiate (plantList[season * 2 - j - 1]) as GameObject;
                        tree.transform.position = trees;
                        tree.transform.eulerAngles = new Vector3 (0, rotation, 0);
                        treeList.Add (tree);
                        tree.transform.SetParent (mainpanel.transform, false);
                    } else if (p.y > heightHigh && plantRatio > topRate) {
                        GameObject tree = Instantiate (plantList[season * 2 - j - 1]) as GameObject;
                        tree.transform.position = trees;
                        tree.transform.eulerAngles = new Vector3 (0, rotation, 0);
                        treeList.Add (tree);
                        tree.transform.SetParent (mainpanel.transform, false);
                    }
                }
            }
        }

    }

    //clear mesh data
    void ClearMeshData () {
        verts.Clear ();
        indices.Clear ();
        colors.Clear ();
    }

    //perlin noise implementation
    //using for terrain generation
    void AddMeshData () {
        for (int z = 0; z < N; z++) {
            for (int x = 0; x < N; x++) {
                float y = 0;

                foreach (NoiseLayer layer in noiseLayers) {
                    float frenquency = layer.noiseParam;
                    float amp = layer.height;
                    for (int i = 0; i < layer.numPasses; i++) {
                        y += Mathf.PerlinNoise ((x + offsetX) * frenquency, (z + offsetZ) * frenquency) * amp;
                        frenquency *= layer.roughness;
                        amp *= layer.persistence;
                    }
                }

                if (y < 50) {
                    y = 50f;
                }

                Vector3 p = new Vector3 (x, y, z) * width;
                if (y > maxHeight) { maxHeight = y; }
                if (y < minHeight) { minHeight = y; }
                verts.Add (p);
            }
        }

        //color the terrain
        for (int z = 0; z < N; z++) {
            for (int x = 0; x < N; x++) {
                int index = z * N + x;
                float y = verts[index].y;
                if (y < 50) {
                    y = 50f;
                    Color c = gradient.Evaluate (0.1f);
                    colors.Add (c);
                } else {
                    float p = (y - minHeight) / (maxHeight - minHeight);
                    Color c;
                    if (season > 7)
                        c = gradient.Evaluate (1);
                    else
                        c = gradient.Evaluate (p);
                    colors.Add (c);
                }
            }
        }

        //mesh vertice connection
        for (int z = 0; z < N - 1; z++) {
            for (int x = 0; x < N - 1; x++) {
                int index = z * N + x;
                int index1 = (z + 1) * N + x;
                int index2 = (z + 1) * N + x + 1;
                int index3 = z * N + x + 1;

                indices.Add (index);
                indices.Add (index1);
                indices.Add (index2);

                indices.Add (index);
                indices.Add (index2);
                indices.Add (index3);
            }
        }
    }

    //judge the time for snowing
    private void SnowJudge() {
        if (season <= 7){
            Snow.SetActive(false);
        }
        else{
            Snow.SetActive(true);
        }

    }

    private void Update () {
        if (Time.time >= lastUpdateTime + updateTimespan) {
            SnowJudge();

            foreach (GameObject grass in grassList) {
                Destroy (grass);
            }
            foreach (GameObject tree in treeList) {
                Destroy (tree);
            }

            Generate ();

            PlantTrees ();
            PlantGrass ();

            noiseLayers[0].height = averageHeight;
            noiseLayers[1].roughness = roughness;
            lastUpdateTime = Time.time;
            updateTimespan = 1000;
        }
    }

}