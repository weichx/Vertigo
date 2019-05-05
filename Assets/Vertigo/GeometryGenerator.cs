using System;
using Unity.Mathematics;
using UnityEditor.Sprites;
using UnityEngine;

namespace Vertigo {

    public class GeometryGenerator {

        private RenderState renderState;
        private static readonly LightList<int> s_IntScratch0 = new LightList<int>(32);
        private static readonly LightList<int> s_IntScratch1 = new LightList<int>(32);
        private static readonly LightList<float> s_FloatScratch = new LightList<float>(32);
        private static readonly StructList<Vector2> s_ScratchVector2 = new StructList<Vector2>(32);
        private static readonly Vector3 DefaultNormal = new Vector3(0, 0, -1);

        public GeometryGenerator() {
            this.renderState.strokeWidth = 1f;
            this.renderState.strokeColor = new Color32(0, 0, 0, 1);
            this.renderState.lineCap = LineCap.Butt;
            this.renderState.lineJoin = LineJoin.Miter;
            this.renderState.miterLimit = 10;
        }

        public struct RenderState {

            public Color strokeColor;
            public float strokeWidth;
            public LineCap lineCap;
            public LineJoin lineJoin;
            public int miterLimit;
            public Color fillColor;

        }

        public void SetMiterLimit(int miterLimit) {
            this.renderState.miterLimit = miterLimit;
        }

        public void SetLineCap(LineCap lineCap) {
            this.renderState.lineCap = lineCap;
        }

        public void SetLineJoin(LineJoin lineJoin) {
            this.renderState.lineJoin = lineJoin;
        }

        public void SetStrokeColor(Color32 color) {
            renderState.strokeColor = color;
        }

        public void SetStrokeWidth(float width) {
            renderState.strokeWidth = width;
        }

        private static void FillRegularPolygon(GeometryCache retn, ShapeType shapeType, Color color, Vector3 position, float widthRadius, float heightRadius, int segmentCount) {
            Vector3 normal = new Vector3(0, 0, -1);

            int nPlus1 = segmentCount + 1;
            int nMinus2 = segmentCount - 2;

            int vertexStart = retn.positions.size;
            int triangleStart = retn.triangles.size;
            int vertexCount = nPlus1;
            int triangleCount = nMinus2 * 3;

            retn.EnsureAdditionalCapacity(vertexCount, triangleCount);

            Color[] colors = retn.colors.array;
            Vector3[] positions = retn.positions.array;
            Vector3[] normals = retn.normals.array;
            Vector4[] texCoord0 = retn.texCoord0.array;
            int[] triangles = retn.triangles.array;

            float width = 2 * widthRadius;
            float height = 2 * heightRadius;

            float centerX = position.x + widthRadius;
            float centerY = position.y + heightRadius;

            for (int i = 0; i < nPlus1; i++) {
                float a = (2 * Mathf.PI * i) / segmentCount;
                float x = widthRadius * math.sin(a);
                float y = heightRadius * math.cos(a);
                int vertIdx = vertexStart + i;
                float finalY = -(position.y + y);
                positions[vertIdx].x = position.x + x;
                positions[vertIdx].y = finalY;
                texCoord0[vertexStart + i].x = 1 + ((position.x + x - centerX) / width);
                texCoord0[vertexStart + i].y = 1 + ((finalY - centerY) / height);
            }

            for (int i = 0; i < nPlus1; i++) {
                colors[vertexStart + i] = color;
            }

            for (int i = 0; i < nPlus1; i++) {
                normals[vertexStart + i] = normal;
            }

            int s = retn.triangles.size;
            for (int i = 0; i < nMinus2; i++) {
                triangles[s++] = triangleStart;
                triangles[s++] = triangleStart + i + 1;
                triangles[s++] = triangleStart + i + 2;
            }

            retn.colors.size += vertexCount;
            retn.positions.size += vertexCount;
            retn.normals.size += vertexCount;
            retn.texCoord0.size += vertexCount;
            retn.texCoord1.size += vertexCount;
            retn.triangles.size += triangleCount;

            retn.shapes.Add(new GeometryShape() {
                shapeType = shapeType,
                geometryType = GeometryType.Physical,
                vertexStart = vertexStart,
                vertexCount = vertexCount,
                triangleStart = triangleStart,
                triangleCount = triangleCount
            });
        }

