using System;
using UnityEngine;

namespace Vertigo {

    public class ShapeGenerator {

        private Vector2 lastPoint;
        private PathDef currentPath;

        internal StructList<PathPoint> points;
        internal StructList<ShapeDef> shapes;
        private bool buildingPath;
        
        public ShapeGenerator(int capacity = 8) {
            this.shapes = new StructList<ShapeDef>(capacity);
            this.points = new StructList<PathPoint>(capacity * 8);
        }

//        public void MoveTo(float x, float y) {
//            if (!buildingPath) {
//                return;
//            }
//            lastPoint.x = x;
//            lastPoint.y = y;
//            points.Add(new PathPoint(x, y, PointFlag.Corner));
//            // todo - check this isn't duplicated
//            currentPath.pointRange.length++;
//
//        }

        public void LineTo(float x, float y) {
            points.Add(new PathPoint(x, y, PointFlag.Corner));
            currentPath.pointRange.length++;
        }

        public void BeginPath(float x, float y) {
            if (buildingPath) {
                // delete old path if it hasn't ended
            }
            currentPath = new PathDef();
            currentPath.pointRange.start = points.Count;
            currentPath.pointRange.length++;
            points.Add(new PathPoint(x, y, PointFlag.Corner));
            buildingPath = true;
        }

        public void ClosePath() {
            buildingPath = false;
            currentPath.isClosed = true;
            currentPath.pointRange.length++;
            points.Add(points[currentPath.pointRange.start]);
            ShapeDef shapeDef = new ShapeDef(ShapeType.ClosedPath);
            shapeDef.pathDef = currentPath;
            
            shapes.Add(shapeDef);
            currentPath = default;
        }

        public void EndPath() {
            buildingPath = false;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Path);
            shapeDef.pathDef = currentPath;
            shapes.Add(shapeDef);
            currentPath = default;
        }

        public void BeginHole() { }

        public void CloseHole() { }
        
        public void Rect(float x, float y, float width, float height) {
            ShapeDef shapeDef = new ShapeDef(ShapeType.Rect);
            shapeDef.p0 = new Vector2(x, y);
            shapeDef.p1 = new Vector2(width, height);
            shapeDef.bounds = new Rect(x, y, width, height);
            shapes.Add(shapeDef);
        }

        public void RoundedRect(float x, float y, float width, float height, float r0, float r1, float r2, float r3) {
            ShapeDef shapeDef = new ShapeDef(ShapeType.RoundedRect);
            shapeDef.p0 = new Vector2(x, y);
            shapeDef.p1 = new Vector2(width, height);
            shapeDef.p2 = new Vector2(r0, r1);
            shapeDef.p3 = new Vector2(r2, r3);
            shapeDef.bounds = new Rect(x, y, width, height);
            shapes.Add(shapeDef);
        }

        public void Circle(float x, float y, float r, int segmentCount = 50) {
            float diameter = r * 2;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Circle);
            shapeDef.p0 = new Vector2(x, y);
            shapeDef.p1 = new Vector2(r, 0);
            shapeDef.p2 = new Vector2(segmentCount, 0);
            shapeDef.bounds = new Rect(x, y, diameter, diameter);
            shapes.Add(shapeDef);
        }

        public void Ellipse(float x, float y, float rw, float rh, int segmentCount = 50) {
            ShapeDef shapeDef = new ShapeDef(ShapeType.Ellipse);
            shapeDef.p0 = new Vector2(x, y);
            shapeDef.p1 = new Vector2(rw, rh);
            shapeDef.p2 = new Vector2(segmentCount, 0);
            shapeDef.bounds = new Rect(x, y, rw * 2, rh * 2);
            shapes.Add(shapeDef);
        }

        public void RegularPolygon(float x, float y, float width, float height, int sides) {
            if (sides < 3) sides = 3;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Polygon);
            shapeDef.p0 = new Vector2(x, y);
            shapeDef.p1 = new Vector2(width, height);
            shapeDef.p2 = new Vector2(sides, 0);
            shapeDef.bounds = new Rect(x, y, width, height);
            shapes.Add(shapeDef);
        }

        public void Triangle(float x0, float y0, float x1, float y1, float x2, float y2) {
            ShapeDef shapeDef = new ShapeDef(ShapeType.Triangle);
            shapeDef.p0 = new Vector2(x0, y0);
            shapeDef.p1 = new Vector2(x1, y1);
            shapeDef.p2 = new Vector2(x2, y2);
            float minX = x0;
            float minY = y0;
            float maxX = x0;
            float maxY = y0;
            minX = x1 < minX ? x1 : minX;
            minX = x2 < minX ? x2 : minX;
            minY = y1 < minY ? y1 : minY;
            minY = y2 < minY ? y2 : minY;
            maxX = x1 > maxX ? x1 : maxX;
            maxX = x2 > maxX ? x2 : maxX;
            maxY = y1 > maxY ? y1 : maxY;
            maxY = y2 > maxY ? y2 : maxY;
            shapeDef.bounds = new Rect(minX, minY, maxX - minX, maxY - minY); 
            shapes.Add(shapeDef);
        }

        public void Rhombus(float x, float y, float width, float height) {
            ShapeDef shapeDef = new ShapeDef(ShapeType.Rhombus);
            shapeDef.p0 = new Vector2(x, y);
            shapeDef.p1 = new Vector2(width, height);
            shapeDef.bounds = new Rect(x, y, width, height);
            shapes.Add(shapeDef);
        }
        
        // equallateral triangle & iso triangle should be simple once triangle is done
        // could also add trapezoid, vesica , & cross
        
        [Flags]
        internal enum PointFlag {

            Hole = 1 << 1,
            Corner = 1 << 2

        }

        internal struct PathPoint {

            public PointFlag flags;
            public Vector2 position;

            public PathPoint(float x, float y, PointFlag flags = 0) {
                this.position.x = x;
                this.position.y = y;
                this.flags = flags;
            }

        }

        internal struct PathDef {

            public bool isClosed;
            public bool hasHoles;
            public RangeInt pointRange;

        }

        // todo -- crunch this down w/ field offsets, path def can be re-used
        internal struct ShapeDef {

            public Vector2 p0;
            public Vector2 p1;
            public Vector2 p2;
            public Vector2 p3;
            public Rect bounds;
            public PathDef pathDef;
            public readonly ShapeType shapeType;

            public ShapeDef(ShapeType shapeType) {
                this.p0 = default;
                this.p1 = default;
                this.p2 = default;
                this.p3 = default;
                this.pathDef = default;
                this.bounds = default;
                this.shapeType = shapeType;
            }

        }

        public void Clear() {
            throw new NotImplementedException();
        }

    }

}

//public void CubicCurveTo(Vector2 ctrl0, Vector2 ctrl1, Vector2 end) {
//int pointStart = points.Count;
//int pointCount = Bezier.CubicCurve(points, lastPoint, ctrl0, ctrl1, end);
//UpdateShape(pointStart, pointCount);
//lastPoint = end;
//}