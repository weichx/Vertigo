using UnityEngine;
using UnityEngine.Rendering;

namespace Vertigo {

    public class VertigoContextOld {

        public readonly int instanceId;
        
        private RangeInt currentShapeRange;
        
        private Vector2 lastPoint;
        private VertigoEffect defaultEffect;
        private VertigoState state;
        
        private readonly ParameterTexture parameterTexture;
        private readonly ShapeBatch.ShapeBatchPool shapeBatchPool;
        
        private readonly StructList<Shape> shapes;
        private readonly StructList<Vector3> shapeData;
        private readonly StructList<IssuedDrawCall> issuedDrawCalls;
        private readonly StructList<PendingDrawCall> pendingDrawCalls;
        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        
        private static int s_InstanceIdGenerator;
        
        private static readonly int s_VertigoParameterTex = Shader.PropertyToID("_VertigoParameterTex");
        private static readonly int SVertigoParameterTexWidth = Shader.PropertyToID("s_VertigoParameterTexWidth");

        public VertigoContextOld() {

            instanceId = s_InstanceIdGenerator;
            s_InstanceIdGenerator += 100000;

            parameterTexture = new ParameterTexture(64, 64);
            
            shapeData = new StructList<Vector3>(128);
            shapes = new StructList<Shape>(64);
            pendingDrawCalls = new StructList<PendingDrawCall>(32);
            issuedDrawCalls = new StructList<IssuedDrawCall>();
            shapeBatchPool = new ShapeBatch.ShapeBatchPool();
            shapes.Add(new Shape(ShapeType.Unset));
        }

        public void Clear() {
            shapes.Clear();
            shapeData.Clear();
            pendingDrawCalls.Clear();
//            issuedDrawCalls.Clear();
            shapes.Add(new Shape(ShapeType.Unset));
            currentShapeRange = new RangeInt();
        }

        public void Stroke(Path2D path) {
            PendingDrawCall call = new PendingDrawCall();
            call.effect = VertigoEffect.Default;
            call.effectDataIndex = call.effect.StoreState(instanceId);
            call.isStroke = true;
            call.state = state;
            call.shapeRange = currentShapeRange;
            pendingDrawCalls.Add(call);
        }
        
        public void Rect(float x, float y, float w, float h) {
            RectBoundedShape(ShapeType.Rect, x, y, w, h);
        }

        private void RectBoundedShape(ShapeType shapeType, float x, float y, float width, float height) {
            ShapeType lastType = shapes[shapes.Count - 1].type;

            Vector2 x0y0 = new Vector2(x, y);
            Vector2 x1y1 = new Vector2(width, height);

            Shape currentShape = new Shape(shapeType, new RangeInt(shapeData.Count, 2), new Rect(x0y0, x1y1));

            shapeData.Add(x0y0);
            shapeData.Add(x1y1);

            if (lastType != ShapeType.Unset) {
                shapes.Add(currentShape);
            }
            else {
                shapes[shapes.Count - 1] = currentShape;
            }

            currentShapeRange.length++;
        }

        public void Circle(float cx, float cy, float r) {
            RectBoundedShape(ShapeType.Circle, cx - r, cy - r, r * 2, r * 2);
        }

        public void InsetCircle(float x, float y, float r) {
            RectBoundedShape(ShapeType.Circle, x - r, y - r, r * 2, r * 2);
        }

        public void InsetEllipse(float x, float y, float w, float h) {
            RectBoundedShape(ShapeType.Circle, x, y, w, h);
        }

        public void BeginHole() {
            // either make a new point type that holds a flag or write a 1 into the z coord of position to handle holes
            // also mark currentShape.hasHoles = true;
        }

        public void CloseHole() { }

        public void BeginPath() {
            ShapeType currentShapeType = shapes.Array[shapes.Count - 1].type;
            if (currentShapeType != ShapeType.Unset) {
                shapes.Add(new Shape(ShapeType.Unset));
                currentShapeRange = new RangeInt(shapes.Count - 1, 0);
            }
        }

        public void ClosePath() {
            if (currentShapeRange.length == 0) {
                return;
            }

            ShapeType lastType = shapes.Array[shapes.Count - 1].type;
//            if (lastType == ShapeType.Path) {
//                shapes.Array[shapes.Count - 1].type = ShapeType.ClosedPath;
//            }
        }

        public void Fill() {
            if (currentShapeRange.length == 0) {
                return;
            }

            PendingDrawCall call = new PendingDrawCall();
            call.effect = VertigoEffect.Default;
            call.effectDataIndex = call.effect.StoreState(instanceId);
            call.isStroke = true;
            call.state = state;
            call.shapeRange = currentShapeRange;
            pendingDrawCalls.Add(call);
        }

        public void Fill(VertigoEffect effect) {
            if (currentShapeRange.length == 0) {
                return;
            }

            PendingDrawCall call = new PendingDrawCall();
            call.state = state;
            call.isStroke = false;
            call.shapeRange = currentShapeRange;
            call.effect = effect ?? VertigoEffect.Default;
            call.effectDataIndex = effect.StoreState(instanceId);
            pendingDrawCalls.Add(call);
        }

        public void MoveTo(float x, float y) {
            lastPoint.x = x;
            lastPoint.y = y;
            if (shapes[shapes.Count - 1].type != ShapeType.Unset) {
                shapes.Add(new Shape(ShapeType.Unset));
            }

        }