        public RangeInt Fill(ShapeGenerator shapeGenerator, RangeInt shapeRange, GeometryCache retn) {

            if (retn == null) {
                return default;
            }

            int shapeStart = shapeRange.start;
            int shapeEnd = shapeRange.end;

            ShapeGenerator.ShapeDef[] shapes = shapeGenerator.shapes.array;
            int geometryShapeStart = retn.shapeCount;
            int geometryShapeCount = 0;
            for (int i = shapeStart; i < shapeEnd; i++) {
                ShapeGenerator.ShapeDef shape = shapes[i];

                switch (shapes[i].shapeType) {
                    case ShapeType.Rect: {
                        Color color = renderState.fillColor;
                        retn.EnsureAdditionalCapacity(4, 6);

                        float x = shape.p0.x;
                        float y = shape.p0.y;
                        float width = shape.p1.x;
                        float height = shape.p1.y;

                        Vector3 p0 = new Vector3(x, -y);
                        Vector3 p1 = new Vector3(x + width, -y);
                        Vector3 p2 = new Vector3(x + width, -(y + height));
                        Vector3 p3 = new Vector3(x, -(y + height));

                        Vector3 n0 = new Vector3(0, 0, -1);

                        Vector4 uv0 = new Vector4(0, 1);
                        Vector4 uv1 = new Vector4(1, 1);
                        Vector4 uv2 = new Vector4(1, 0);
                        Vector4 uv3 = new Vector4(0, 0);

                        int startVert = retn.vertexCount;
                        int startTriangle = retn.triangleCount;

                        Vector3[] positions = retn.positions.array;
                        Vector3[] normals = retn.normals.array;
                        Color[] colors = retn.colors.array;
                        Vector4[] texCoord0 = retn.texCoord0.array;
                        int[] triangles = retn.triangles.array;

                        positions[startVert + 0] = p0;
                        positions[startVert + 1] = p1;
                        positions[startVert + 2] = p2;
                        positions[startVert + 3] = p3;

                        normals[startVert + 0] = n0;
                        normals[startVert + 1] = n0;
                        normals[startVert + 2] = n0;
                        normals[startVert + 3] = n0;

                        colors[startVert + 0] = color;
                        colors[startVert + 1] = color;
                        colors[startVert + 2] = color;
                        colors[startVert + 3] = color;

                        texCoord0[startVert + 0] = uv0;
                        texCoord0[startVert + 1] = uv1;
                        texCoord0[startVert + 2] = uv2;
                        texCoord0[startVert + 3] = uv3;

                        retn.shapes.Add(new GeometryShape() {
                            geometryType = GeometryType.Physical,
                            shapeType = ShapeType.Rect,
                            vertexStart = startVert,
                            vertexCount = 4,
                            triangleStart = startTriangle,
                            triangleCount = 6
                        });

                        triangles[startTriangle + 0] = startVert + 0;
                        triangles[startTriangle + 1] = startVert + 1;
                        triangles[startTriangle + 2] = startVert + 2;
                        triangles[startTriangle + 3] = startVert + 2;
                        triangles[startTriangle + 4] = startVert + 3;
                        triangles[startTriangle + 5] = startVert + 0;

                        retn.positions.size += 4;
                        retn.normals.size += 4;
                        retn.colors.size += 4;
                        retn.texCoord0.size += 4;
                        retn.texCoord1.size += 4;
                        retn.triangles.size += 6;
                        geometryShapeCount++;
                        break;
                    }

                    case ShapeType.RoundedRect:
                        break;

                    case ShapeType.Circle: {
                        FillRegularPolygon(retn, ShapeType.Circle, renderState.fillColor, shape.p0, shape.p1.x, shape.p1.x, (int) shape.p2.x);
                        geometryShapeCount++;
                        break;
                    }

                    case ShapeType.Ellipse: {
                        FillRegularPolygon(retn, ShapeType.Ellipse, renderState.fillColor, shape.p0, shape.p1.x, shape.p1.y, (int) shape.p2.x);
                        geometryShapeCount++;
                        break;
                    }

                    case ShapeType.Polygon: {
                        FillRegularPolygon(retn, ShapeType.Polygon, renderState.fillColor, shape.p0, shape.p1.x, shape.p1.y, (int) shape.p2.x);
                        geometryShapeCount++;
                        break;
                    }

                    case ShapeType.Rhombus: {
                        Color color = renderState.fillColor;
                        retn.EnsureAdditionalCapacity(4, 6);

                        float x = shape.p0.x;
                        float y = -shape.p0.y;
                        float width = shape.p1.x;
                        float height = shape.p1.y;

                        float halfWidth = width * 0.5f;
                        float halfHeight = height * 0.5f;

                        Vector3 n0 = new Vector3(0, 0, -1);

                        Vector4 uv0 = new Vector4(0.5f, 1);
                        Vector4 uv1 = new Vector4(1, 0.5f);
                        Vector4 uv2 = new Vector4(0.5f, 0f);
                        Vector4 uv3 = new Vector4(0f, 0.5f);

                        int startVert = retn.vertexCount;
                        int startTriangle = retn.triangleCount;
                        Vector3[] positions = retn.positions.array;
                        Vector3[] normals = retn.normals.array;
                        Color[] colors = retn.colors.array;
                        Vector4[] texCoord0 = retn.texCoord0.array;
                        int[] triangles = retn.triangles.array;

                        positions[startVert + 0] = new Vector3(x + halfWidth, -y);
                        positions[startVert + 1] = new Vector3(x + width, -(y + halfHeight));
                        positions[startVert + 2] = new Vector3(x + halfWidth, -(y + height));
                        positions[startVert + 3] = new Vector3(x, -(y + halfHeight));

                        normals[startVert + 0] = n0;
                        normals[startVert + 1] = n0;
                        normals[startVert + 2] = n0;
                        normals[startVert + 3] = n0;

                        colors[startVert + 0] = color;
                        colors[startVert + 1] = color;
                        colors[startVert + 2] = color;
                        colors[startVert + 3] = color;

                        texCoord0[startVert + 0] = uv0;
                        texCoord0[startVert + 1] = uv1;
                        texCoord0[startVert + 2] = uv2;
                        texCoord0[startVert + 3] = uv3;

                        retn.shapes.Add(new GeometryShape() {
                            shapeType = ShapeType.Rhombus,
                            geometryType = GeometryType.Physical,
                            vertexStart = startVert,
                            vertexCount = 4,
                            triangleStart = startTriangle,
                            triangleCount = 6
                        });

                        int vertexCount = startVert + 4;
                        triangles[startTriangle + 0] = vertexCount + 0;
                        triangles[startTriangle + 1] = vertexCount + 1;
                        triangles[startTriangle + 2] = vertexCount + 2;
                        triangles[startTriangle + 3] = vertexCount + 2;
                        triangles[startTriangle + 4] = vertexCount + 3;
                        triangles[startTriangle + 5] = vertexCount + 0;

                        retn.positions.size += 4;
                        retn.normals.size += 4;
                        retn.colors.size += 4;
                        retn.texCoord0.size += 4;
                        retn.texCoord1.size += 4;
                        retn.triangles.size += 6;
                        geometryShapeCount++;
                        break;
                    }

                    case ShapeType.Path: {
                        break;
                    }

                    case ShapeType.ClosedPath: {
                        Color color = renderState.fillColor;
                        ShapeGenerator.PathDef pathDef = shape.pathDef;

                        int pointRangeStart = pathDef.pointRange.start;
                        int pointRangeEnd = pathDef.pointRange.end;

                        int holeRangeStart = pathDef.holeRange.start;
                        int holeRangeEnd = pathDef.holeRange.end;

                        int vertexStart = retn.vertexCount;
                        int triangleStart = retn.triangleCount;

                        ShapeGenerator.PathPoint[] points = shapeGenerator.points.array;
                        ShapeGenerator.PathPoint[] holes = shapeGenerator.holes.array;

                        s_FloatScratch.EnsureCapacity(2 * (pointRangeEnd - pointRangeStart));
                        retn.EnsureAdditionalCapacity(pathDef.TotalVertices, 0);

                        int floatIdx = 0;
                        float[] floats = s_FloatScratch.Array;
                        Vector3[] positions = retn.positions.array;
                        Vector3[] normals = retn.normals.array;
                        Vector4[] texCoord0 = retn.texCoord0.array;
                        Color[] colors = retn.colors.array;
                        int vertexIdx = retn.positions.size;

                        float minX = shape.bounds.xMin;
                        float maxX = shape.bounds.xMax;
                        float minY = shape.bounds.yMin;
                        float maxY = shape.bounds.yMax;

                        Vector3 normal = new Vector3(0, 0, -1);

                        for (int j = pointRangeStart; j < pointRangeEnd; j++) {
                            Vector2 position = points[j].position;
                            floats[floatIdx++] = position.x;
                            floats[floatIdx++] = -position.y;
                            colors[vertexIdx] = color;
                            normals[vertexIdx] = normal;
                            positions[vertexIdx] = new Vector3(position.x, -position.y);
                            texCoord0[vertexIdx] = new Vector4(
                                PercentOfRange(position.x, minX, maxX),
                                1 - PercentOfRange(position.y, minY, maxY)
                            );
                            vertexIdx++;
                        }

                        for (int j = holeRangeStart; j < holeRangeEnd; j++) {
                            if ((holes[j].flags & ShapeGenerator.PointFlag.HoleStart) != 0) {
                                s_IntScratch0.Add(vertexIdx);
                            }

                            Vector2 position = holes[j].position;
                            floats[floatIdx++] = position.x;
                            floats[floatIdx++] = -position.y;
                            colors[vertexIdx] = color;
                            normals[vertexIdx] = normal;
                            positions[vertexIdx] = new Vector3(position.x, -position.y);
                            texCoord0[vertexIdx] = new Vector4(
                                PercentOfRange(position.x, minX, maxX),
                                1 - PercentOfRange(position.y, minY, maxY)
                            );
                            vertexIdx++;
                        }

                        s_FloatScratch.Count = floatIdx;

                        Earcut.Tessellate(s_FloatScratch, s_IntScratch0, s_IntScratch1);

                        int count = s_IntScratch1.Count;
                        int[] tessellatedIndices = s_IntScratch1.Array;

                        retn.EnsureAdditionalCapacity(0, count);
                        int triangleIdx = retn.triangles.size;
                        int[] triangles = retn.triangles.array;

                        for (int j = 0; j < count; j++) {
                            triangles[triangleIdx++] = tessellatedIndices[j];
                        }

                        s_IntScratch0.Count = 0;
                        s_IntScratch1.Count = 0;
                        s_FloatScratch.Count = 0;

                        retn.colors.size = vertexIdx;
                        retn.normals.size = vertexIdx;
                        retn.positions.size = vertexIdx;
                        retn.texCoord0.size = vertexIdx;
                        retn.texCoord1.size = vertexIdx;
                        retn.triangles.size = triangleIdx;

                        retn.shapes.Add(new GeometryShape() {
                            geometryType = GeometryType.Physical,
                            shapeType = ShapeType.ClosedPath,
                            vertexStart = vertexStart,
                            vertexCount = retn.vertexCount - vertexStart,
                            triangleStart = triangleStart,
                            triangleCount = retn.triangleCount - triangleStart
                        });
                        geometryShapeCount++;
                        break;
                    }

                    case ShapeType.Triangle: {
                        Color color = renderState.fillColor;
                        retn.EnsureAdditionalCapacity(3, 3);

                        Vector2 p0 = shape.p0;
                        Vector2 p1 = shape.p1;
                        Vector2 p2 = shape.p2;

                        Vector3 n0 = new Vector3(0, 0, -1);

                        float minX = shape.bounds.xMin;
                        float maxX = shape.bounds.xMax;
                        float minY = shape.bounds.yMin;
                        float maxY = shape.bounds.yMax;

                        Vector4 uv0 = new Vector4(
                            PercentOfRange(p0.x, minX, maxX),
                            1 - PercentOfRange(p0.y, minY, maxY)
                        );

                        Vector4 uv1 = new Vector4(
                            PercentOfRange(p1.x, minX, maxX),
                            1 - PercentOfRange(p1.y, minY, maxY)
                        );

                        Vector4 uv2 = new Vector4(
                            PercentOfRange(p2.x, minX, maxX),
                            1 - PercentOfRange(p2.y, minY, maxY)
                        );

                        int startVert = retn.positions.size;
                        int startTriangle = retn.triangles.size;
                        Vector3[] positions = retn.positions.array;
                        Vector3[] normals = retn.normals.array;
                        Color[] colors = retn.colors.array;
                        Vector4[] texCoord0 = retn.texCoord0.array;
                        int[] triangles = retn.triangles.array;

                        positions[startVert + 0] = new Vector3(p0.x, -p0.y);
                        positions[startVert + 1] = new Vector3(p1.x, -p1.y);
                        positions[startVert + 2] = new Vector3(p2.x, -p2.y);

                        normals[startVert + 0] = n0;
                        normals[startVert + 1] = n0;
                        normals[startVert + 2] = n0;

                        colors[startVert + 0] = color;
                        colors[startVert + 1] = color;
                        colors[startVert + 2] = color;

                        texCoord0[startVert + 0] = uv0;
                        texCoord0[startVert + 1] = uv1;
                        texCoord0[startVert + 2] = uv2;

                        triangles[startTriangle + 0] = startTriangle + 0;
                        triangles[startTriangle + 1] = startTriangle + 1;
                        triangles[startTriangle + 2] = startTriangle + 2;

                        retn.positions.size += 3;
                        retn.normals.size += 3;
                        retn.colors.size += 3;
                        retn.texCoord0.size += 3;
                        retn.texCoord1.size += 3;
                        retn.triangles.size += 3;

                        retn.shapes.Add(new GeometryShape() {
                            shapeType = ShapeType.Triangle,
                            geometryType = GeometryType.Physical,
                            vertexStart = startVert,
                            vertexCount = 3,
                            triangleStart = startTriangle,
                            triangleCount = 3
                        });
                        geometryShapeCount++;
                        break;
                    }

                }
            }

            return new RangeInt(geometryShapeStart, geometryShapeCount);
        }

