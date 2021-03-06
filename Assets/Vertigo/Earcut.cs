using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Vertigo {

    // Adapted from https://github.com/oberbichler/earcut.net/blob/master/src/Earcut.cs
    // which is a port of https://github.com/mapbox/earcut/blob/master/src/earcut.js
    // my adaptations involve pooling, removing allocations, and converting types to float

    public static class Earcut {

        private static readonly LightList<Node> inactive = new LightList<Node>(32);
        private static readonly LightList<Node> active = new LightList<Node>(32);
        private static readonly NodeComparer nodeComparer = new NodeComparer();
        private static readonly LightList<Node> holeQueue = new LightList<Node>();

        private class NodeComparer : IComparer<Node> {

            public int Compare(Node a, Node b) {
                return Math.Sign(a.x - b.x);
            }

        }

        public static void Tessellate(LightList<float> input, LightList<int> holeIndices, LightList<int> output) {
            float[] data = input.Array;
            bool hasHoles = holeIndices.Count > 0;
            int outerLen = hasHoles ? holeIndices[0] * 2 : input.Count;

            Node outerNode = LinkedList(data, 0, outerLen, true);
          
            LightList<int> triangles = output ?? new LightList<int>();

            if (outerNode == null) {
                return;
            }

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            float invSize = default(float);


            if (hasHoles) {
                outerNode = EliminateHoles(input, holeIndices, outerNode);
            }
         
            // if the shape is not too simple, we'll use z-order curve hash later; calculate polygon bbox
            if (input.Count > 80 * 2) {
                for (int i = 0; i < outerLen; i += 2) {
                    float x = data[i];
                    float y = data[i + 1];

                    if (x < minX) {
                        minX = x;
                    }

                    if (y < minY) {
                        minY = y;
                    }

                    if (x > maxX) {
                        maxX = x;
                    }

                    if (y > maxY) {
                        maxY = y;
                    }
                }

                // minX, minY and invSize are later used to transform coords into integers for z-order calculation
                invSize = Math.Max(maxX - minX, maxY - minY);
                invSize = invSize != 0 ? 1 / invSize : 0;
            }

            EarcutLinked(outerNode, triangles, minX, minY, invSize);

            // pooling clears on Get, not release so we don't need to re-iterate the list
            inactive.AddRange(active);
            active.QuickClear();
        }

        private static Node Get(int i, float x, float y) {
            Node retn = null;
            if (inactive.Count > 0) {
                retn = inactive.RemoveLast();
            }
            else {
                retn = new Node(i, x, y);
            }

            retn.steiner = false;
            retn.next = default;
            retn.prev = default;
            retn.prevZ = default;
            retn.nextZ = default;
            retn.z = default;
            retn.i = i;
            retn.x = x;
            retn.y = y;
            active.Add(retn);
            return retn;
        }

        // Creates a circular doubly linked list from polygon points in the specified winding order.
        private static Node LinkedList(float[] data, int start, int end, bool clockwise) {
            var last = default(Node);

            if (clockwise == (SignedArea(data, start, end) > 0)) {
                for (int i = start; i < end; i += 2) {
                    last = InsertNode(i, data[i], data[i + 1], last);
                }
            }
            else {
                for (int i = end - 2; i >= start; i -= 2) {
                    last = InsertNode(i, data[i], data[i + 1], last);
                }
            }

            if (last != null && Equals(last, last.next)) {
                RemoveNode(last);
                last = last.next;
            }

            return last;
        }

        // eliminate co-linear or duplicate points
        private static Node FilterPoints(Node start, Node end = null) {
            if (start == null) {
                return null;
            }

            if (end == null) {
                end = start;
            }

            var p = start;
            bool again;

            do {
                again = false;

                if (!p.steiner && (Equals(p, p.next) || Area(p.prev, p, p.next) == 0)) {
                    RemoveNode(p);
                    p = end = p.prev;
                    if (p == p.next) {
                        break;
                    }

                    again = true;

                }
                else {
                    p = p.next;
                }
            } while (again || p != end);

            return end;
        }

        // main ear slicing loop which triangulates a polygon (given as a linked list)
        private static void EarcutLinked(Node ear, LightList<int> triangles, float minX, float minY, float invSize, int pass = 0) {
            if (ear == null) {
                return;
            }

            // interlink polygon nodes in z-order
            if (pass == 0 && invSize != 0) {
                IndexCurve(ear, minX, minY, invSize);
            }

            var stop = ear;

            // iterate through ears, slicing them one by one
            while (ear.prev != ear.next) {
                var prev = ear.prev;
                var next = ear.next;

                if (invSize != 0 ? IsEarHashed(ear, minX, minY, invSize) : IsEar(ear)) {
                    // cut off the triangle
                    triangles.Add(prev.i / 2);
                    triangles.Add(ear.i / 2);
                    triangles.Add(next.i / 2);

                    RemoveNode(ear);

                    // skipping the next vertex leads to less sliver triangles
                    ear = next.next;
                    stop = next.next;

                    continue;
                }

                ear = next;

                // if we looped through the whole remaining polygon and can't find any more ears
                if (ear == stop) {
                    // try filtering points and slicing again
                    if (pass == 0) {
                        EarcutLinked(FilterPoints(ear), triangles, minX, minY, invSize, 1);

                        // if this didn't work, try curing all small self-intersections locally
                    }
                    else if (pass == 1) {
                        ear = CureLocalIntersections(ear, triangles);
                        EarcutLinked(ear, triangles, minX, minY, invSize, 2);

                        // as a last resort, try splitting the remaining polygon into two
                    }
                    else if (pass == 2) {
                        SplitEarcut(ear, triangles, minX, minY, invSize);
                    }

                    break;
                }
            }
        }

        // check whether a polygon node forms a valid ear with adjacent nodes
        private static bool IsEar(Node ear) {
            var a = ear.prev;
            var b = ear;
            var c = ear.next;

            if (Area(a, b, c) >= 0) {
                return false; // reflex, can't be an ear
            }

            // now make sure we don't have other points inside the potential ear
            var p = ear.next.next;

            while (p != ear.prev) {
                if (PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                    Area(p.prev, p, p.next) >= 0) {
                    return false;
                }

                p = p.next;
            }

            return true;
        }

        private static bool IsEarHashed(Node ear, float minX, float minY, float invSize) {
            var a = ear.prev;
            var b = ear;
            var c = ear.next;

            if (Area(a, b, c) >= 0) {
                return false; // reflex, can't be an ear
            }

            // triangle bounding box; min & max are calculated like this for speed
            var minTX = a.x < b.x ? (a.x < c.x ? a.x : c.x) : (b.x < c.x ? b.x : c.x);
            var minTY = a.y < b.y ? (a.y < c.y ? a.y : c.y) : (b.y < c.y ? b.y : c.y);
            var maxTX = a.x > b.x ? (a.x > c.x ? a.x : c.x) : (b.x > c.x ? b.x : c.x);
            var maxTY = a.y > b.y ? (a.y > c.y ? a.y : c.y) : (b.y > c.y ? b.y : c.y);

            // z-order range for the current triangle bounding box;
            var minZ = ZOrder(minTX, minTY, minX, minY, invSize);
            var maxZ = ZOrder(maxTX, maxTY, minX, minY, invSize);

            var p = ear.prevZ;
            var n = ear.nextZ;

            // look for points inside the triangle in both directions
            while (p != null && p.z >= minZ && n != null && n.z <= maxZ) {
                if (p != ear.prev && p != ear.next &&
                    PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                    Area(p.prev, p, p.next) >= 0) {
                    return false;
                }

                p = p.prevZ;

                if (n != ear.prev && n != ear.next &&
                    PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, n.x, n.y) &&
                    Area(n.prev, n, n.next) >= 0) {
                    return false;
                }

                n = n.nextZ;
            }

            // look for remaining points in decreasing z-order
            while (p != null && p.z >= minZ) {
                if (p != ear.prev && p != ear.next &&
                    PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                    Area(p.prev, p, p.next) >= 0) {
                    return false;
                }

                p = p.prevZ;
            }

            // look for remaining points in increasing z-order
            while (n != null && n.z <= maxZ) {
                if (n != ear.prev && n != ear.next &&
                    PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, n.x, n.y) &&
                    Area(n.prev, n, n.next) >= 0) {
                    return false;
                }

                n = n.nextZ;
            }

            return true;
        }

        // go through all polygon nodes and cure small local self-intersections
        private static Node CureLocalIntersections(Node start, LightList<int> triangles) {
            var p = start;
            do {
                var a = p.prev;
                var b = p.next.next;

                if (!Equals(a, b) && Intersects(a, p, p.next, b) && LocallyInside(a, b) && LocallyInside(b, a)) {

                    triangles.Add(a.i / 2);
                    triangles.Add(p.i / 2);
                    triangles.Add(b.i / 2);

                    // remove two nodes involved
                    RemoveNode(p);
                    RemoveNode(p.next);

                    p = start = b;
                }

                p = p.next;
            } while (p != start);

            return p;
        }

        // try splitting polygon into two and triangulate them independently
        private static void SplitEarcut(Node start, LightList<int> triangles, float minX, float minY, float invSize) {
            // look for a valid diagonal that divides the polygon into two
            var a = start;
            do {
                var b = a.next.next;
                while (b != a.prev) {
                    if (a.i != b.i && IsValidDiagonal(a, b)) {
                        // split the polygon in two by the diagonal
                        var c = SplitPolygon(a, b);

                        // filter co-linear points around the cuts
                        a = FilterPoints(a, a.next);
                        c = FilterPoints(c, c.next);

                        // run earcut on each half
                        EarcutLinked(a, triangles, minX, minY, invSize);
                        EarcutLinked(c, triangles, minX, minY, invSize);
                        return;
                    }

                    b = b.next;
                }

                a = a.next;
            } while (a != start);
        }

        // link every hole into the outer loop, producing a single-ring polygon without holes
        private static Node EliminateHoles(LightList<float> data, LightList<int> holeIndices, Node outerNode) {

            var len = holeIndices.Count;

            for (var i = 0; i < len; i++) {
                var start = holeIndices[i] * 2;
                var end = i < len - 1 ? holeIndices[i + 1] * 2 : data.Count;
                var list = LinkedList(data.Array, start, end, false);
                if (list == list.next) {
                    list.steiner = true;
                }

                holeQueue.Add(GetLeftmost(list));
            }

            holeQueue.Sort(nodeComparer);

            // process holes from left to right
            for (var i = 0; i < holeQueue.Count; i++) {
                EliminateHole(holeQueue[i], outerNode);
                outerNode = FilterPoints(outerNode, outerNode.next);
            }

            holeQueue.QuickClear();
           
            return outerNode;
        }

        // find a bridge between vertices that connects hole with an outer ring and and link it
        private static void EliminateHole(Node hole, Node outerNode) {
            outerNode = FindHoleBridge(hole, outerNode);
            if (outerNode != null) {
                var b = SplitPolygon(outerNode, hole);
                FilterPoints(b, b.next);
            }
        }

        // David Eberly's algorithm for finding a bridge between hole and outer polygon
        private static Node FindHoleBridge(Node hole, Node outerNode) {
            var p = outerNode;
            var hx = hole.x;
            var hy = hole.y;
            var qx = float.NegativeInfinity;
            Node m = null;

            // find a segment intersected by a ray from the hole's leftmost point to the left;
            // segment's endpoint with lesser x will be potential connection point
            do {
                if (hy <= p.y && hy >= p.next.y && p.next.y != p.y) {
                    var x = p.x + (hy - p.y) * (p.next.x - p.x) / (p.next.y - p.y);
                    if (x <= hx && x > qx) {
                        qx = x;
                        if (x == hx) {
                            if (hy == p.y) {
                                return p;
                            }

                            if (hy == p.next.y) {
                                return p.next;
                            }
                        }

                        m = p.x < p.next.x ? p : p.next;
                    }
                }

                p = p.next;
            } while (p != outerNode);

            if (m == null) {
                return null;
            }

            if (hx == qx) {
                return m.prev; // hole touches outer segment; pick lower endpoint
            }

            // look for points inside the triangle of hole point, segment intersection and endpoint;
            // if there are no points found, we have a valid connection;
            // otherwise choose the point of the minimum angle with the ray as connection point

            var stop = m;
            var mx = m.x;
            var my = m.y;
            var tanMin = float.PositiveInfinity;

            p = m.next;

            while (p != stop) {
                if (hx >= p.x && p.x >= mx && hx != p.x && PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, p.x, p.y)) {

                    var tan = Math.Abs(hy - p.y) / (hx - p.x);

                    if ((tan < tanMin || (tan == tanMin && p.x > m.x)) && LocallyInside(p, hole)) {
                        m = p;
                        tanMin = tan;
                    }
                }

                p = p.next;
            }

            return m;
        }

        // interlink polygon nodes in z-order
        private static void IndexCurve(Node start, float minX, float minY, float invSize) {
            Node p = start;
            do {
                if (p.z == null) {
                    p.z = ZOrder(p.x, p.y, minX, minY, invSize);
                }

                p.prevZ = p.prev;
                p.nextZ = p.next;
                p = p.next;
            } while (p != start);

            p.prevZ.nextZ = null;
            p.prevZ = null;

            SortLinked(p);
        }

        // Simon Tatham's linked list merge sort algorithm
        // http://www.chiark.greenend.org.uk/~sgtatham/algorithms/listsort.html
        private static Node SortLinked(Node list) {
            int numMerges;
            int inSize = 1;

            do {
                var p = list;
                list = null;
                Node tail = null;
                numMerges = 0;

                while (p != null) {
                    numMerges++;
                    var q = p;
                    var pSize = 0;
                    int i;
                    for (i = 0; i < inSize; i++) {
                        pSize++;
                        q = q.nextZ;
                        if (q == null) {
                            break;
                        }
                    }

                    var qSize = inSize;

                    while (pSize > 0 || (qSize > 0 && q != null)) {

                        Node e;
                        if (pSize != 0 && (qSize == 0 || q == null || p.z <= q.z)) {
                            e = p;
                            p = p.nextZ;
                            pSize--;
                        }
                        else {
                            e = q;
                            q = q.nextZ;
                            qSize--;
                        }

                        if (tail != null) {
                            tail.nextZ = e;
                        }
                        else {
                            list = e;
                        }

                        e.prevZ = tail;
                        tail = e;
                    }

                    p = q;
                }

                tail.nextZ = null;
                inSize *= 2;

            } while (numMerges > 1);

            return list;
        }

        // z-order of a point given coords and inverse of the longer side of data bounding box
        private static int ZOrder(float x, float y, float minX, float minY, float invSize) {
            // coords are transformed into non-negative 15-bit integer range
            int intX = (int) (32767 * (x - minX) * invSize);
            int intY = (int) (32767 * (y - minY) * invSize);

            intX = (intX | (intX << 8)) & 0x00FF00FF;
            intX = (intX | (intX << 4)) & 0x0F0F0F0F;
            intX = (intX | (intX << 2)) & 0x33333333;
            intX = (intX | (intX << 1)) & 0x55555555;

            intY = (intY | (intY << 8)) & 0x00FF00FF;
            intY = (intY | (intY << 4)) & 0x0F0F0F0F;
            intY = (intY | (intY << 2)) & 0x33333333;
            intY = (intY | (intY << 1)) & 0x55555555;

            return intX | (intY << 1);
        }

        // find the leftmost node of a polygon ring
        private static Node GetLeftmost(Node start) {
            Node p = start;
            Node leftmost = start;
            do {
                if (p.x < leftmost.x) {
                    leftmost = p;
                }

                p = p.next;
            } while (p != start);

            return leftmost;
        }

        // check if a point lies within a convex triangle
        private static bool PointInTriangle(float ax, float ay, float bx, float by, float cx, float cy, float px, float py) {
            return (cx - px) * (ay - py) - (ax - px) * (cy - py) >= 0 &&
                   (ax - px) * (by - py) - (bx - px) * (ay - py) >= 0 &&
                   (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0;
        }

        // check if a diagonal between two polygon nodes is valid (lies in polygon interior)
        private static bool IsValidDiagonal(Node a, Node b) {
            return a.next.i != b.i && a.prev.i != b.i && !IntersectsPolygon(a, b) &&
                   LocallyInside(a, b) && LocallyInside(b, a) && MiddleInside(a, b);
        }

        // signed area of a triangle
        static float Area(Node p, Node q, Node r) {
            return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        }

        // check if two points are equal
        private static bool Equals(Node p1, Node p2) {
            return p1.x == p2.x && p1.y == p2.y;
        }

        // check if two segments intersect
        private static bool Intersects(Node p1, Node q1, Node p2, Node q2) {
            if ((Equals(p1, q1) && Equals(p2, q2)) ||
                (Equals(p1, q2) && Equals(p2, q1))) {
                return true;
            }

            return Area(p1, q1, p2) > 0 != Area(p1, q1, q2) > 0 &&
                   Area(p2, q2, p1) > 0 != Area(p2, q2, q1) > 0;
        }

        // check if a polygon diagonal intersects any polygon segments
        private static bool IntersectsPolygon(Node a, Node b) {
            Node p = a;
            do {
                if (p.i != a.i && p.next.i != a.i && p.i != b.i && p.next.i != b.i &&
                    Intersects(p, p.next, a, b)) {
                    return true;
                }

                p = p.next;
            } while (p != a);

            return false;
        }

        // check if a polygon diagonal is locally inside the polygon
        private static bool LocallyInside(Node a, Node b) {
            return Area(a.prev, a, a.next) < 0 ? Area(a, b, a.next) >= 0 && Area(a, a.prev, b) >= 0 : Area(a, b, a.prev) < 0 || Area(a, a.next, b) < 0;
        }

        // check if the middle point of a polygon diagonal is inside the polygon
        private static bool MiddleInside(Node a, Node b) {
            var p = a;
            var inside = false;
            var px = (a.x + b.x) / 2;
            var py = (a.y + b.y) / 2;
            do {
                if (((p.y > py) != (p.next.y > py)) && p.next.y != p.y &&
                    (px < (p.next.x - p.x) * (py - p.y) / (p.next.y - p.y) + p.x)) {
                    inside = !inside;
                }

                p = p.next;
            } while (p != a);

            return inside;
        }

        // link two polygon vertices with a bridge; if the vertices belong to the same ring, it splits polygon into two;
        // if one belongs to the outer ring and another to a hole, it merges it into a single ring
        private static Node SplitPolygon(Node a, Node b) {
            var a2 = Get(a.i, a.x, a.y);
            var b2 = Get(b.i, b.x, b.y);
            var an = a.next;
            var bp = b.prev;

            a.next = b;
            b.prev = a;

            a2.next = an;
            an.prev = a2;

            b2.next = a2;
            a2.prev = b2;

            bp.next = b2;
            b2.prev = bp;

            return b2;
        }

        // create a node and optionally link it with previous one (in a circular doubly linked list)
        private static Node InsertNode(int i, float x, float y, Node last) {
            var p = Get(i, x, y);

            if (last == null) {
                p.prev = p;
                p.next = p;

            }
            else {
                p.next = last.next;
                p.prev = last;
                last.next.prev = p;
                last.next = p;
            }

            return p;
        }

        private static void RemoveNode(Node p) {
            p.next.prev = p.prev;
            p.prev.next = p.next;

            if (p.prevZ != null) {
                p.prevZ.nextZ = p.nextZ;
            }

            if (p.nextZ != null) {
                p.nextZ.prevZ = p.prevZ;
            }

        }

        private class Node {

            public int i;
            public float x;
            public float y;

            public int? z;

            public Node prev;
            public Node next;

            public Node prevZ;
            public Node nextZ;

            public bool steiner;

            public Node(int i, float x, float y) {
                // vertex index in coordinates array
                this.i = i;

                // vertex coordinates
                this.x = x;
                this.y = y;

                // previous and next vertex nodes in a polygon ring
                this.prev = null;
                this.next = null;

                // z-order curve value
                this.z = null;

                // previous and next nodes in z-order
                this.prevZ = null;
                this.nextZ = null;

                // indicates whether this is a steiner point
                this.steiner = false;
            }

        }

        private static float SignedArea(float[] data, int start, int end) {
            var sum = default(float);
            for (int i = start, j = end - 2; i < end; i += 2) {
                sum += (data[j] - data[i]) * (data[i + 1] + data[j + 1]);
                j = i;
            }

            return sum;
        }

        // return a percentage difference between the polygon area and its triangulation area;
        // used to verify correctness of triangulation
        public static float Deviation(LightList<float> input, LightList<int> holeIndices, LightList<int> triangles) {
            var hasHoles = holeIndices.Count > 0;
            var outerLen = hasHoles ? holeIndices[0] * 2 : input.Count;

            float[] data = input.Array;
            var polygonArea = Math.Abs(SignedArea(data, 0, outerLen));
            if (hasHoles) {
                var len = holeIndices.Count;

                for (var i = 0; i < len; i++) {
                    var start = holeIndices[i] * 2;
                    var end = i < len - 1 ? holeIndices[i + 1] * 2 : input.Count;
                    polygonArea -= Math.Abs(SignedArea(data, start, end));
                }
            }

            var trianglesArea = default(float);
            for (var i = 0; i < triangles.Count; i += 3) {
                var a = triangles[i] * 2;
                var b = triangles[i + 1] * 2;
                var c = triangles[i + 2] * 2;
                trianglesArea += Math.Abs(
                    (data[a] - data[c]) * (data[b + 1] - data[a + 1]) -
                    (data[a] - data[b]) * (data[c + 1] - data[a + 1]));
            }

            return polygonArea == 0 && trianglesArea == 0 ? 0 : Math.Abs((trianglesArea - polygonArea) / polygonArea);
        }

    }

}