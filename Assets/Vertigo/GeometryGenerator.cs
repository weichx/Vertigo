using System;
using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    public class GeometryGenerator {

        private RenderState renderState;

        public GeometryGenerator() {
            this.renderState.strokeWidth = 1f;
            this.renderState.strokeColor = new Color32(0, 0, 0, 1);
            this.renderState.lineCap = LineCap.Butt;
            this.renderState.lineJoin = LineJoin.Miter;
            this.renderState.miterLimit = 10;
        }

        public struct RenderState {

            public Color32 strokeColor;
            public float strokeWidth;
            public LineCap lineCap;
            public LineJoin lineJoin;
            public int miterLimit;
            public Color32 fillColor;

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

        private static void FillRegularPolygon(GeometryCache retn, Color color, Vector3 position, float widthRadius, float heightRadius, int segmentCount) {
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
                vertexStart = vertexStart,
                vertexCount = vertexCount,
                triangleStart = triangleStart,
                triangleCount = triangleCount
            });
        }

        public GeometryCache Fill(ShapeGenerator shapeGenerator) {
            int shapeCount = shapeGenerator.shapes.Count;
            ShapeGenerator.ShapeDef[] shapes = shapeGenerator.shapes.array;
            GeometryCache retn = new GeometryCache();

            for (int i = 0; i < shapeCount; i++) {
                ShapeGenerator.ShapeDef shape = shapes[i];

                switch (shapes[i].shapeType) {
                    case ShapeType.Rect: {
                        Color color = renderState.fillColor;
                        retn.EnsureAdditionalCapacity(4, 6);

                        float x = shape.p0.x;
                        float y = -shape.p0.y;
                        float width = shape.p1.x;
                        float height = shape.p1.y;

                        Vector3 p0 = new Vector3(x, y);
                        Vector3 p1 = new Vector3(x + width, y);
                        Vector3 p2 = new Vector3(x + width, y + height);
                        Vector3 p3 = new Vector3(x, y + height);

                        Vector3 n0 = new Vector3(0, 0, -1);

                        Vector4 uv0 = new Vector4(0, 1);
                        Vector4 uv1 = new Vector4(1, 1);
                        Vector4 uv2 = new Vector4(1, 0);
                        Vector4 uv3 = new Vector4(0, 0);

                        int startVert = retn.positions.size;
                        Vector3[] positions = retn.positions.array;
                        Vector3[] normals = retn.normals.array;
                        Color[] colors = retn.colors.array;
                        Vector4[] texCoord0 = retn.texCoord0.array;

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

                        retn.AddQuad();

                        break;
                    }

                    case ShapeType.RoundedRect:
                        break;

                    case ShapeType.Circle: {
                        FillRegularPolygon(retn, renderState.fillColor, shape.p0, shape.p1.x, shape.p1.x, (int) shape.p2.x);
                        break;
                    }

                    case ShapeType.Ellipse: {
                        FillRegularPolygon(retn, renderState.fillColor, shape.p0, shape.p1.x, shape.p1.y, (int) shape.p2.x);
                        break;
                    }

                    case ShapeType.Polygon: {
                        FillRegularPolygon(retn, renderState.fillColor, shape.p0, shape.p1.x, shape.p1.y, (int) shape.p2.x);
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

                        int startVert = retn.positions.size;
                        Vector3[] positions = retn.positions.array;
                        Vector3[] normals = retn.normals.array;
                        Color[] colors = retn.colors.array;
                        Vector4[] texCoord0 = retn.texCoord0.array;

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

                        retn.AddQuad();
                        break;
                    }

                    case ShapeType.Path: {
                        break;
                    }

                    case ShapeType.ClosedPath: {
                        ShapeGenerator.PathDef pathDef = shape.pathDef;
                        if (pathDef.hasHoles) {
                            
                        }

                        int start = pathDef.pointRange.start;
                        int end = pathDef.pointRange.end;
                        
                        ShapeGenerator.PathPoint[] points = shapeGenerator.points.array;
                        LightList<float> floats = new LightList<float>();
                        
                        for (int j = start; j < end; j++) {
                            Vector2 position = points[j].position;
                            floats.Add(position.x);
                            floats.Add(position.y);
                        }

                        LightList<int> output = new LightList<int>();
                        
                        Earcut.Tessellate(floats, null, output);

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
                            vertexStart = startVert,
                            vertexCount = 3,
                            triangleStart = startTriangle,
                            triangleCount = 3
                        });

                        break;
                    }

                    case ShapeType.Other:
                        break;
                    case ShapeType.Unset:
                        break;
                  
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return retn;
        }

        private static float PercentOfRange(float v, float bottom, float top) {
            float div = top - bottom;
            return div == 0 ? 0 : (v - bottom) / div;
        }

        public void Dash(ShapeGenerator shape) { }

        public void Stroke(ShapeGenerator shape) { }

    }

}