        private static float PercentOfRange(float v, float bottom, float top) {
            float div = top - bottom;
            return div == 0 ? 0 : (v - bottom) / div;
        }

        public void Dash(ShapeGenerator shape) { }

        public void Stroke(ShapeGenerator shapeGenerator, GeometryCache retn = null) {
            int shapeCount = shapeGenerator.shapes.Count;
            ShapeGenerator.ShapeDef[] shapes = shapeGenerator.shapes.array;
            retn = retn ?? new GeometryCache();

            for (int i = 0; i < shapeCount; i++) {
                switch (shapes[i].shapeType) {
                    case ShapeType.Unset:
                        break;
                    case ShapeType.Rect:
                        break;
                    case ShapeType.RoundedRect:
                        break;
                    case ShapeType.Circle:
                        break;
                    case ShapeType.Ellipse:
                        break;
                    case ShapeType.Triangle:
                        break;
                    case ShapeType.Path:
                        StrokeOpenPath(shapeGenerator.points, shapeGenerator.shapes[i], retn);
                        retn.SetVertexColors(i, renderState.strokeColor);
                        retn.SetNormals(i, new Vector3(0, 0, -1));
                        break;
                    case ShapeType.ClosedPath:
                        StrokeClosedPath(shapeGenerator.points, shapeGenerator.shapes[i], retn);
                        retn.SetVertexColors(i, renderState.strokeColor);
                        retn.SetNormals(i, new Vector3(0, 0, -1));
                        break;
                    case ShapeType.Polygon:
                        break;
                    case ShapeType.Rhombus:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void GenerateStartCap(GeometryCache retn, ShapeGenerator.PathPoint[] pathPointArray, int startIdx) {
            float halfStrokeWidth = renderState.strokeWidth * 0.5f;
            Vector2 start = pathPointArray[startIdx + 0].position;
            Vector2 next = pathPointArray[startIdx + 1].position;
            start.y = -start.y;
            next.y = -next.y;
            Vector2 fromNext = (start - next).normalized;
            Vector2 perp = new Vector2(-fromNext.y, fromNext.x);
            int vertexCount = retn.vertexCount;

            if (renderState.lineCap == LineCap.Round) {
                // todo 
            }
            else if (renderState.lineCap == LineCap.TriangleOut) {

                retn.EnsureAdditionalCapacity(3, 3);

                retn.positions.AddUnsafe(start + (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(start - (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(start + (fromNext * halfStrokeWidth));

                retn.triangles.AddUnsafe(vertexCount + 0);
                retn.triangles.AddUnsafe(vertexCount + 1);
                retn.triangles.AddUnsafe(vertexCount + 2);
            }
            else if (renderState.lineCap == LineCap.TriangleIn) {

                retn.EnsureAdditionalCapacity(6, 6);

                retn.positions.AddUnsafe(start);
                retn.positions.AddUnsafe(start + (fromNext * halfStrokeWidth) + (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(start + (perp * halfStrokeWidth));

                retn.positions.AddUnsafe(start);
                retn.positions.AddUnsafe(start + (fromNext * halfStrokeWidth) - (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(start + (perp * -halfStrokeWidth));

                retn.triangles.AddUnsafe(vertexCount + 0);
                retn.triangles.AddUnsafe(vertexCount + 1);
                retn.triangles.AddUnsafe(vertexCount + 2);

                retn.triangles.AddUnsafe(vertexCount + 3);
                retn.triangles.AddUnsafe(vertexCount + 4);
                retn.triangles.AddUnsafe(vertexCount + 5);
            }
            else if (renderState.lineCap == LineCap.Square) {

                retn.EnsureAdditionalCapacity(4, 6);

                retn.positions.AddUnsafe(start + (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(start - (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(start + (fromNext * halfStrokeWidth) + (perp * -halfStrokeWidth));
                retn.positions.AddUnsafe(start + (fromNext * halfStrokeWidth) - (perp * -halfStrokeWidth));

                retn.triangles.AddUnsafe(vertexCount + 0);
                retn.triangles.AddUnsafe(vertexCount + 1);
                retn.triangles.AddUnsafe(vertexCount + 2);
                retn.triangles.AddUnsafe(vertexCount + 2);
                retn.triangles.AddUnsafe(vertexCount + 3);
                retn.triangles.AddUnsafe(vertexCount + 0);

            }

            int finalVertexCount = retn.vertexCount;
            retn.texCoord0.size = finalVertexCount;
            retn.texCoord1.size = finalVertexCount;
            retn.colors.size = finalVertexCount;
            retn.normals.size = finalVertexCount;
        }

        private void GenerateEndCap(GeometryCache retn, ShapeGenerator.PathPoint[] pathPointArray, int endIndex) {
            float halfStrokeWidth = renderState.strokeWidth * 0.5f;
            Vector2 end = pathPointArray[endIndex - 1].position;
            Vector2 prev = pathPointArray[endIndex - 2].position;
            end.y = -end.y;
            prev.y = -prev.y;

            Vector2 fromPrev = (prev - end).normalized;
            Vector2 perp = new Vector2(-fromPrev.y, fromPrev.x);
            int vertexCount = retn.vertexCount;

            if (renderState.lineCap == LineCap.Round) {
//                 Vector2 center = end;
//                 int segmentCount = (int) (math.abs(Math.PI * halfStrokeWidth) / 5) + 1;
//
//                 Vector3[] positions = retn.positions.array;
//                 int vertIdx = retn.vertexCount;
//                 int triIdx = retn.triangleCount;
//                 int[] triangles = retn.triangles.array;
//                 
//                 float angleInc = 180f / segmentCount;      
//                 
//                 for (int i = 0; i < segmentCount; i++) {
//                     
//                     positions[vertIdx++] = new Vector3(center.x, center.y);
//
//                     positions[vertIdx++] = new Vector3(
//                         center.x + halfStrokeWidth * math.cos(angleInc * i),
//                         center.y + halfStrokeWidth * math.sin(angleInc * i)
//                     );
//
//                     positions[vertIdx++] = new Vector3(
//                         center.x + halfStrokeWidth * math.cos(angleInc * (1 + i)),
//                         center.y + halfStrokeWidth * math.sin(angleInc * (1 + i))
//                     );
//
//                     triangles[triIdx++] = vertexCount + 0;
//                     triangles[triIdx++] = vertexCount + 1;
//                     triangles[triIdx++] = vertexCount + 2;
//                     vertexCount += 3;
//                 }
//                 
            }
            else if (renderState.lineCap == LineCap.TriangleOut) {

                retn.EnsureAdditionalCapacity(3, 3);

                retn.positions.AddUnsafe(end - (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(end + (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(end - (fromPrev * halfStrokeWidth));

                retn.triangles.AddUnsafe(vertexCount + 0);
                retn.triangles.AddUnsafe(vertexCount + 1);
                retn.triangles.AddUnsafe(vertexCount + 2);
            }
            else if (renderState.lineCap == LineCap.TriangleIn) {

                retn.EnsureAdditionalCapacity(6, 6);

                retn.positions.AddUnsafe(end);
                retn.positions.AddUnsafe(end - (fromPrev * halfStrokeWidth) - (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(end - (perp * halfStrokeWidth));

                retn.positions.AddUnsafe(end);
                retn.positions.AddUnsafe(end - (fromPrev * halfStrokeWidth) + (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(end - (perp * -halfStrokeWidth));

                retn.triangles.AddUnsafe(vertexCount + 0);
                retn.triangles.AddUnsafe(vertexCount + 1);
                retn.triangles.AddUnsafe(vertexCount + 2);

                retn.triangles.AddUnsafe(vertexCount + 3);
                retn.triangles.AddUnsafe(vertexCount + 4);
                retn.triangles.AddUnsafe(vertexCount + 5);
            }
            else if (renderState.lineCap == LineCap.Square) {
                retn.EnsureAdditionalCapacity(4, 6);

                retn.positions.AddUnsafe(end - (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(end + (perp * halfStrokeWidth));
                retn.positions.AddUnsafe(end - (fromPrev * halfStrokeWidth) - (perp * -halfStrokeWidth));
                retn.positions.AddUnsafe(end - (fromPrev * halfStrokeWidth) + (perp * -halfStrokeWidth));

                retn.triangles.AddUnsafe(vertexCount + 0);
                retn.triangles.AddUnsafe(vertexCount + 1);
                retn.triangles.AddUnsafe(vertexCount + 2);
                retn.triangles.AddUnsafe(vertexCount + 2);
                retn.triangles.AddUnsafe(vertexCount + 3);
                retn.triangles.AddUnsafe(vertexCount + 0);

            }

            int finalVertexCount = retn.vertexCount;
            retn.normals.size = finalVertexCount;
            retn.colors.size = finalVertexCount;
            retn.texCoord0.size = finalVertexCount;
            retn.texCoord1.size = finalVertexCount;
        }

        private void StrokeOpenPath(StructList<ShapeGenerator.PathPoint> pathPoints, in ShapeGenerator.ShapeDef shape, GeometryCache retn) {
            if (shape.pathDef.pointRange.length < 2) {
                return;
            }

            float halfStrokeWidth = renderState.strokeWidth * 0.5f;
            LineJoin join = renderState.lineJoin;
            int miterLimit = renderState.miterLimit;

            int vertexStart = retn.vertexCount;
            int triangleCount = retn.triangleCount;

            ComputeOpenPathSegments(shape.pathDef.pointRange, pathPoints, s_ScratchVector2);
            int count = s_ScratchVector2.size;
            Vector2[] midpoints = s_ScratchVector2.array;

            GenerateStartCap(retn, pathPoints.array, shape.pathDef.pointRange.start);

            ShapeGenerator.PathPoint[] pathPointArray = pathPoints.array;

            EnsureCapacityForStrokeTriangles(retn, join, count / 2);

            for (int i = 1; i < count; i++) {
                CreateStrokeTriangles(retn, midpoints[i - 1], pathPointArray[i].position, midpoints[i], halfStrokeWidth, join, miterLimit);
            }

            GenerateEndCap(retn, pathPoints.array, shape.pathDef.pointRange.end);

            retn.shapes.Add(new GeometryShape() {
                geometryType = GeometryType.PhysicalStroke,
                shapeType = ShapeType.Path,
                vertexStart = vertexStart,
                vertexCount = retn.vertexCount,
                triangleStart = triangleCount,
                triangleCount = retn.triangleCount
            });
        }

        private void StrokeClosedPath(StructList<ShapeGenerator.PathPoint> pathPoints, in ShapeGenerator.ShapeDef shape, GeometryCache retn) {
            if (shape.pathDef.pointRange.length < 2) {
                return;
            }

            float halfStrokeWidth = renderState.strokeWidth * 0.5f;
            LineJoin join = renderState.lineJoin;
            int miterLimit = renderState.miterLimit;

            int vertexStart = retn.vertexCount;
            int triangleCount = retn.triangleCount;

            ComputeClosedPathSegments(shape.pathDef.pointRange, pathPoints, s_ScratchVector2);

            int count = s_ScratchVector2.size;
            Vector2[] points = s_ScratchVector2.array;

            EnsureCapacityForStrokeTriangles(retn, join, count / 2);

            for (int i = 0; i < count - 2; i += 2) {
                CreateStrokeTriangles(retn, points[i], points[i + 1], points[i + 2], halfStrokeWidth, join, miterLimit);
            }

            retn.shapes.Add(new GeometryShape() {
                shapeType = ShapeType.ClosedPath,
                geometryType = GeometryType.PhysicalStroke,
                vertexStart = vertexStart,
                vertexCount = retn.vertexCount,
                triangleStart = triangleCount,
                triangleCount = retn.triangleCount
            });
        }

        private void ComputeClosedPathSegments(RangeInt range, StructList<ShapeGenerator.PathPoint> points, StructList<Vector2> segments) {
            ShapeGenerator.PathPoint[] pointData = points.array;
            int start = range.start + 1;
            int end = range.end - 1;
            segments.EnsureCapacity(range.length * 2);
            segments.size = 0;
            int ptIdx = 0;

            // find first mid point
            Vector2 p0 = pointData[start - 1].position;
            Vector2 m0 = (p0 + pointData[start].position) * 0.5f;

            segments[ptIdx++] = m0;

            for (int i = start; i < end; i++) {
                Vector2 current = pointData[i + 0].position;
                Vector2 next = pointData[i + 1].position;
                segments[ptIdx++] = current;
                segments[ptIdx++] = (current + next) * 0.5f;
            }

            segments[ptIdx++] = points[end].position;
            segments[ptIdx++] = (points[end].position + p0) * 0.5f;
            segments[ptIdx++] = p0;
            segments[ptIdx++] = m0;

            segments.size = ptIdx;
        }

        private void ComputeOpenPathSegments(RangeInt range, StructList<ShapeGenerator.PathPoint> points, StructList<Vector2> midpoints) {
            int count = range.length - 2;
            int ptIdx = 0;
            ShapeGenerator.PathPoint[] pointData = points.array;

            midpoints.EnsureCapacity(count + 2);
            midpoints.size = 0;

            Vector2[] data = midpoints.array;

            data[ptIdx].x = points[0].position.x;
            data[ptIdx].y = points[0].position.y;
            ptIdx++;

            int start = range.start + 1;
            int end = range.start + count;

            for (int i = start; i < end; i++) {
                Vector2 p0 = pointData[i + 0].position;
                Vector2 p1 = pointData[i + 1].position;
                data[ptIdx].x = (p0.x + p1.x) * 0.5f;
                data[ptIdx].y = (p0.y + p1.y) * 0.5f;
                ptIdx++;
            }

            data[ptIdx].x = points[end + 1].position.x;
            data[ptIdx].y = points[end + 1].position.y;
            midpoints.size = ptIdx + 1;
        }

        private void EnsureCapacityForStrokeTriangles(GeometryCache retn, LineJoin lineJoin, int segmentCount) {
            // 4 before join 
            // 4 after join
            // 3 - 6 for miter / bevel
            // ? for round, guessing about 16 but who won't know until we hit that case
            int beforeJoinSize = 4 * segmentCount;
            int afterJoinSize = 4 * segmentCount;
            if (lineJoin == LineJoin.Bevel || lineJoin == LineJoin.Miter || lineJoin == LineJoin.MiterClip) {
                int size = beforeJoinSize + afterJoinSize + (6 * segmentCount);
                retn.EnsureAdditionalCapacity(size, (int) (size * 1.5));
            }
            else {
                int size = beforeJoinSize + afterJoinSize + (16 * segmentCount);
                retn.EnsureAdditionalCapacity(size, (int) (size * 1.5));
            }
        }

        private void CreateStrokeTriangles(GeometryCache retn, Vector2 p0, Vector2 p1, Vector2 p2, float strokeWidth, LineJoin joinType, int miterLimit) {

            p0.y = -p0.y;
            p1.y = -p1.y;
            p2.y = -p2.y;

            Vector2 t0 = p1 - p0;
            Vector2 t2 = p2 - p1;

            float tempX = t0.x;
            t0.x = -t0.y;
            t0.y = tempX;

            tempX = t2.x;
            t2.x = -t2.y;
            t2.y = tempX;

            float signedArea = (p1.x - p0.x) * (p2.y - p0.y) - (p2.x - p0.x) * (p1.y - p0.y);

            // invert if signed area > 0
            if (signedArea > 0) {
                t0.x = -t0.x;
                t0.y = -t0.y;
                t2.x = -t2.x;
                t2.y = -t2.y;
            }

            t0 = t0.normalized;
            t2 = t2.normalized;

            t0 *= strokeWidth;
            t2 *= strokeWidth;

            Vector2 intersection = default;
            Vector2 anchor = default;
            float anchorLength = float.MaxValue;

            bool intersecting = LineIntersect(t0 + p0, t0 + p1, t2 + p2, t2 + p1, out intersection);

            if (intersecting) {
                anchor = intersection - p1;
                anchorLength = anchor.magnitude;
            }

            int dd = (int) (anchorLength / strokeWidth);

            Vector2 p0p1 = p0 - p1;
            float p0p1Length = p0p1.magnitude;

            Vector2 p1p2 = p1 - p2;
            float p1p2Length = p1p2.magnitude;

            int vertIdx = retn.vertexCount;
            int triangleIdx = retn.triangleCount;
            int[] triangles = retn.triangles.array;
            Vector3[] positions = retn.positions.array;
//            Color[] colors = retn.colors.array;

            // todo -- texcoords & sdf packing
            // todo -- fix reversed triangles, this only works w/ culling off right now
            // todo -- fix overdraw cases where possible
            // todo -- fix bad bending of joins at sharp angles
            // todo -- use burst & jobs

            // can't use the miter a s reference point
            if (anchorLength > p0p1Length || anchorLength > p1p2Length) {
                Vector2 v0 = p0 - t0;
                Vector2 v1 = p0 + t0;
                Vector2 v2 = p1 + t0;
                Vector2 v3 = p1 - t0;

                Vector2 v4 = p2 + t2;
                Vector2 v5 = p1 + t2;
                Vector2 v6 = p1 - t2;
                Vector2 v7 = p2 - t2;

//                colors[vertIdx + 0] = Color.red;
//                colors[vertIdx + 1] = Color.red;
//                colors[vertIdx + 2] = Color.red;
//                colors[vertIdx + 3] = Color.red;

                positions[vertIdx + 0] = v0;
                positions[vertIdx + 1] = v1;
                positions[vertIdx + 2] = v2;
                positions[vertIdx + 3] = v3;

                triangles[triangleIdx + 0] = vertIdx + 0;
                triangles[triangleIdx + 1] = vertIdx + 1;
                triangles[triangleIdx + 2] = vertIdx + 2;
                triangles[triangleIdx + 3] = vertIdx + 2;
                triangles[triangleIdx + 4] = vertIdx + 3;
                triangles[triangleIdx + 5] = vertIdx + 0;

                vertIdx += 4;
                triangleIdx += 6;

                if (joinType == LineJoin.Round) {
                    // generating unknown geometry in the cap function, have to write out our book keeping
                    retn.positions.size = vertIdx;
                    retn.normals.size = vertIdx;
                    retn.colors.size = vertIdx;
                    retn.texCoord0.size = vertIdx;
                    retn.texCoord1.size = vertIdx;
                    retn.triangles.size = triangleIdx;

                    CreateRoundJoin(retn, p1, v2, v5, p2);
                    vertIdx = retn.vertexCount;
                    triangleIdx = retn.triangleCount;

                    positions = retn.positions.array;
                    triangles = retn.triangles.array;
//                    colors = retn.colors.array;
                }
                else if (joinType == LineJoin.Bevel || joinType == LineJoin.Miter && dd >= miterLimit) {
                    positions[vertIdx + 0] = v2;
                    positions[vertIdx + 1] = p1;
                    positions[vertIdx + 2] = v5;

//                    colors[vertIdx + 0] = Color.yellow;
//                    colors[vertIdx + 1] = Color.yellow;
//                    colors[vertIdx + 2] = Color.yellow;

                    triangles[triangleIdx + 0] = vertIdx + 0;
                    triangles[triangleIdx + 1] = vertIdx + 1;
                    triangles[triangleIdx + 2] = vertIdx + 2;

                    vertIdx += 3;
                    triangleIdx += 3;
                }
                else if (joinType == LineJoin.Miter && dd < miterLimit && intersecting) {
                    positions[vertIdx + 0] = v2;
                    positions[vertIdx + 1] = intersection;
                    positions[vertIdx + 2] = v5;
                    positions[vertIdx + 3] = p1;

//                    colors[vertIdx + 0] = Color.yellow;
//                    colors[vertIdx + 1] = Color.yellow;
//                    colors[vertIdx + 2] = Color.yellow;
//                    colors[vertIdx + 3] = Color.yellow;

                    triangles[triangleIdx + 0] = vertIdx + 0;
                    triangles[triangleIdx + 1] = vertIdx + 1;
                    triangles[triangleIdx + 2] = vertIdx + 2;
                    triangles[triangleIdx + 3] = vertIdx + 2;
                    triangles[triangleIdx + 4] = vertIdx + 3;
                    triangles[triangleIdx + 5] = vertIdx + 0;

                    vertIdx += 4;
                    triangleIdx += 6;
                }

                positions[vertIdx + 0] = v4;
                positions[vertIdx + 1] = v5;
                positions[vertIdx + 2] = v6;
                positions[vertIdx + 3] = v7;

//                colors[vertIdx + 0] = Color.red;
//                colors[vertIdx + 1] = Color.red;
//                colors[vertIdx + 2] = Color.red;
//                colors[vertIdx + 3] = Color.red;

                triangles[triangleIdx + 0] = vertIdx + 0;
                triangles[triangleIdx + 1] = vertIdx + 1;
                triangles[triangleIdx + 2] = vertIdx + 2;
                triangles[triangleIdx + 3] = vertIdx + 2;
                triangles[triangleIdx + 4] = vertIdx + 3;
                triangles[triangleIdx + 5] = vertIdx + 0;

                vertIdx += 4;
                triangleIdx += 6;
            }
            else {
                Vector2 v0 = p1 - anchor;
                Vector2 v1 = p0 - t0;
                Vector2 v2 = p0 + t0;
                Vector2 v3 = p1 + t0;

                Vector2 v4 = v0;
                Vector2 v5 = p1 + t2;
                Vector2 v6 = p2 + t2;
                Vector2 v7 = p2 - t2;

                positions[vertIdx + 0] = v0;
                positions[vertIdx + 1] = v1;
                positions[vertIdx + 2] = v2;
                positions[vertIdx + 3] = v3;

//                colors[vertIdx + 0] = Color.white;
//                colors[vertIdx + 1] = Color.white;
//                colors[vertIdx + 2] = Color.white;
//                colors[vertIdx + 3] = Color.white;

                triangles[triangleIdx + 0] = vertIdx + 0;
                triangles[triangleIdx + 1] = vertIdx + 1;
                triangles[triangleIdx + 2] = vertIdx + 2;
                triangles[triangleIdx + 3] = vertIdx + 2;
                triangles[triangleIdx + 4] = vertIdx + 3;
                triangles[triangleIdx + 5] = vertIdx + 0;

                vertIdx += 4;
                triangleIdx += 6;

                if (joinType == LineJoin.Round) {
                    // generating unknown geometry in the cap function, have to write out our book keeping

                    positions[vertIdx + 0] = v3;
                    positions[vertIdx + 1] = p1; // + t2;
                    positions[vertIdx + 2] = v0;

//                    colors[vertIdx + 0] = Color.yellow;
//                    colors[vertIdx + 1] = Color.yellow;
//                    colors[vertIdx + 2] = Color.yellow;

                    triangles[triangleIdx + 0] = vertIdx + 0;
                    triangles[triangleIdx + 1] = vertIdx + 1;
                    triangles[triangleIdx + 2] = vertIdx + 2;

                    vertIdx += 3;
                    triangleIdx += 3;

                    retn.positions.size = vertIdx;
                    retn.normals.size = vertIdx;
                    retn.colors.size = vertIdx;
                    retn.texCoord0.size = vertIdx;
                    retn.texCoord1.size = vertIdx;
                    retn.triangles.size = triangleIdx;

                    CreateRoundJoin(retn, p1, v3, p1 + t2, v0);

                    vertIdx = retn.vertexCount;
                    triangleIdx = retn.triangleCount;

                    positions = retn.positions.array;
                    triangles = retn.triangles.array;
//                    colors = retn.colors.array;

                    positions[vertIdx + 0] = p1 - anchor;
                    positions[vertIdx + 1] = p1;
                    positions[vertIdx + 2] = p1 + t2;
//
//                    colors[vertIdx + 0] = Color.yellow;
//                    colors[vertIdx + 1] = Color.yellow;
//                    colors[vertIdx + 2] = Color.yellow;

                    triangles[triangleIdx + 0] = vertIdx + 0;
                    triangles[triangleIdx + 1] = vertIdx + 1;
                    triangles[triangleIdx + 2] = vertIdx + 2;

                    vertIdx += 3;
                    triangleIdx += 3;
                }
                else {
                    if (joinType == LineJoin.Bevel || joinType == LineJoin.Miter && dd >= miterLimit) {
                        positions[vertIdx + 0] = p1 + t2;
                        positions[vertIdx + 1] = p1 - anchor;
                        positions[vertIdx + 2] = p1 + t0;
//
//                        colors[vertIdx + 0] = Color.yellow;
//                        colors[vertIdx + 1] = Color.yellow;
//                        colors[vertIdx + 2] = Color.yellow;

                        triangles[triangleIdx + 0] = vertIdx + 0;
                        triangles[triangleIdx + 1] = vertIdx + 1;
                        triangles[triangleIdx + 2] = vertIdx + 2;

                        vertIdx += 3;
                        triangleIdx += 3;
                    }
                    else if (joinType == LineJoin.Miter && dd < miterLimit) {
                        // todo -- convert to quad

                        positions[vertIdx + 0] = p1 + t2;
                        positions[vertIdx + 1] = p1 - anchor;
                        positions[vertIdx + 2] = p1 + t0;
//
//                        colors[vertIdx + 0] = Color.yellow;
//                        colors[vertIdx + 1] = Color.yellow;
//                        colors[vertIdx + 2] = Color.yellow;

                        triangles[triangleIdx + 0] = vertIdx + 0;
                        triangles[triangleIdx + 1] = vertIdx + 1;
                        triangles[triangleIdx + 2] = vertIdx + 2;

                        vertIdx += 3;
                        triangleIdx += 3;

                        positions[vertIdx + 0] = p1 + t0;
                        positions[vertIdx + 1] = p1 + t2;
                        positions[vertIdx + 2] = intersection;

//                        colors[vertIdx + 0] = Color.yellow;
//                        colors[vertIdx + 1] = Color.yellow;
//                        colors[vertIdx + 2] = Color.yellow;

                        triangles[triangleIdx + 0] = vertIdx + 0;
                        triangles[triangleIdx + 1] = vertIdx + 1;
                        triangles[triangleIdx + 2] = vertIdx + 2;

                        vertIdx += 3;
                        triangleIdx += 3;
                    }
                }

                positions[vertIdx + 0] = v4;
                positions[vertIdx + 1] = v5;
                positions[vertIdx + 2] = v6;
                positions[vertIdx + 3] = v7;

//                colors[vertIdx + 0] = Color.white;
//                colors[vertIdx + 1] = Color.white;
//                colors[vertIdx + 2] = Color.white;
//                colors[vertIdx + 3] = Color.white;

                triangles[triangleIdx + 0] = vertIdx + 0;
                triangles[triangleIdx + 1] = vertIdx + 1;
                triangles[triangleIdx + 2] = vertIdx + 2;
                triangles[triangleIdx + 3] = vertIdx + 2;
                triangles[triangleIdx + 4] = vertIdx + 3;
                triangles[triangleIdx + 5] = vertIdx + 0;

                vertIdx += 4;
                triangleIdx += 6;
            }

            retn.positions.size = vertIdx;
            retn.normals.size = vertIdx;
            retn.colors.size = vertIdx;
            retn.texCoord0.size = vertIdx;
            retn.texCoord1.size = vertIdx;
            retn.triangles.size = triangleIdx;
        }

        private void CreateRoundJoin(GeometryCache retn, in Vector2 center, in Vector2 p0, in Vector2 p1, in Vector2 nextInLine) {
            const float Epsilon = 0.0001f;
            float radius = (center - p0).magnitude;
            float angle0 = math.atan2(p1.y - center.y, p1.x - center.x);
            float angle1 = math.atan2(p0.y - center.y, p0.x - center.x);
            float originalAngle = angle0;
            if (angle1 > angle0) {
                if (angle1 - angle0 >= Math.PI - 0.0001) {
                    angle1 -= 2 * Mathf.PI;
                }
            }
            else {
                if (angle0 - angle1 >= Math.PI - 0.0001) {
                    angle0 -= 2 * Mathf.PI;
                }
            }

            float angleDiff = angle1 - angle0;
            if (math.abs(angleDiff) >= Mathf.PI - Epsilon && math.abs(angleDiff) <= Mathf.PI + Epsilon) {
                Vector2 r1 = center - nextInLine;
                if (r1.x == 0) {
                    if (r1.y > 0) {
                        angleDiff = -angleDiff;
                    }
                }
                else if (r1.x >= -Epsilon) {
                    angleDiff = -angleDiff;
                }
            }

            int segmentCount = (int) (math.abs(angleDiff * radius) / 5) + 1;

            float angleInc = angleDiff / segmentCount;

            int vertexCount = retn.vertexCount;
            int triangleCount = retn.triangleCount;
            int triIdx = triangleCount;
            int vertIdx = vertexCount;

            retn.EnsureAdditionalCapacity(segmentCount * 5, segmentCount * 5);

            int[] triangles = retn.triangles.array;
            Vector3[] positions = retn.positions.array;

            // todo -- can do fewer sin / cos if we precompute and store since its always next & prev
            for (int i = 0; i < segmentCount; i++) {
                positions[vertIdx++] = new Vector3(center.x, center.y);

                positions[vertIdx++] = new Vector3(
                    center.x + radius * math.cos(originalAngle + angleInc * i),
                    center.y + radius * math.sin(originalAngle + angleInc * i)
                );

                positions[vertIdx++] = new Vector3(
                    center.x + radius * math.cos(originalAngle + angleInc * (1 + i)),
                    center.y + radius * math.sin(originalAngle + angleInc * (1 + i))
                );

                triangles[triIdx++] = vertexCount + 0;
                triangles[triIdx++] = vertexCount + 1;
                triangles[triIdx++] = vertexCount + 2;
                vertexCount += 3;
            }

            retn.positions.size = vertIdx;
            retn.normals.size = vertIdx;
            retn.colors.size = vertIdx;
            retn.texCoord0.size = vertIdx;
            retn.texCoord1.size = vertIdx;
            retn.triangles.size = triIdx;
        }

        private static bool LineIntersect(in Vector2 p0, in Vector2 p1, in Vector2 p2, in Vector2 p3, out Vector2 intersection) {
            const float Epsilon = 0.0001f;

            float a0 = p1.y - p0.y;
            float b0 = p0.x - p1.x;

            float a1 = p3.y - p2.y;
            float b1 = p2.x - p3.x;

            float det = a0 * b1 - a1 * b0;
            if (det > -Epsilon && det < Epsilon) {
                intersection = default;
                return false;
            }
            else {
                float c0 = a0 * p0.x + b0 * p0.y;
                float c1 = a1 * p2.x + b1 * p2.y;

                float x = (b1 * c0 - b0 * c1) / det;
                float y = (a0 * c1 - a1 * c0) / det;
                intersection = new Vector2(x, y);
                return true;
            }
        }

        public void SetFillColor(Color32 color) {
            renderState.fillColor = color;
        }

        public void ResetRenderState() {
            renderState.lineCap = LineCap.Butt;
            renderState.lineJoin = LineJoin.Miter;
            renderState.fillColor = Color.black;
            renderState.strokeColor = Color.white;
            renderState.strokeWidth = 1f;
        }

        public void FillSprite(Sprite sprite, Rect rect, GeometryCache retn) {
            VertigoUtil.SpriteData spriteData = VertigoUtil.GetSpriteData(sprite);
            Vector2[] vertices = spriteData.vertices;
            Vector2[] texCoords = spriteData.uvs;
            ushort[] spriteTriangles = spriteData.triangles;

            retn.EnsureAdditionalCapacity(vertices.Length, spriteData.triangles.Length);
            Vector3[] positions = retn.positions.array;
            Vector3[] normals = retn.normals.array;
            Color[] colors = retn.colors.array;
            Vector4[] texCoord0 = retn.texCoord0.array;
            int[] triangles = retn.triangles.array;
            int vertexStart = retn.vertexCount;
            int triangleStart = retn.triangleCount;
            int vertIdx = retn.vertexCount;
            int triIdx = retn.triangleCount;
            Vector2 pivot = sprite.pivot;
            float ppi = sprite.pixelsPerUnit;
            
            for (int i = 0; i < vertices.Length; i++) {
                positions[vertIdx].x = pivot.x - (vertices[i].x * ppi);
                positions[vertIdx].y = (vertices[i].y * ppi) - pivot.y;
                normals[vertIdx] = DefaultNormal;
                colors[vertIdx] = renderState.fillColor;
                texCoord0[vertIdx].x = texCoords[i].x;
                texCoord0[vertIdx].y = texCoords[i].y; 
                vertIdx++;
            }

            if (rect != default) {
                vertIdx = vertexStart;
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;
                for (int i = 0; i < vertices.Length; i++) {
                    float x = positions[vertIdx].x;
                    float y = -positions[vertIdx].y;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                    vertIdx++;
                }

                float boundsWidth = maxX - minX;
                float boundsHeight = maxY - minY;
                float baseX = rect.x;
                float baseY = rect.y;
                float targetWidth = rect.width;
                float targetHeight = rect.height;
                vertIdx = vertexStart;

                for (int i = 0; i < vertices.Length; i++) {
                    float x = positions[vertIdx].x;
                    float y = positions[vertIdx].y;
                    float percentX = PercentOfRange(x, minX, boundsWidth);
                    float percentY = PercentOfRange(y, minY, boundsHeight);
                    positions[vertIdx].x = baseX + (percentX * targetWidth);
                    positions[vertIdx].y = (percentY * targetHeight) - baseY;
                    vertIdx++;
                }
            }

            for (int i = 0; i < spriteTriangles.Length; i++) {
                triangles[triIdx++] = vertexStart + spriteTriangles[i];
            }

            retn.triangles.size = triIdx;
            retn.positions.size = vertIdx;
            retn.normals.size = vertIdx;
            retn.colors.size = vertIdx;
            retn.texCoord0.size = vertIdx;
            retn.texCoord1.size = vertIdx;

            retn.shapes.Add(new GeometryShape() {
                shapeType = ShapeType.Sprite,
                geometryType =  GeometryType.Physical,
                vertexStart = vertexStart,
                vertexCount = vertIdx,
                triangleStart = triangleStart,
                triangleCount = triIdx
            });
        }

    }

}