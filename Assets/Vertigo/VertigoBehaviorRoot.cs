using System;
using UnityEngine;
using UnityEngine.U2D;
using Vertigo;

public class VertigoBehaviorRoot : MonoBehaviour {

    public new Camera camera;

    public float radius = 50f;
    public float width = 50f;
    public float height = 100f;
    public VertigoContext ctx;
    ShapeGenerator shapeGen = new ShapeGenerator();
    GeometryGenerator geo = new GeometryGenerator();
    private GeometryCache cache;
    public int segmentCount = 60;

    public int miterLimit = 10;
    public float strokeWidth = 60f;
    public LineCap lineCap = LineCap.Butt;
    public LineJoin lineJoin = LineJoin.Bevel;
    public Color strokeColor = Color.white;
    public Texture2D bgImage;
    public SpriteAtlas atlas;
    
    public Vector2[] vector2s = {
        new Vector2(0, 0),
        new Vector2(100, 0),
        new Vector2(100, 100),
        new Vector2(0, 100)
    };

    private Sprite oldguy;
    
    public void Start() {
        ctx = new VertigoContext();
        oldguy = atlas.GetSprite("Light_Frame");
    }

    public bool closed;

    public void Update() {
        camera.orthographicSize = Screen.height * 0.5f;
        
        ctx.Clear();
        shapeGen.Clear();
        
        VertigoMaterial background = ctx.materialPool.GetShared("Material/DisolveHDR");
        VertigoMaterial defaultMaterial = ctx.materialPool.GetShared("VertigoSDF");
        
        background.material.SetTexture("_MainTex", oldguy.texture);
        defaultMaterial.material.SetTexture("_MainTex", oldguy.texture);
        ctx.BeginShapeRange();


//        ctx.SetTexture(texture);
        
//        SetMaterial();
//        ctx.SetMaterialInt();
//            ctx.Rect(0, 0, width, height);
//        ctx.SetUVTiling();
//        ctx.SetUVOffset();
        
//        ctx.SetUVRect(VertigoUtil.RectFromSpriteUV(oldguy));
//        ctx.SetFillColor(Color.white);
//        ctx.Fill(background);
//        ctx.ResetUVState();
//        
//        ctx.BeginShapeRange();
//        
//        ctx.Circle(200, 200, 40);
//        
//        ctx.Fill(defaultMaterial);
        
        ctx.DrawSprite(oldguy, new Rect(200, 200, 200, 200), defaultMaterial);
        
      
        ctx.Flush(camera);

        
        
    }

}