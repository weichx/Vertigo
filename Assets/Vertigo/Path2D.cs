using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    public struct Path2D {

        public int id;
        public List<Shape> shapes;
        public Rect bounds;
        public List<float3> points;

        public void MoveTo(float x, float y) { }

        public void LineTo(float x, float y) { }

    }

}