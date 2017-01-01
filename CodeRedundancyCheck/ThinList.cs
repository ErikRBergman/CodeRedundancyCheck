namespace CodeRedundancyCheck.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class ThinList<T>
    {
        internal T[] array;

        public int length = 0;

        public ThinList(int capacity)
        {
            this.array = new T[capacity];
        }

        public int Capacity => this.array.Length;

        public int Length => this.length;

        public static implicit operator T[](ThinList<T> list)
        {
            return list.array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            this.array[this.length] = item;
            this.length++;
        }

        public ThinListCollection<T> AsCollection() => new ThinListCollection<T>(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            this.length = 0;
        }

        public bool Contains(T item)
        {
            return this.array.Contains(item);
        }

        public T[] Detach()
        {
            var detachedArray = this.array;
            this.array = null;
            return detachedArray;
        }

        public T[] GetArrayFromCurrentSize()
        {
            var newArray = new T[this.length];
            Array.Copy(this.array, newArray, this.length);
            return newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Item(int index) => this.array[index];


        public void Resize(int newCapacity)
        {
            var newArray = new T[newCapacity];
            Array.Resize(ref this.array, newCapacity);
        }
    }

    public struct ThinListCollection<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private readonly ThinList<T> thinList;

        public ThinListCollection(ThinList<T> thinList)
        {
            this.thinList = thinList;
        }

        public int Count => this.thinList.Length;

        public bool IsReadOnly => true;

        public void Add(T item)
        {
            this.thinList.Add(item);
        }

        public void Clear()
        {
            this.thinList.Clear();
        }

        public bool Contains(T item)
        {
            return this.thinList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this.thinList.array, arrayIndex, array, 0, this.Count - arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ThinListEnumerator<T>(this.thinList);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal struct ThinListEnumerator<T> : IEnumerator<T>
        {
            private readonly ThinList<T> thinList;

            private T[] array;

            private int length;

            private int position;

            public ThinListEnumerator(ThinList<T> thinList)
            {
                this.thinList = thinList;
                this.position = -1;
                this.length = thinList.Length;
                this.array = thinList.array;
            }

            public T Current => this.array[this.position];

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                this.position++;
                return this.position < this.length;
            }

            public void Reset()
            {
                this.position = -1;
            }
        }
    }
}