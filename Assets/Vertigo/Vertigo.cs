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

        private int pathIndex;

        public VertigoContext() {
            shapeData = new LightList<Vector3>(128);
            shapes = new LightList<Shape>(64);
            drawCallList = new LightList<DrawCall>(32);
            commandBuffer = new CommandBuffer();
            shapes.Add(new Shape(ShapeType.Unset));
            renderedEffectList = new LightList<VertigoEffect>();
        }

        public void Clear() {
            shapes.Clear();
            shapeData.Clear();
            drawCallList.Clear();
            renderedEffectList.Clear();
            shapes.Add(new Shape(ShapeType.Unset));
            currentShapeRange = new RangeInt();
        }

        public void Stroke(Path2D path) {
            DrawCall call = new DrawCall();
            call.effect = VertigoEffect.Default;
            call.effectDataIndex = 0;
            call.stroke = true;
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
            call.stroke = false;
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
            call.stroke = false;
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

        public Material material;

        private LightList<VertigoEffect> renderedEffectList;

        public unsafe void Render(Camera camera, RenderTexture targetTexture = null, bool clear = true) {
//            commandBuffer.SetRenderTarget(targetTexture);

            commandBuffer.Clear();
            ShapeBatch shapeBatch = new ShapeBatch();

            if (clear) {
//                commandBuffer.ClearRenderTarget(true, true, Color.blue);
            }

            int drawCallCount = drawCallList.Count;
            DrawCall[] drawCalls = drawCallList.Array;

            for (int i = 0; i < drawCallCount; i++) {
                DrawCall drawCall = drawCalls[i];
                // do culling

                // do batching

                if (drawCall.effect != null && !renderedEffectList.Contains(drawCall.effect)) {
                    renderedEffectList.Add(drawCall.effect);
                }

                if (drawCall.stroke) { }
                else {
                    Geometry.CreateFillGeometry(shapeBatch, drawCall.shapeRange, shapes, shapeData);
                    int start = drawCall.shapeRange.start;
                    int end = drawCall.shapeRange.end;

                    for (int j = start; j < end; j++) {
                        MeshSlice slice = new MeshSlice(shapeBatch, j);
                        drawCall.effect?.Fill(shapeBatch, drawCall.effectDataIndex, slice, shapes.Array[j]);
                        slice.batch = null;
                    }
                }

                // commandBuffer.DrawMesh(shapeBatch.GetBakedMesh(), Matrix4x4.identity, material, 0, 0, block);
//                Graphics.DrawMesh(shapeBatch.GetBakedMesh(), Matrix4x4.identity, material, 0, 0, block);
                Vector3 offset = camera.transform.position;
                offset.z = -2;
                Matrix4x4 mat = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
                Mesh m = shapeBatch.GetBakedMesh();
                Graphics.DrawMesh(m, mat, material, 0, camera, 0, null, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
            }

            VertigoEffect[] renderedEffects = this.renderedEffectList.Array;
            for (int i = 0; i < renderedEffectList.Count; i++) {
                renderedEffects[i].ClearState(); // might need to take an id since effects can be reused 
            }

            renderedEffectList.Clear();
//            DrawCall lastCall = default;
//
//            List<Vertex> input = new List<Vertex>(128);
//            List<Vertex> output = new List<Vertex>(128);
//
//          
//            // todo handle rotation of target transform and take from position
//            Rect viewport = new Rect(0, 0, targetTexture.width, targetTexture.height);
//
//            MeshBuilder meshBuilder = new MeshBuilder();
//            List<Shape> renderedShapes = new List<Shape>();
//            List<Rect> boundsList = new List<Rect>();
//
//            for (int i = 0; i < drawCalls.Count; i++) {
//                // do culling (also apply scissor rect and maybe a basic clip test if clip shape is set and simple)
//                DrawCall call = drawCalls[i];
//
////                    call.effect.GetShapeBounds(call.path.shapes, points, boundsList);
//
//                for (int j = 0; j < call.path.shapes.Count; j++) {
//                    // effect get bounds(data, shapes, output);
//                //    if (!viewport.ContainOrOverlap(call.path.shapes[j].bounds)) {
//                        renderedShapes.Add(call.path.shapes[j]);
//                  //  }
//                }
//
//             //   Geometry.CreateFillGeometry(meshBuilder, call.matrix, renderedShapes);
//
//                // now generate geometry for non culled shapes
//                for (int j = 0; j < renderedShapes.Count; j++) { }
//
//                // now let the effect do it's magic
//                for (int j = 0; j < renderedShapes.Count; j++) { }
//
//                // call.effect.Fill(data, shape, meshBuilder);
//
//                if (!lastCall.CanBatchWith(ref call)) {
//                    block.Clear();
////                    lastCall.effect.Stroke();
//                    lastCall.effect.PopulateMaterialBlock(block);
//                    //   commandBuffer.DrawMesh(mesh, lastCall.matrix, lastCall.effect.material, 0, 0, block);
//                }
//
////                // if culling passes, generate base geometry
////                for (int j = 0; j < call.path.shapes.Count; j++) {
////                    Geometry.Stroke(5f, call.path.shapes[j], positions, input);
//                // call.effect.Stroke(this, call.effect, input, output);
////                    if (lastBatchMesh != null) {
////                        lastBatchMesh.Append(meshBuilder);
////                        meshBuilder.Clear();
////                    }
////                }
//            }

//            Graphics.ExecuteCommandBuffer(commandBuffer);
//            commandBuffer.Clear();
        }

        public void SetFill(Color color) { }

    }

}

// for each shape
// if shape is a curve it might bend out of the containing rect so we have to generate the geometry for it before cull testing
// if shape is text, test bounds
// if shape is path if any segment is in view just draw it all

// if the effect demands post-transformation culling we have to generate geometry, process it, then do culling

// generate points only if we need to (can rect test viewport)

// decompose into shapes 
// shape has a bounds
// shape belongs to a draw call
// for each draw call
// for each shape
// test against target texture size
// if full outside, drop the shape
// if shape count is 0
// drop the call

// another batching strategy
// when drawing an effect
// find other calls with same effect
// walk backwards doing bounds checks per draw call
// if found and no call between start and end intersect or contain the next call, add it to batch safely

// first do a cull pass, generate geometry as needed