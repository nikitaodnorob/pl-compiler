using System;
using System.Linq;

namespace MyCompilerLibrary
{
    public class Array<T>
    {
        private readonly T[] arr;

        public Array(int length)
        {
            if (length < 0) throw new ArgumentException("Length can not be negative!");
            arr = new T[length];
        }

        public Array(T[] array)
        {
            arr = array.ToArray();
        }

        public Array(Array<T> other)
        {
            arr = other.arr.ToArray();
        }

        public Array() { }

        public T this[int i]
        {
            get
            {
                if (i < 0) throw new ArgumentException("Index can not be negative!");
                return arr[i];
            }
            set
            {
                if (i < 0) throw new ArgumentException("Index can not be negative!");
                arr[i] = value;
            }
        }

        public int length
        {
            get => arr.Length;
        }

        public static implicit operator Array<T>(T[] array) => new Array<T>(array);

        public Array<int> indices
        {
            get
            {
                Array<int> res = new Array<int>(length);
                for (int i = 0; i < length; i++) res[i] = i;
                return res;
            }
        }

        public override string ToString()
        {
            string res = "[";
            for (int i = 0; i < length; i++)
            {
                res += arr[i].ToString();
                if (i != length - 1) res += ",";
            }
            return res + "]";
        }

        public string toString() => ToString();

        public void swapByIndex(int i, int j)
        {
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}
