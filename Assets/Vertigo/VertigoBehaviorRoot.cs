using Effect;
using UnityEngine;
using UnityEngine.Rendering;
using Vertigo;

public class VertigoBehaviorRoot : MonoBehaviour {

    private VertigoContext ctx;
    public new Camera camera;

    public Texture2D texture;

    public ShadowOutline shadowOutline;

    public ShadowData shadowData;

    private bool didStart = false;

    public Vector2 shadowOffset = new Vector2(20, 20);

    public VertigoCtx ctx2;

    public void Start() {
        didStart = true;
        ctx = new VertigoContext();
        ctx2 = new VertigoCtx();
        //  Camera.onPostRender += Render;
        //camera.AddCommandBuffer(CameraEvent.AfterEverything, ctx.commandBuffer);
        shadowOutline = new ShadowOutline(new Material(Shader.Find("Vertigo/Default")));
    }

    private void OnDestroy() {
        Camera.onPostRender = null;
    }

    public void Update() {
//    public void Render(Camera camera) {
        if (!didStart) return;

        camera.orthographicSize = Screen.height * 0.5f;

        PathGenerator path = new PathGenerator();
        path.MoveTo(100, 100);
        path.LineTo(200, 200);
        
        GeometryGenerator geo = new GeometryGenerator();

        geo.SetLineCap(LineCap.Round);
        geo.SetLineJoin(LineJoin.Bevel);
        
        geo.StrokeSDF(path);
        
        ctx2.SetStrokeWidth(4);
        ctx2.SetStrokeColor(Color.yellow);
        
        VertigoMaterial material = ctx2.MaterialPool.GetShared("VertigoSDF");
        
        ctx2.Draw(geo, material);
        
//        ctx.BeginPath();
//        ctx.Rect(0, 0, 112, 188);
//        ctx.ClosePath();
//        ctx.SetFill(Color.red);
//        ctx.SetTexture(texture, 0);
//        shadowOutline.data = shadowData;
//        ctx.Fill(shadowOutline);
//        ctx.Render(camera);
//        ctx.Clear();
    }

}