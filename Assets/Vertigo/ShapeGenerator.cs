using System;
using UnityEngine;

namespace Vertigo {

    public class ShapeGenerator {

        private Vector2 lastPoint;
        private PathDef currentPath;

        internal StructList<PathPoint> points;
        internal StructList<PathPoint> holes;
        internal StructList<ShapeDef> shapes;

        private bool buildingPath;
        private bool inHole;

        public ShapeGenerator(int capacity = 8) {
            this.shapes = new StructList<ShapeDef>(capacity);
            this.points = new StructList<PathPoint>(capacity * 8);
            this.holes = new StructList<PathPoint>(capacity * 2);
        }

        public void LineTo(float x, float y) {
            if (!buildingPath) return;

            if (inHole) {
                holes.Add(new PathPoint(x, y, PointFlag.Corner | PointFlag.Hole));
                currentPath.holeRange.length++;
            }
            else {
                points.Add(new PathPoint(x, y, PointFlag.Corner));
                currentPath.pointRange.length++;
            }

            lastPoint = new Vector2(x, y);
        }

        public void HorizontalLineTo(float x) {
            LineTo(x, lastPoint.y);
        }

        public void VerticalLineTo(float y) {
            LineTo(lastPoint.x, y);
        }
        
        public void CubicBezierTo(float c0x, float c0y, float c1x, float c1y, float x, float y) {
            int pointCount = 0;
            Vector2 end = new Vector2(x, y);
            // todo flags
            if (inHole) {
                pointCount = Bezier.CubicCurve(holes, lastPoint, new Vector2(c0x, c0y), new Vector2(c1x, c1y), end);
                currentPath.holeRange.length += pointCount;
            }
            else {
                pointCount = Bezier.CubicCurve(points, lastPoint, new Vector2(c0x, c0y), new Vector2(c1x, c1y), end);
                currentPath.pointRange.length += pointCount;
            }

            lastPoint.x = end.x;
            lastPoint.y = end.y;
        }

        public void RectTo(float x, float y) {
            Vector2 start = lastPoint;
            HorizontalLineTo(x);
            VerticalLineTo(y);
            HorizontalLineTo(start.x);
            VerticalLineTo(start.y);
        }

        public void BeginPath(float x, float y) {
            if (buildingPath) {
                // delete old path if it hasn't ended
                points.Count = currentPath.pointRange.start;
                holes.Count = currentPath.holeRange.start;
            }

            currentPath = new PathDef();
            currentPath.pointRange.start = points.Count;
            currentPath.holeRange.start = holes.Count;
            currentPath.pointRange.length++;
            points.Add(new PathPoint(x, y, PointFlag.Corner));
            buildingPath = true;
            inHole = false; // just in case
            lastPoint = new Vector2(x, y);
        }

        public void ClosePath() {
            if (!buildingPath) return;
            buildingPath = false;
            inHole = false;
            ShapeDef shapeDef = new ShapeDef(ShapeType.ClosedPath);
            shapeDef.pathDef = currentPath;
            shapeDef.bounds = ComputePathBounds();
            ClampHoles(shapeDef.pathDef.holeRange, shapeDef.bounds);
            shapes.Add(shapeDef);
            currentPath = default;
            lastPoint.x = 0;
            lastPoint.y = 0;
        }

        public void EndPath() {
            if (!buildingPath) return;
            buildingPath = false;
            inHole = false;
            ShapeDef shapeDef = new ShapeDef(ShapeType.Path);
            shapeDef.pathDef = currentPath;
            shapeDef.bounds = ComputePathBounds();
            ClampHoles(shapeDef.pathDef.holeRange, shapeDef.bounds);
            shapes.Add(shapeDef);
            currentPath = default;
            lastPoint.x = 0;
            lastPoint.y = 0;
        }

        public void BeginHole(float x, float y) {
            inHole = true;
            holes.Add(new PathPoint(x, y, PointFlag.Corner | PointFlag.Hole | PointFlag.HoleStart));
            currentPath.holeRange.length++;
            lastPoint.x = x;
            lastPoint.y = y;
        }

        public void CloseHole() {
            inHole = false;
        }

        public void Rect(float x, float y, float width, float height) {
            ShapeDef shapeDef = new ShapeDef(ShapeType.Rect);
            shapeDef.p0 = new Vector2(x, y);
            shapeDef.p1 = new Vector2(width, height);
            shapeDef.bounds = new Rect(x, y, width, height);
            shapes.Add(shapeDef);
        }

        // todo -- not working yet
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

        private Rect ComputePathBounds() {
            int start = currentPath.pointRange.start;
            int end = currentPath.pointRange.end;
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            PathPoint[] array = points.array;
            for (int i = start; i < end; i++) {
                float x = array[i].position.x;
                float y = array[i].position.y;

                if (x < minX) {
                    minX = x;
                }

                if (x > maxX) {
                    maxX = x;
                }

                if (y < minY) {
                    minY = y;
                }

                if (y > maxY) {
                    maxY = y;
                }
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private void ClampHoles(in RangeInt holeRange, in Rect rect) {
            int start = holeRange.start;
            int end = holeRange.end;

            if (end - start == 0) {
                return;
            }
            
            PathPoint[] array = holes.array;
            
            float minX = rect.xMin;
            float minY = rect.yMin;
            float maxX = rect.xMax;
            float maxY = rect.yMax;
            for (int i = start; i < end; i++) {
                float x = array[i].position.x;
                float y = array[i].position.y;
                if (x < minX) x = minX;
                if (x > maxX) x = maxX;
                if (y < minY) y = minY;
                if (y > maxY) y = maxY;
                array[i].position.x = x;
                array[i].position.y = y;
            }
        }
        
        public void Clear() {
            shapes.QuickClear();
            points.QuickClear();
            holes.QuickClear();
            inHole = false;
            buildingPath = false;
            lastPoint = Vector2.zero;
        }

        [Flags]
        internal enum PointFlag {

            Hole = 1 << 1,
            Corner = 1 << 2,
            HoleStart = 1 << 3

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

            public RangeInt pointRange;
            public RangeInt holeRange;

            public int TotalVertices => pointRange.length + holeRange.length;

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

    }

}