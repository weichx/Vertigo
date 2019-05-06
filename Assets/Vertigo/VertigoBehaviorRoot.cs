using System;
using System.IO;
using TreeEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Vertigo;

public class VertigoBehaviorRoot : MonoBehaviour {

    public new Camera camera;
    public VertigoContext ctx;
    ShapeGenerator shapeGen = new ShapeGenerator();
    GeometryGenerator geo = new GeometryGenerator();
    private GeometryCache cache;

    public Texture2D bgImage;
    public Texture2D lightFrame;
    public float softness;
    public SpriteAtlas atlas;

    private CommandBuffer commandBuffer;

    public void Start() {
        ctx = new VertigoContext();
        commandBuffer = new CommandBuffer();
        camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    }

    public void Update() {
        camera.orthographicSize = Screen.height * 0.5f;

        ctx.Clear();
        shapeGen.Clear();

        VertigoMaterial defaultMaterial = ctx.materialPool.GetInstance("VertigoSDF");
        VertigoMaterial bgMat = ctx.materialPool.GetInstance("VertigoSDF");

        ctx.Rect(0, 0, 32, 32);

        // PushRenderTexture();
        // ctx.SaveState();
        // ctx.SetColorMask(ColorMask.Alpha);
        // ctx.FillCircle();

        // ctx.RestoreState();
        // RenderTexture maskTexture = ctx.Render();
        // ctx.PopRenderTexture();
        // ctx.SetMask(maskTexture, 0.4f);
        // ctx.FillRect(...., material);
//        ctx.FillCircleSDF();
//        ctx.StrokeCircleSDF();
//        
//        ctx.SetDefaultGeometryMode(GeometryMode.SDF);
//        
//        ctx.Circle(0, 0, 50, GeometryMode.SDF);

//        ctx.BeginShapeRange();
//        shapeGen.Rect();
//        shapeGen.Rect();
//        shapeGen.Rect();
//        shapeGen.Rect();
//        shapeGen.Rect();
//        geo.Fill(shapeGen, shapeGen.currentRange)
//        geo.Stroke(shapeGen, shapeGen.currentRange)
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Circle(0, 100, 100);
//        ctx.BeginPath();
//        ctx.LineTo()
//        ctx.LineTo()
//        ctx.LineTo()
//        ctx.LineTo()
//        ctx.LineTo()
//        ctx.CurveTo();
//        ctx.EndPath();
//        
//        ctx.BeginStrokePath();
//        ctx.BeginFillPath();
//        ctx.FillPath(new [] {
//            Path.MoveTo(),
//            Path.LineTo()
//        });
//        int myRect = ctx.Rect(0, 0, 100, 100);
//        
//        ctx.Fill();
//        
//        ctx.SetDefaultMaterial(bg);
//        
//        int id = ctx.FillRect(0, 100, 500, 22, bgMat); // immediate
//        ctx.SetVertexColor(id);
//        
//        ctx.StrokeRect(0, 100, 500, 200, bgMat);
//        
//        ctx.DrawMesh();
//        ctx.DrawParticles();
//        ctx.DrawSprite();
//        ctx.DrawGeometry();
//        ctx.Draw(); // already created geometry
//        GeometryCache cache = null;
//        cache.SetVertexColors(3, Color.black);
//            ctx.BeginPath();
//        ctx.LineTo();
//        
        ctx.Fill(bgMat);
        
        ctx.SetMask(lightFrame, softness);
        ctx.Fill(bgMat);
        ctx.Render();
        ctx.Flush(camera, commandBuffer);
    }

}