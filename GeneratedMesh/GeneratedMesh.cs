using Godot;
using System;

public class GeneratedMesh : MeshInstance{

    ArrayMesh tmpMesh = new ArrayMesh();
    [Export]Material mat; 
    Random random = new Random();

    [Export] public  int terrainSize = 512;
    [Export] public int seed = -1;

    [Export] public  float minColorHeight = -1f, maxColorHeight = 10f;
    [Export] Color[] heightsColors = {
        new  Color(0.1f, 0.1f, 1f),//on inspector change colors: ocean dark blue to montain white snow
        new  Color(0.1f, 0.9f, 0.1f),
        new  Color(05f, 0.4f, 0.1f),
        new  Color(0.8f, 0.8f, 0.8f),
        new  Color(0.8f, 0.8f, 0.8f),
        new  Color(0.8f, 0.8f, 0.8f),
        new  Color(0.8f, 0.8f, 0.8f),
        new  Color(0.8f, 0.8f, 0.8f),
        new  Color(0.8f, 0.8f, 0.8f),
        new  Color(0.8f, 0.8f, 0.8f),
    };

    public override void _Ready() {

        float[,] dataTerrain = generateData();
        generateMeshAndColors(dataTerrain);

    }

    private float[,] generateData(){
        //random noise data: n x n heights
        OpenSimplexNoise noise = new OpenSimplexNoise();
        if (seed == -1) seed = random.Next();
        noise.Seed = seed;

        noise.Octaves = 8;
        noise.Period = 22;
        noise.Lacunarity = 1.5f;
        noise.Persistence = 0.02f;

        float heigthMulti = 2f;

        //1 pass low noise
        float[,] dataTerrain = new float[terrainSize,terrainSize];
        for (int i = 0; i<terrainSize; i++){
            for (int j = 0; j<terrainSize; j++){
                dataTerrain[j,i] += noise.GetNoise2d(j,i) * heigthMulti;
            }
        }

        //2 pass medium heighs
        heigthMulti = 10f;
        noise.Octaves = 8;
        noise.Period = 100;
        noise.Lacunarity = 1.5f;
        noise.Persistence = 0.02f;
        
        for (int i = 0; i<terrainSize; i++){
            for (int j = 0; j<terrainSize; j++){
                dataTerrain[j,i] += noise.GetNoise2d(j,i) * heigthMulti;
            }
        }

        //3 pass Heavy heighs
        heigthMulti = 30f;
        noise.Octaves = 8;
        noise.Period = 200;
        noise.Lacunarity = 1.5f;
        noise.Persistence = 0.02f;

        //and get min and max heights
        minColorHeight = float.MaxValue;
        maxColorHeight = float.MinValue;

        for (int i = 0; i<terrainSize; i++){
            for (int j = 0; j<terrainSize; j++){
                dataTerrain[j,i] += noise.GetNoise2d(j,i) * heigthMulti;
                //minMax
                float h =  dataTerrain[j,i];
                if (h<minColorHeight) minColorHeight = h;
                if (h>maxColorHeight) maxColorHeight = h;
            }
        }

        
       
        return dataTerrain;
    }

    public void generateMeshAndColors(float[,] dataTerrain){
        //init tool 
        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        st.SetMaterial(mat);

        //get HeightsQuats and create triangles
        int maxI = dataTerrain.GetLength(0);
        int maxJ = dataTerrain.GetLength(1);
        int quadCount = 0;

        //1º rows, 2º columns
        for (int i = 0; i < maxI-1; i++){
            for (int j = 0; j < maxJ-1; j++){
                
                Quat q = new Quat();//heights to Quat
                
                q.x = dataTerrain[i,j];       q.y = dataTerrain[i,j+1];

                q.z = dataTerrain[i+1,j];     q.w = dataTerrain[i+1,j+1];

                //to mesh
                Vector3 offset = new Vector3(j,0,i);
                createQuad(st, offset, q);

                /*//debug algoritm
                GD.Print(string.Format("•quad{0}:({1},{2}): points: \n{3} {4} {5}   {6} {7} {8}\n{9} {10} {11}   {12} {13} {14}",
                quadCount,i,j, 
                offset.x, q.x, offset.z - 1,       offset.x + 1, q.y, offset.z-1,
                offset.x, q.z, offset.z,         offset.x + 1, q.w, offset.z));
                */
                quadCount++;
            }
        }

        //finally
        st.GenerateNormals();
        st.Commit(tmpMesh);
        this.SetMesh(tmpMesh);

    }

    private SurfaceTool createQuad(SurfaceTool st, Vector3 pos, Quat q){
        
        //1Quad = 4 points = 2 triangles
        Vector3 v1 = new Vector3(0, q.x,-1) + pos;
        Vector3 v2 = new Vector3(1, q.y,-1) + pos;
        Vector3 v3 = new Vector3(1, q.w, 0) + pos;
        Vector3 v4 = new Vector3(0, q.z,0) + pos;

        //tri1
        st.AddUv(new Vector2(0,0));
        st.AddColor(heightToColor(v1.y));// active albedoVertexColors on material
        st.AddVertex(v1);
        

        st.AddUv(new Vector2(0,1));
        st.AddColor(heightToColor(v2.y));
        st.AddVertex(v2);


        st.AddUv(new Vector2(1,1));
        st.AddColor(heightToColor(v4.y));
        st.AddVertex(v4);

        //tri2
        st.AddUv(new Vector2(0,0));
        st.AddColor(heightToColor(v2.y));
        st.AddVertex(v2);

        st.AddUv(new Vector2(0,1));
        st.AddColor(heightToColor(v3.y));
        st.AddVertex(v3);

        st.AddUv(new Vector2(1,1));
        st.AddColor(heightToColor(v4.y));
        st.AddVertex(v4);

        return st;
    }

    private Color heightToColor(float height){

        //get color
        int colorsCount = heightsColors.Length;
        float value = Mathf.InverseLerp(minColorHeight,maxColorHeight,height);
        int indexColor =  Mathf.Clamp( Mathf.RoundToInt( Mathf.Lerp(0,colorsCount-1,value)), 0, colorsCount-1);
        
        return heightsColors[indexColor];
    }

}
