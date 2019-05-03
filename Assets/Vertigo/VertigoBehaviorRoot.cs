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

    public int miterLimit = 10;
    public float strokeWidth = 60f;
    public LineCap lineCap = LineCap.Butt;
    public LineJoin lineJoin = LineJoin.Bevel;
    public Color strokeColor = Color.white;
    
    public Vector2[] vector2s = {
        new Vector2(0, 0),
        new Vector2(100, 0),
        new Vector2(100, 100),
        new Vector2(0, 100)
    };

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

    public bool closed;

    public void Update() {
        camera.orthographicSize = Screen.height * 0.5f;

        ctx2.Clear();
        shapeGen.Clear();

        shapeGen.BeginPath(vector2s[0].x, vector2s[0].y);
        for (int i = 1; i < vector2s.Length; i++) {
            shapeGen.LineTo(vector2s[i].x, vector2s[i].y);
        }

//        shapeGen.LineTo(400, 500);
//        shapeGen.LineTo(200, 100);
//        shapeGen.CubicBezierTo(100, 100, 400, 100, 400, 250);
//        
//        shapeGen.BeginHole(200, 200);
//
//        shapeGen.RectTo(220, 220);
//        shapeGen.CloseHole();
//        
//        shapeGen.BeginHole(250, 200);
//        
//        shapeGen.RectTo(width, height);
        if (closed) {
            shapeGen.ClosePath();
        }
        else {
            shapeGen.EndPath();
        }

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
        cache.Clear();

        geo.SetLineCap(lineCap);
        geo.SetLineJoin(lineJoin);
        geo.SetStrokeWidth(strokeWidth);
        geo.SetStrokeColor(strokeColor);
        geo.SetMiterLimit(miterLimit);
        geo.Stroke(shapeGen, cache);
        // fill rect | circle | whatever

        VertigoMaterial sharedMat = ctx2.materialPool.GetShared("VertigoSDF");

//        cache.SetVertexColors(0, Color.red);
        ctx2.SetPosition(new Vector3(0, 0));
        ctx2.Draw(cache, sharedMat);


//        cache.SetVertexColors(0, Color.green);
//        ctx2.Draw(cache, sharedMat);

        ctx2.Flush(camera);
    }

}