using System;
using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    public class GeometryGenerator {

        private LineCap lineCap;
        private LineJoin lineJoin;
        private GeometryCache geometryCache = new GeometryCache();

        public void SetLineCap(LineCap lineCap) {
            this.lineCap = lineCap;
        }

        public void SetLineJoin(LineJoin lineJoin) {
            this.lineJoin = lineJoin;
        }

        public void Stroke(PathGenerator path) {
            
        }

        public void StrokeSDF(PathGenerator path) {
            for (int i = 0; i < path.shapes.size; i++) {
                StrokeSDFShape(path.shapes.array[i], path.points.array);
            }
        }

        private void StrokeSDFShape(in PathGenerator.ShapeDef shapeDef, PathGenerator.PathPoint[] points) {
            int dataStart = shapeDef.dataRange.start;
            
            switch (shapeDef.shapeType) {

                case ShapeType.Rect:
                    Vector3[] positions = geometryCache.positions.array;
                    Vector2 position = points[dataStart].position;
                    Vector2 size = points[dataStart + 1].position;
                    
                    int idx = geometryCache.vertexCount;
                    
                    positions[idx] = new Vector3(position.x, -position.y, 0);
                    positions[idx] = new Vector3(position.x, -position.y, 0);
                    positions[idx] = new Vector3(position.x, -position.y, 0);
                    positions[idx] = new Vector3(position.x, -position.y, 0);
                    
                    break;
                case ShapeType.RoundedRect:
                    break;
                case ShapeType.Circle:
                    break;
                case ShapeType.Ellipse:
                    break;
                case ShapeType.Path:
                    StrokeSDFPath(shapeDef, points);
                    break;
                case ShapeType.Triangle:
                    break;
                case ShapeType.Other:
                    break;
                case ShapeType.Unset:
                    break;
                case ShapeType.ClosedPath:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StrokeSDFPath(in PathGenerator.ShapeDef shapeDef, PathGenerator.PathPoint[] points) {
            GenerateCapStart(shapeDef, points);
        }

        private void GenerateCapStart(in PathGenerator.ShapeDef shapeDef, PathGenerator.PathPoint[] points) {
            float2 p0 = points[shapeDef.dataRange.start + 0].position;
            float2 p1 = points[shapeDef.dataRange.start + 1].position;

            float2 toNext = math.normalize(p1 - p0);
            float2 toNextPerp = new float2(-toNext.y, toNext.x);

            float strokeWidth = 0.5f;
            float dist = strokeWidth * 0.5f;

            if (lineCap == LineCap.Butt) {
                dist = 2f;
            }

            float2 v0 = p0 + (toNextPerp * (dist));
            float2 v1 = p0 + (toNextPerp * (-dist));
            float2 v2 = p0 - (toNext * dist) + (toNextPerp * (-dist));
            float2 v3 = p0 - (toNext * dist) + (toNextPerp * (dist));

            int idx = geometryCache.vertexCount;
            Vector3[] positions = geometryCache.positions.array;
            Vector4[] sdfData = geometryCache.texCoord1.array;

            positions[idx].x = v0.x;
            positions[idx].y = -v0.y;
            idx++;

            positions[idx].x = v1.x;
            positions[idx].y = -v1.y;
            idx++;

            positions[idx].x = v2.x;
            positions[idx].y = -v2.y;
            idx++;

            positions[idx].x = v3.x;
            positions[idx].y = -v3.y;
            idx++;

            geometryCache.vertexCount += 4;
        }

        public void Dash(PathGenerator path) { }

        public void Fill(PathGenerator path) { }

    }

}