        public void LineTo(float x, float y) {

            Shape currentShape = shapes[shapes.Count - 1];

            Vector2 point = new Vector2(x, y);

            switch (currentShape.type) {
                case ShapeType.Path:
                    currentShape.pointRange.length++;
                    shapes[shapes.Count - 1] = currentShape;
                    break;
                case ShapeType.Unset:
                    currentShape = new Shape(ShapeType.Path, new RangeInt(shapeData.Count, 2));
                    shapes[shapes.Count - 1] = currentShape;
                    currentShapeRange.length++;
                    shapeData.Add(lastPoint);
                    break;
                default:
                    currentShape = new Shape(ShapeType.Path, new RangeInt(shapeData.Count, 2));
                    shapes.Add(currentShape);
                    shapeData.Add(lastPoint);
                    currentShapeRange.length++;
                    break;
            }

            lastPoint = point;
            shapeData.Add(point);
        }

        public void CubicCurveTo(Vector2 ctrl0, Vector2 ctrl1, Vector2 end) {
//            int pointStart = points.Count;
//            int pointCount = SVGXBezier.CubicCurve(points, lastPoint, ctrl0, ctrl1, end);
//            UpdateShape(pointStart, pointCount);
//            lastPoint = end;
        }

        public void SetStrokeColor(Color color) { }

        public void SetStrokeColor(Color32 color) { }

        public void SetStrokeColor(uint color) { }

        public void Stroke() { }

        public void SetDefaultEffect(VertigoEffect effect) {
            this.defaultEffect = effect ?? VertigoEffect.Default;
        }

        public T GetDefaultEffect<T>() where T : VertigoEffect {
            return defaultEffect as T;
        }

        public void SetFill(Color color) { }

        private struct IssuedDrawCall {

            public VertigoEffect effect;
            public Material material;
            public int drawCallId;
            public Mesh mesh;
            public ShapeBatch batch;
            public VertigoState state;

        }

        
        public void Render(Camera camera, RenderTexture targetTexture = null, bool clear = true) {

            for (int i = 0; i < issuedDrawCalls.Count; i++) {
                issuedDrawCalls[i].effect.ReleaseMaterial(issuedDrawCalls[i].drawCallId, issuedDrawCalls[i].material);
            }
            
            issuedDrawCalls.Clear();
            
            int drawCallId = instanceId;
            shapeBatchPool.ReleaseAll();

            ShapeBatch shapeBatch = shapeBatchPool.Get(parameterTexture);

            Vector3 offset = camera.transform.position;
            offset.z = -2;
            Matrix4x4 rootMatrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);

            int drawCallCount = this.pendingDrawCalls.Count;
            PendingDrawCall[] pendingCalls = pendingDrawCalls.Array;

            VertigoEffect currentEffect = VertigoEffect.Default;
            IssuedDrawCall issuedDrawCall = default;

            issuedDrawCalls.EnsureCapacity(drawCallCount);

            for (int i = 0; i < drawCallCount; i++) {
                PendingDrawCall pendingDrawCall = pendingCalls[i];

                // do culling unless current effect wants to do culling itself (for things like shadow which render outside shape's bounds)
                // should maybe handle culling for masking if using aligned rect mask w/o texture

                // Cull(drawCall)

                if (pendingDrawCall.isStroke) {
                    Geometry.CreateStrokeGeometry(shapeBatch, pendingDrawCall.shapeRange, shapes, shapeData);
                }
                else {
                    Geometry.CreateFillGeometry(shapeBatch, pendingDrawCall.shapeRange, shapes, shapeData);
                }

                // draw current effect now if we need to     
                // change this to a vertex check
                if (currentEffect != pendingDrawCall.effect && shapeBatch.finalPositionList.Count > 0) { 
                    
                    issuedDrawCall = new IssuedDrawCall();
                    issuedDrawCall.drawCallId = drawCallId++;
                    issuedDrawCall.effect = currentEffect;
                    issuedDrawCall.batch = shapeBatch;
                    issuedDrawCall.state = state;
                    issuedDrawCalls.AddUnsafe(issuedDrawCall);
                    
                    shapeBatch = shapeBatchPool.Get(parameterTexture);
                }

                currentEffect = pendingDrawCall.effect;
                int start = pendingDrawCall.shapeRange.start;
                int end = pendingDrawCall.shapeRange.end;

                for (int j = start; j < end; j++) {
                    MeshSlice slice = new MeshSlice(shapeBatch, j);
                    pendingDrawCall.effect.Apply(shapeBatch, slice, state, pendingDrawCall.effectDataIndex);
                    slice.batch = null;
                }

            }

            issuedDrawCall = new IssuedDrawCall();
            issuedDrawCall.drawCallId = drawCallId++;
            issuedDrawCall.effect = currentEffect;
            issuedDrawCall.batch = shapeBatch;
            issuedDrawCall.state = state;
            issuedDrawCalls.AddUnsafe(issuedDrawCall);

            shapeBatchPool.Release(shapeBatch);
            
            shapeBatch = null;

            IssuedDrawCall[] issuedCalls = issuedDrawCalls.array;
            int issuedCount = issuedDrawCalls.size;
            parameterTexture.Upload();
            
            for (int i = 0; i < issuedCount; i++) {
                drawCallId = issuedCalls[i].drawCallId;
                Mesh mesh = issuedCalls[i].batch.GetBakedMesh();
                Material material = issuedCalls[i].effect.GetMaterialToDraw(drawCallId, issuedCalls[i].state, issuedCalls[i].batch, block);
                Graphics.DrawMesh(mesh, rootMatrix, material, 0, camera, 0, block, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
            }

        }

        public void SetTexture(Texture2D texture, int i) {
           // state.renderState.texture0 = texture;
        }

    }

}