using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Vertigo {

    public class StructList<T> where T : struct {

        public T[] array;
        public int size;

        public T[] Array => array;

        public StructList(int capacity = 8) {
            this.size = 0;
            this.array = new T[capacity];
        }

        public int Count {
            get { return size; }
            set { size = value; }
        }

        public void Add(in T item) {
            if (size + 1 > array.Length) {
                System.Array.Resize(ref array, (size + 1) * 2);
            }

            array[size] = item;
            size++;
        }

        public void AddUnsafe(in T item) {
            array[size++] = item;
        }

        public void AddRange(T[] collection) {
            if (size + collection.Length >= array.Length) {
                System.Array.Resize(ref array, size + collection.Length * 2);
            }

            System.Array.Copy(collection, 0, array, size, collection.Length);
            size += collection.Length;
        }

        public void AddRange(T[] collection, int start, int count) {
            if (size + count >= array.Length) {
                System.Array.Resize(ref array, size + count * 2);
            }

            System.Array.Copy(collection, start, array, size, count);
            size += count;
        }

        public void AddRange(StructList<T> collection) {
            if (size + collection.size >= array.Length) {
                System.Array.Resize(ref array, size + collection.size * 2);
            }

            System.Array.Copy(collection.array, 0, array, size, collection.size);
            size += collection.size;
        }

        public void AddRange(StructList<T> collection, int start, int count) {
            if (size + collection.size >= array.Length) {
                System.Array.Resize(ref array, size + count * 2);
            }

            System.Array.Copy(collection.array, start, array, size, count);
            size += count;
        }

        public void EnsureCapacity(int capacity) {
            if (array.Length < capacity) {
                System.Array.Resize(ref array, capacity * 2);
            }
        }

        public void EnsureAdditionalCapacity(int capacity) {
            if (array.Length < size + capacity) {
                System.Array.Resize(ref array, (size + capacity) * 2);
            }
        }

        public void QuickClear() {
            size = 0;
        }

        public void Clear() {
            size = 0;
            System.Array.Clear(array, 0, array.Length);
        }

        public T this[int idx] {
            get => array[idx];
            set => array[idx] = value;
        }

        public void SetFromRange(T[] source, int start, int count) {
            if (array.Length <= count) {
                System.Array.Resize(ref array, count * 2);
            }

            System.Array.Copy(source, start, array, 0, count);
            size = count;
        }

        private static readonly LightList<StructList<T>> s_Pool = new LightList<StructList<T>>();

        public static StructList<T> Get() {
            if (s_Pool.Count > 0) {
                return s_Pool.RemoveLast();
            }

            return new StructList<T>();
        }

        public static void Release(ref StructList<T> toPool) {
            toPool.Clear();
            s_Pool.Add(toPool);
        }

    }

    [DebuggerDisplay("Count = {" + nameof(size) + "}")]
    public class LightList<T> : IReadOnlyList<T>, IList<T> {

        private int size;
        private T[] array;

        [DebuggerStepThrough]
        public LightList(int capacity = 8) {
            this.array = new T[capacity];
            this.size = 0;
        }

        [DebuggerStepThrough]
        public LightList(T[] items) {
            this.array = items;
            this.size = items.Length;
        }

        [DebuggerStepThrough]
        public LightList(IList<T> items) {
            this.array = new T[items.Count];
            this.size = items.Count;
            for (int i = 0; i < items.Count; i++) {
                array[i] = items[i];
            }
        }

        public T[] Array => array;

        public int Count {
            [DebuggerStepThrough] get => size;
            [DebuggerStepThrough] set => size = value;
        }

        public bool IsReadOnly => false;

        public int Capacity => array.Length;

        public T First {
            [DebuggerStepThrough] get { return array[0]; }
            [DebuggerStepThrough] set { array[0] = value; }
        }

        public T Last {
            [DebuggerStepThrough] get { return array[size - 1]; }
            [DebuggerStepThrough] set { array[size - 1] = value; }
        }

        [DebuggerStepThrough]
        public void Add(T item) {
            if (size + 1 > array.Length) {
                System.Array.Resize(ref array, (size + 1) * 2);
            }

            array[size] = item;
            size++;
        }

        [DebuggerStepThrough]
        public void AddRange(IEnumerable<T> collection) {
            if (collection == null || Equals(collection, this)) {
                return;
            }

            if (collection is LightList<T> list) {
                EnsureAdditionalCapacity(list.size);
                System.Array.Copy(list.array, 0, array, size, list.size);
                size += list.size;
                return;
            }
            else if (collection is T[] cArray) {
                EnsureAdditionalCapacity(cArray.Length);
                System.Array.Copy(cArray, 0, array, size, cArray.Length);
                size += cArray.Length;
                return;
            }

            foreach (var item in collection) {
                Add(item);
            }
        }

        public void AddUnchecked(T item) {
            array[size++] = item;
        }

        public void QuickClear() {
            System.Array.Clear(array, 0, size);
            size = 0;
        }

        public void Clear() {
            System.Array.Clear(array, 0, array.Length);
            size = 0;
        }

        public void ResetSize() {
            size = 0;
        }

        // todo -- remove boxing
        public bool Contains(T item) {
            for (int i = 0; i < size; i++) {
                if (array[i].Equals(item)) return true;
            }

            return false;
        }

        public List<T> ToList(List<T> list = null) {
            list = list ?? new List<T>();
            // can't use AddRange because our array is oversized
            for (int i = 0; i < Count; i++) {
                list.Add(array[i]);
            }

            return list;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            for (int i = 0; i < size; i++) {
                array[arrayIndex + i] = this.array[i];
            }
        }

        public void Remove<U>(U closureArg, Func<T, U, bool> fn) {
            int idx = FindIndex(closureArg, fn);
            if (idx != -1) {
                RemoveAt(idx);
            }
        }

        // todo -- remove boxing
        public bool Remove(T item) {
            for (int i = 0; i < size; i++) {
                if (array[i].Equals(item)) {
                    for (int j = i; j < size - 1; j++) {
                        array[j] = array[j + 1];
                    }

                    array[size - 1] = default(T);
                    size--;
                    return true;
                }
            }

            return false;
        }

        // todo -- remove boxing
        public int IndexOf(T item) {
            for (int i = 0; i < size; i++) {
                if (array[i].Equals(item)) return i;
            }

            return -1;
        }

        public void Insert(int index, T item) {
            if (size + 1 >= array.Length) {
                System.Array.Resize(ref array, (size + 1) * 2);
            }

            size++;
            if (index < 0 || index > array.Length) {
                throw new IndexOutOfRangeException();
            }

            System.Array.Copy(array, index, array, index + 1, size - index);
            array[index] = item;
        }

        public void Reverse() {
            System.Array.Reverse(array, 0, size);
        }

        public void InsertRange(int index, IEnumerable<T> collection) {
            if (collection == null) {
                return;
            }

            if ((uint) index > (uint) size) {
                throw new IndexOutOfRangeException();
            }

            if (collection is ICollection<T> objs) {
                int count = objs.Count;
                if (count > 0) {
                    this.EnsureCapacity(size + count);

                    if (index < size) {
                        System.Array.Copy(array, index, array, index + count, size - index);
                    }

                    if (Equals(this, objs)) {
                        System.Array.Copy(array, 0, array, index, index);
                        System.Array.Copy(array, index + count, array, index * 2, size - index);
                    }
                    else {
                        if (objs is LightList<T> list) {
                            System.Array.Copy(list.Array, 0, array, index, list.size);
                        }
                        else if (objs is T[] b) {
                            System.Array.Copy(b, 0, array, index, b.Length);
                        }
                        else {
                            T[] a = new T[count];
                            objs.CopyTo(a, 0);
                            a.CopyTo(array, index);
                        }
                    }

                    size += count;
                }
            }
            else {
                foreach (T obj in collection) {
                    this.Insert(index++, obj);
                }
            }
        }

        public void ShiftRight(int startIndex, int count) {
            if (count <= 0) return;
            if (startIndex < 0) startIndex = 0;
            EnsureCapacity(startIndex + count + count); // I think this is too big
            System.Array.Copy(array, startIndex, array, startIndex + count, count);
            System.Array.Clear(array, startIndex, count);
            size += count;
        }

        public void ShiftLeft(int startIndex, int count) {
            if (count <= 0) return;
            if (startIndex < 0) startIndex = 0;
            System.Array.Copy(array, startIndex, array, startIndex - count, size - startIndex);
            System.Array.Clear(array, size - count, count);
            size -= count;
        }

        public T RemoveLast() {
            T retn = array[size - 1];
            array[size - 1] = default;
            size--;
            return retn;
        }

        public void RemoveAt(int index) {
            if ((uint) index >= (uint) size) return;
            if (index == size - 1) {
                array[--size] = default;
            }
            else {
                for (int j = index; j < size - 1; j++) {
                    array[j] = array[j + 1];
                }

                array[--size] = default(T);
            }
        }

        public int FindIndex(Predicate<T> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(array[i])) {
                    return i;
                }
            }

            return -1;
        }

        public bool Contains<U>(U closureArg, Func<T, U, bool> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(array[i], closureArg)) {
                    return true;
                }
            }

            return false;
        }

        public int FindIndex<U>(U closureArg, Func<T, U, bool> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(array[i], closureArg)) {
                    return i;
                }
            }

            return -1;
        }

        public T Find(Predicate<T> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(array[i])) {
                    return array[i];
                }
            }

            return default(T);
        }

        public T Find<U>(U closureArg, Func<T, U, bool> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(array[i], closureArg)) {
                    return array[i];
                }
            }

            return default(T);
        }

        public T this[int index] {
            [DebuggerStepThrough] get { return array[index]; }
            [DebuggerStepThrough] set { array[index] = value; }
        }

        public void EnsureCapacity(int capacity) {
            if (array.Length < capacity) {
                System.Array.Resize(ref array, capacity * 2);
            }
        }

        public void EnsureAdditionalCapacity(int capacity) {
            if (array.Length < size + capacity) {
                System.Array.Resize(ref array, (size + capacity) * 2);
            }
        }

        public void Sort(int start, int end, IComparer<T> comparison) {
            System.Array.Sort(array, start, end, comparison);
        }

        public void Sort(int start, int end, Comparison<T> comparison) {
            s_Compare.comparison = comparison;
            System.Array.Sort(array, start, end, s_Compare);
            s_Compare.comparison = null;
        }

        public void Sort(Comparison<T> comparison) {
            s_Compare.comparison = comparison;
            System.Array.Sort(array, 0, size, s_Compare);
            s_Compare.comparison = null;
        }

        public void Sort(IComparer<T> comparison) {
            System.Array.Sort(array, 0, size, comparison);
        }

        public int BinarySearch(T value, IComparer<T> comparer) {
            return InternalBinarySearch(array, 0, size, value, comparer);
        }

        public int BinarySearch(T value) {
            return InternalBinarySearch(array, 0, size, value, Comparer<T>.Default);
        }

        private static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer) {
            int num1 = index;
            int num2 = index + length - 1;
            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);
                int num3 = comparer.Compare(array[index1], value);

                if (num3 == 0) {
                    return index1;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return ~num1;
        }

        private static readonly FunctorComparer s_Compare = new FunctorComparer();

        private sealed class FunctorComparer : IComparer<T> {

            public Comparison<T> comparison;

            public int Compare(T x, T y) {
                return this.comparison(x, y);
            }

        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }

        public class Enumerator : IEnumerator<T> {

            private int index;
            private T current;
            private readonly LightList<T> list;

            internal Enumerator(LightList<T> list) {
                this.list = list;
                this.index = 0;
                this.current = default(T);
            }

            public void Dispose() { }

            public bool MoveNext() {
                if ((uint) index >= (uint) list.size) {
                    index = list.size + 1;
                    current = default(T);
                    return false;
                }

                current = list.array[index];
                ++index;
                return true;
            }

            public T Current => current;

            object IEnumerator.Current => Current;

            void IEnumerator.Reset() {
                index = 0;
                current = default(T);
            }

        }

    }

}