using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.Common
{
    using System.Collections;

    internal class ThinList<T>
    {
        internal readonly T[] array;

        private int length = 0;

        public ThinList(int capacity)
        {
            this.array = new T[capacity];
        }

        public void Add(T item)
        {
            this.array[this.length] = item;
            this.length++;
        }

        public void Clear()
        {
            this.length = 0;
        }

        public int Length => this.length;

        public static implicit operator T[](ThinList<T> list)
        {
            return list.array;
        }

        public T this[int index]
        {
            get
            {
                return this.array[index];
            }
        }

        public ICollection<T> AsCollection() => new ThinListCollection<T>(this);

    }

    internal struct ThinListCollection<T> : ICollection<T>
    {
        private readonly ThinList<T> thinList;

        public ThinListCollection(ThinList<T> thinList)
        {
            this.thinList = thinList;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ThinListEnumerator<T>(thinList);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

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
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this.thinList.array, arrayIndex, array, 0, this.Count - arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Count => this.thinList.Length;

        public bool IsReadOnly => true;
    }

    internal struct ThinListEnumerator<T> : IEnumerator<T>
    {
        private readonly ThinList<T> thinList;

        private int position;

        private int length;

        private T[] array;

        public ThinListEnumerator(ThinList<T> thinList)
        {
            this.thinList = thinList;
            this.position = -1;
            this.length = thinList.Length;
            this.array = thinList.array;
        }

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

        public T Current => this.array[this.position];

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }
    }
}
