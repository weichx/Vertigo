using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vertigo {

    public class VertigoContext {

        private float[] paths;

        private LightList<Vector3> shapeData;
        private Vector2 lastPoint;
        private LightList<Shape> shapes;
        private float3x2 transform;
        private LightList<DrawCall> drawCallList;
        public CommandBuffer commandBuffer;
        private RangeInt currentShapeRange;
        private VertigoEffect defaultEffect;
        public readonly int instanceId;
        private int pathIndex;

        private static int s_InstanceIdGenerator;
        private VertigoState state;
        private ShapeBatch.ShapeBatchPool shapeBatchPool;
        private StructList<IssuedDrawCall> issuedDrawCalls;
        public VertigoContext() {

            instanceId = s_InstanceIdGenerator;
            s_InstanceIdGenerator += 100000;

            shapeData = new LightList<Vector3>(128);
            shapes = new LightList<Shape>(64);
            drawCallList = new LightList<DrawCall>(32);
            commandBuffer = new CommandBuffer();
            issuedDrawCalls = new StructList<IssuedDrawCall>();
            shapeBatchPool = new ShapeBatch.ShapeBatchPool();
            shapes.Add(new Shape(ShapeType.Unset));
        }

        public void Clear() {
            shapes.Clear();
            shapeData.Clear();
            drawCallList.Clear();
            issuedDrawCalls.Clear();
            shapes.Add(new Shape(ShapeType.Unset));
            currentShapeRange = new RangeInt();
        }

        public void Stroke(Path2D path) {
            DrawCall call = new DrawCall();
            call.effect = VertigoEffect.Default;
            call.effectDataIndex = 0;
            call.isStroke = true;
            call.matrix = transform;
            call.shapeRange = currentShapeRange;
            drawCallList.Add(call);
        }

        private MaterialPropertyBlock block = new MaterialPropertyBlock();

        private void EnsureAdditionalCapacity(int size) {
            if (pathIndex + size >= paths.Length) {
                Array.Resize(ref paths, 2 * (pathIndex + size));
            }
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

            DrawCall call = new DrawCall();
            call.effect = VertigoEffect.Default;
            call.matrix = transform;
            call.isStroke = false;
            call.shapeRange = currentShapeRange;
            drawCallList.Add(call);
        }

        public void Fill(VertigoEffect effect) {
            if (currentShapeRange.length == 0) {
                return;
            }

            DrawCall call = new DrawCall();
            call.effect = effect ?? VertigoEffect.Default;
            call.matrix = transform;
            call.isStroke = false;
            call.shapeRange = currentShapeRange;
            call.effectDataIndex = effect.StoreState();
            drawCallList.Add(call);
        }

        public void MoveTo(float x, float y) {
            lastPoint.x = x;
            lastPoint.y = y;
            if (shapes[shapes.Count - 1].type != ShapeType.Unset) {
                shapes.Add(new Shape(ShapeType.Unset));
            }

            paths[pathIndex++] = (int) ShapeType.Path;
            paths[pathIndex++] = x;
            paths[pathIndex++] = y;
        }

        public void LineTo(float x, float y) {
            EnsureAdditionalCapacity(4);
            paths[pathIndex++] = (int) ShapeType.Path;
            paths[pathIndex++] = 2;
            paths[pathIndex++] = x;
            paths[pathIndex++] = y;

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

        }

        public void Render(Camera camera, RenderTexture targetTexture = null, bool clear = true) {

            int drawCallId = instanceId;
            shapeBatchPool.Release();

            commandBuffer.Clear();
            ShapeBatch shapeBatch = shapeBatchPool.Get();

            Vector3 offset = camera.transform.position;
            offset.z = -2;
            Matrix4x4 rootMatrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);

            int drawCallCount = drawCallList.Count;
            DrawCall[] drawCalls = drawCallList.Array;

            VertigoEffect currentEffect = VertigoEffect.Default;
            IssuedDrawCall issuedDrawCall = default;

            for (int i = 0; i < drawCallCount; i++) {
                DrawCall drawCall = drawCalls[i];

                // do culling unless current effect wants to do culling itself (for things like shadow which render outside shape's bounds)
                // should maybe handle culling for masking if using aligned rect mask w/o texture

                // Cull(drawCall)

                if (drawCall.isStroke) {
                    Geometry.CreateStrokeGeometry(shapeBatch, drawCall.shapeRange, shapes, shapeData);
                }
                else {
                    Geometry.CreateFillGeometry(shapeBatch, drawCall.shapeRange, shapes, shapeData);
                }

                // draw current effect now if we need to     
                if (currentEffect != drawCall.effect && shapeBatch.finalPositionList.Count > 0) { // change this to a vertex check
                    issuedDrawCall = new IssuedDrawCall();
                    issuedDrawCall.drawCallId = drawCallId++;
                    issuedDrawCall.effect = currentEffect;
                    issuedDrawCall.material = currentEffect.GetMaterialToDraw(drawCallId, state, shapeBatch, block);

                    Graphics.DrawMesh(shapeBatch.GetBakedMesh(), rootMatrix, issuedDrawCall.material, 0, camera, 0, null, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);

                    shapeBatch = shapeBatchPool.Get();
                    issuedDrawCalls.Add(issuedDrawCall);
                }

                currentEffect = drawCall.effect;
                int start = drawCall.shapeRange.start;
                int end = drawCall.shapeRange.end;

                for (int j = start; j < end; j++) {
                    MeshSlice slice = new MeshSlice(shapeBatch, j);
                    drawCall.effect.ModifyShapeMesh(shapeBatch, drawCall.effectDataIndex, slice, shapes.Array[j]);
                    slice.batch = null;
                }

            }

            issuedDrawCall = new IssuedDrawCall();
            issuedDrawCall.drawCallId = drawCallId++;
            issuedDrawCall.effect = currentEffect;
            issuedDrawCall.material = currentEffect.GetMaterialToDraw(drawCallId, state, shapeBatch, block);

            Graphics.DrawMesh(shapeBatch.GetBakedMesh(), rootMatrix, issuedDrawCall.material, 0, camera, 0, block, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
            // commandBuffer.DrawMesh(shapeBatch.GetBakedMesh(), Matrix4x4.identity, material, 0, 0, block);
            issuedDrawCalls.Add(issuedDrawCall);

            shapeBatch = shapeBatchPool.Get(); // let last batch get queued to release and setup for next frame

            
            for (int i = 0; i < issuedDrawCalls.Count; i++) {
                issuedDrawCalls[i].effect.ReleaseMaterial(issuedDrawCall.drawCallId, issuedDrawCall.material);
            }

        }

        public void SetTexture(Texture2D texture, int i) {
            state.renderState.texture0 = texture;
        }

    }

}