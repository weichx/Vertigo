using System;
using UnityEngine;
using Vertigo;

public class VertigoBehaviorRoot : MonoBehaviour {

    private VertigoContextOld ctx;
    public new Camera camera;

    public float radius = 50f;
    public float width = 50f;
    public float height = 100f;
    public VertigoContext ctx2;
    ShapeGenerator shapeGen = new ShapeGenerator();
    GeometryGenerator geo = new GeometryGenerator();
    private GeometryCache cache;
    public int segmentCount = 60;
    
    public void Start() {
        ctx = new VertigoContextOld();
        ctx2 = new VertigoContext();
        
        shapeGen.Rect(100, 100, 200, 100);
        shapeGen.Circle(0, 300, 50);
        
        geo.SetLineCap(LineCap.Round);
        geo.SetLineJoin(LineJoin.Bevel);
        geo.SetStrokeColor(Color.yellow);
        geo.SetStrokeWidth(4f);
        cache = geo.Fill(shapeGen);
    }

    public void Update() {
        camera.orthographicSize = Screen.height * 0.5f;

        ctx2.Clear();
        shapeGen = new ShapeGenerator();
        
        shapeGen.BeginPath(0, 0);
        shapeGen.LineTo(100, 0);
        shapeGen.LineTo(100, 100);
        shapeGen.LineTo(0, 100);
        shapeGen.LineTo(50, 50);
        shapeGen.ClosePath();
        
//        shapeGen.Ellipse(0, 0, width, height, segmentCount);
//        shapeGen.Circle(300, 0, width, segmentCount);
//        shapeGen.RegularPolygon(-0, 0, width, height, segmentCount);

//        shapeGen.Rhombus(-300, 0, width, height);
//        shapeGen.Triangle(0, -50, 100, 100, -100, 100);
        cache = geo.Fill(shapeGen);
        
        
        VertigoMaterial sharedMat = ctx2.materialPool.GetShared("VertigoSDF");

        long start = GC.GetTotalMemory(false);
        cache.SetVertexColors(0, Color.red);
        ctx2.Draw(cache, sharedMat);

        ctx2.SetPosition(new Vector3(200, 200));

//        cache.SetVertexColors(0, Color.green);
//        ctx2.Draw(cache, sharedMat);

        ctx2.Flush(camera);
        long end = GC.GetTotalMemory(false);
        if (end - start > 0) {
            Debug.Log((end - start) + " allocated bytes");
        }

    }

}