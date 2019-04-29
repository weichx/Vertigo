using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo {

    public struct MeshSlice {

        internal ShapeBatch batch;

        public readonly int vertexCount;
        public readonly int triangleCount;

        internal readonly int vertexStart;
        internal readonly int triangleStart;

        internal MeshSlice(ShapeBatch batch, int shapeIdx) {
            this.batch = batch;
            
            MeshRange meshRange = batch.shapeList.Array[shapeIdx].meshRange;

            this.vertexCount = meshRange.vertexRange.length;
            this.triangleCount = meshRange.triangleRange.length;
            this.vertexStart = meshRange.vertexRange.start;
            this.triangleStart = meshRange.triangleRange.start;
        }

        public bool SetVertex(int idx, in Vertex vertex) {
            int listIdx = vertexStart + idx;
            if (listIdx < vertexStart || listIdx >= vertexStart + vertexCount) {
                return false;
            }

            batch.initialPosition[listIdx] = vertex.position;
            batch.initialNormal[listIdx] = vertex.normal;
            batch.initialColor[listIdx] = vertex.color;
            batch.initialTexCoord0[listIdx] = vertex.texCoord0;
            batch.initialTexCoord1[listIdx] = vertex.texCoord1;
            batch.initialTexCoord2[listIdx] = vertex.texCoord1;
            return true;
        }

        public bool SetPosition(int idx, in Vector3 position) {
            int listIdx = vertexStart + idx;
            if (listIdx < vertexStart || listIdx >= vertexStart + vertexCount) {
                return false;
            }

            batch.initialPosition[listIdx] = position;
            return true;
        }

        public bool SetNormal(int idx, in Vector3 normal) {
            int listIdx = vertexStart + idx;
            if (listIdx < vertexStart || listIdx >= vertexStart + vertexCount) {
                return false;
            }

            batch.initialNormal[listIdx] = normal;
            return true;
        }

        public bool SetColor(int idx, in Color color) {
            int listIdx = vertexStart + idx;
            if (listIdx < vertexStart || listIdx >= vertexStart + vertexCount) {
                return false;
            }

            batch.initialColor[listIdx] = color;
            return true;
        }

        public bool SetTexCoord0(int idx, in Vector4 texCoord) {
            int listIdx = vertexStart + idx;
            if (listIdx < vertexStart || listIdx >= vertexStart + vertexCount) {
                return false;
            }

            batch.initialTexCoord0[listIdx] = texCoord;
            return true;
        }

        public bool SetTexCoord1(int idx, in Vector4 texCoord) {
            int listIdx = vertexStart + idx;
            if (listIdx < vertexStart || listIdx >= vertexStart + vertexCount) {
                return false;
            }

            batch.initialTexCoord1[listIdx] = texCoord;
            return true;
        }

        public bool SetTexCoord2(int idx, in Vector4 texCoord) {
            int listIdx = vertexStart + idx;
            if (listIdx < vertexStart || listIdx >= vertexStart + vertexCount) {
                return false;
            }

            batch.initialTexCoord2[listIdx] = texCoord;
            return true;
        }

        public void SetVertices(Vertex[] vertices, int count = -1) {
            if (count == -1) {
                count = vertices.Length;
            }

            if (count != vertexCount) {
                throw new Exception("Cannot set a vertex list with a different number vertices than were originally defined in a MeshSlice");
            }

            for (int i = 0; i < count; i++) {
                int idx = vertexStart + i;
                batch.initialPosition[idx] = vertices[i].position;
                batch.initialNormal[idx] = vertices[i].normal;
                batch.initialColor[idx] = vertices[i].color;
                batch.initialTexCoord0[idx] = vertices[i].texCoord0;
                batch.initialTexCoord1[idx] = vertices[i].texCoord1;
                batch.initialTexCoord2[idx] = vertices[i].texCoord2;
            }
        }

        public int GetVertices(ref Vertex[] retn) {
            if (retn == null) {
                retn = new Vertex[vertexCount];
            }

            if (retn.Length < vertexCount) {
                Array.Resize(ref retn, vertexCount);
            }

            int idx = 0;
            int vertexEnd = vertexStart + vertexCount;

            for (int i = vertexStart; i < vertexEnd; i++) {
                retn[idx].position = batch.initialPosition[i];
                retn[idx].color = batch.initialColor[i];
                retn[idx].normal = batch.initialNormal[i];
                retn[idx].texCoord0 = batch.initialTexCoord0[i];
                retn[idx].texCoord1 = batch.initialTexCoord1[i];
                retn[idx].texCoord2 = batch.initialTexCoord2[i];
                idx++;
            }

            return vertexCount;
        }

        public void SetPositions(List<Vector3> positionsToCopy) { }

        public void SetPositions(Vector3[] positionsToCopy, int start, int count) { }

        public void SetPositions(Vector3[] positionsToCopy, int count = -1) {
            if (count < 0) count = positionsToCopy.Length;
            if (count != vertexCount) {
                throw new Exception("Cannot set a position list with a different number inputs than were originally defined in a MeshSlice");
            }

            Array.Copy(positionsToCopy, 0, batch.initialPosition.Array, vertexStart, vertexCount);
        }

        public void FillShapeBuffer(ShapeMeshBuffer buffer, VertexChannel channels = VertexChannel.All) {
            if (channels == 0) return;

            buffer.EnsureCapacity(vertexCount, triangleCount);

            if (channels == VertexChannel.All) {
                Array.Copy(batch.initialColor.Array, vertexStart, buffer.colorList.Array, 0, vertexCount);
                Array.Copy(batch.initialNormal.Array, vertexStart, buffer.normalList.Array, 0, vertexCount);
                Array.Copy(batch.initialPosition.Array, vertexStart, buffer.positionList.Array, 0, vertexCount);

                Array.Copy(batch.initialTexCoord0.Array, vertexStart, buffer.texCoord0List.Array, 0, vertexCount);
                Array.Copy(batch.initialTexCoord1.Array, vertexStart, buffer.texCoord1List.Array, 0, vertexCount);
                Array.Copy(batch.initialTexCoord2.Array, vertexStart, buffer.texCoord2List.Array, 0, vertexCount);

                Array.Copy(batch.initialTriangles.Array, triangleStart, buffer.triangleList.Array, 0, triangleCount);

                buffer.colorList.Count = vertexCount;
                buffer.normalList.Count = vertexCount;
                buffer.positionList.Count = vertexCount;
                buffer.texCoord0List.Count = vertexCount;
                buffer.texCoord1List.Count = vertexCount;
                buffer.texCoord2List.Count = vertexCount;
                buffer.triangleList.Count = triangleCount;
                return;
            }

            if ((channels & VertexChannel.Position) != 0) {
                Array.Copy(batch.initialPosition.Array, vertexStart, buffer.positionList.Array, 0, vertexCount);
            }

            if ((channels & VertexChannel.Normal) != 0) {
                Array.Copy(batch.initialNormal.Array, vertexStart, buffer.normalList.Array, 0, vertexCount);
            }

            if ((channels & VertexChannel.Color) != 0) {
                Array.Copy(batch.initialColor.Array, vertexStart, buffer.colorList.Array, 0, vertexCount);
            }

            if ((channels & VertexChannel.TexCoord0) != 0) {
                Array.Copy(batch.initialTexCoord0.Array, vertexStart, buffer.texCoord0List.Array, 0, vertexCount);
            }

            if ((channels & VertexChannel.TexCoord1) != 0) {
                Array.Copy(batch.initialTexCoord1.Array, vertexStart, buffer.texCoord1List.Array, 0, vertexCount);
            }

            if ((channels & VertexChannel.TexCoord2) != 0) {
                Array.Copy(batch.initialTexCoord2.Array, vertexStart, buffer.texCoord2List.Array, 0, vertexCount);
            }

            buffer.colorList.Count = vertexCount;
            buffer.normalList.Count = vertexCount;
            buffer.positionList.Count = vertexCount;
            buffer.texCoord0List.Count = vertexCount;
            buffer.texCoord1List.Count = vertexCount;
            buffer.texCoord2List.Count = vertexCount;
            buffer.triangleList.Count = triangleCount;
            
            Array.Copy(batch.initialTriangles.Array, triangleStart, buffer.triangleList.Array, 0, triangleCount);
            buffer.triangleList.Count = triangleCount;
        }

    }

}