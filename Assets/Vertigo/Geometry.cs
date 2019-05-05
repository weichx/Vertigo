using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    internal static class Geometry {

        public static void Stroke(float strokeWidth, in Shape pathShape, List<float4> points, List<Vertex> output) {
            switch (pathShape.type) {

                case ShapeType.Rect:

//                    // this should be a cut out, we don't want to do blending for all the pixels we aren't going to draw anyway
//
//                    meshBatch.EnsureAdditionalCapacity(4);
//
//                    float4 rect = points[pathShape.pointRange.start];
//
//                    Vertex topLeft = new Vertex();
//                    topLeft.position = rect.x;
//                    topLeft.uv0 = new float4(0, 1, 0, 0);
//
//                    meshBatch.AddVertex(new Vertex());
//                    meshBatch.AddVertex(new Vertex());
//                    meshBatch.AddVertex(new Vertex());
//                    meshBatch.AddVertex(new Vertex());
//
////                    meshBuilder.GetTriangles();
////                    meshBuilder.GetVertices();
//
//                    meshBatch.AddQuad();

                    break;

                case ShapeType.RoundedRect:
                    break;
                case ShapeType.Circle:
                    break;
                case ShapeType.Ellipse:
                    break;
                case ShapeType.Path:
                    break;
                case ShapeType.Triangle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void CreateRectFillGeometry(ShapeBatch shapeBatch, in Shape shape, StructList<Vector3> shapeData) {
            Vertex v0 = new Vertex();
            Vertex v1 = new Vertex();
            Vertex v2 = new Vertex();
            Vertex v3 = new Vertex();


            int start = shape.pointRange.start;

            Vector2 position = shapeData[start + 0];
            Vector2 size = shapeData[start + 1];

            v0.position.x = position.x;
            v0.position.y = -position.y;
            v0.position.z = 10;
            v0.texCoord0.x = 0;
            v0.texCoord0.y = 1;

            v1.position.x = position.x + size.x;
            v1.position.y = -position.y;
            v1.position.z = 10;
            v1.texCoord0.x = 1;
            v1.texCoord0.y = 1;

            v2.position.x = position.x + size.x;
            v2.position.y = -position.y - size.y;
            v2.position.z = 10;

            v2.texCoord0.x = 1;
            v2.texCoord0.y = 0;

            v3.position.x = position.x;
            v3.position.y = -position.y - size.y;
            v3.position.z = 10;
            v3.texCoord0.x = 0;
            v3.texCoord0.y = 0;

            ShapeMeshData data = new ShapeMeshData();
            data.isSDF = false;
            data.bounds = shape.bounds;
            data.shapeType = shape.type;
            data.creationRange = shape.pointRange;
            data.meshRange = shapeBatch.AddQuadWithRange(
                v0, v1, v2, v3
            );

            shapeBatch.AddShapeMeshData(data);
        }

        private static void CreateCircleFillGeometry(ShapeBatch shapeBatch, in Shape shape, StructList<Vector3> shapeData) {
            // todo -- even in the sdf case we can 'clip' the corners of the rect by some amount to get a better fit

            Vertex v0 = new Vertex();
            Vertex v1 = new Vertex();
            Vertex v2 = new Vertex();
            Vertex v3 = new Vertex();

            int start = shape.pointRange.start;

            Vector2 position = shapeData[start + 0];
            Vector2 size = shapeData[start + 1];

            v0.position.x = position.x;
            v0.position.y = position.y;
            v0.position.z = 10f;
            v0.texCoord0.x = 0;
            v0.texCoord0.y = 1;

            v1.position.x = position.x + size.x;
            v1.position.y = position.y;
            v1.position.z = 10f;
            v1.texCoord0.x = 1;
            v1.texCoord0.y = 1;

            v2.position.x = position.x + size.x;
            v2.position.y = (position.y + size.y);
            v2.position.z = 10f;
            v2.texCoord0.x = 1;
            v2.texCoord0.y = 0;

            v3.position.x = position.x;
            v3.position.y = (position.y + size.y);
            v3.position.z = 10f;
            v3.texCoord0.x = 0;
            v3.texCoord0.y = 0;

            ShapeMeshData data = new ShapeMeshData();
            data.isSDF = false;
            data.bounds = shape.bounds;
            data.shapeType = shape.type;
            data.creationRange = shape.pointRange;
            data.meshRange = shapeBatch.AddQuadWithRange(
                v0, v1, v2, v3
            );

            shapeBatch.AddShapeMeshData(data);
        }

        public static void CreateFillGeometry(ShapeBatch output, in Shape shape, StructList<Vector3> shapeData) {
            switch (shape.type) {

                case ShapeType.Rect:
                    CreateRectFillGeometry(output, shape, shapeData);
                    break;

                case ShapeType.RoundedRect:
                    return;
                case ShapeType.Circle:
                    CreateCircleFillGeometry(output, shape, shapeData);
                    return;
                case ShapeType.Ellipse:
                    return;
                case ShapeType.Path:
                    return;
                case ShapeType.Triangle:
                    return;
                case ShapeType.Unset:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public static void CreateFillGeometry(ShapeBatch output, RangeInt shapeRange, StructList<Shape> shapeList, StructList<Vector3> shapeData) {
            int start = shapeRange.start;
            int end = shapeRange.end;

            Shape[] shapes = shapeList.Array;

            for (int i = start; i < end; i++) {
                CreateFillGeometry(output, shapes[i], shapeData);
            }

        }

        public static void CreateStrokeGeometry(ShapeBatch shapeBatch, RangeInt drawCallShapeRange, StructList<Shape> shapes, StructList<Vector3> shapeData) {
            throw new NotImplementedException();
        }

    }

}