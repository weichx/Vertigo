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
        long start = GC.GetTotalMemory(false);

        ctx2.Clear();
        shapeGen = new ShapeGenerator();
        shapeGen.Clear();
                
        shapeGen.BeginPath(100, 250);
        shapeGen.CubicBezierTo(100, 100, 400, 100, 400, 250);
        shapeGen.BeginHole(200, 200);

        shapeGen.RectTo(220, 220);
        shapeGen.CloseHole();
        shapeGen.BeginHole(250, 200);
        shapeGen.RectTo(width, height);
        shapeGen.ClosePath();
        
//        shapeGen.BeginPath(0, 0);
//        shapeGen.LineTo(200, 0);
//        
//        shapeGen.BeginHole(20, 20);
//        shapeGen.LineTo(40, 20);
//        shapeGen.LineTo(40, 40);
//        shapeGen.LineTo(20, 40);
//        shapeGen.CloseHole();
//
//        shapeGen.BeginHole(120, 120);
//        shapeGen.LineTo(140, 120);
//        shapeGen.LineTo(140, 140);
//        shapeGen.LineTo(120, 140);
//        shapeGen.CloseHole();
//        
//        shapeGen.LineTo(200, 200);
//        shapeGen.LineTo(0, 200);
//        
//        shapeGen.ClosePath();

//        shapeGen.Ellipse(0, 0, width, height, segmentCount);
//        shapeGen.Circle(300, 0, width, segmentCount);
//        shapeGen.RegularPolygon(-0, 0, width, height, segmentCount);

//        shapeGen.Rhombus(-300, 0, width, height);
//        shapeGen.Triangle(0, -50, 100, 100, -100, 100);
        cache = geo.Fill(shapeGen);

        // fill rect | circle | whatever

        VertigoMaterial sharedMat = ctx2.materialPool.GetShared("VertigoSDF");

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