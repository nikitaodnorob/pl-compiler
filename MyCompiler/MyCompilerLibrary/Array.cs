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

        #region Deconstruct methods
        public void Deconstruct(out T el1)
        {
            if (length != 1) throw new Exception("Размерности массива и кортежа не совпадают");
            el1 = arr[0];
        }
        public void Deconstruct(out T el1, out T el2)
        {
            if (length != 2) throw new Exception("Размерности массива и кортежа не совпадают");
            el1 = arr[0]; el2 = arr[1];
        }
        public void Deconstruct(out T el1, out T el2, out T el3)
        {
            if (length != 3) throw new Exception("Размерности массива и кортежа не совпадают");
            el1 = arr[0]; el2 = arr[1]; el3 = arr[2];
        }
        public void Deconstruct(out T el1, out T el2, out T el3, out T el4)
        {
            if (length != 4) throw new Exception("Размерности массива и кортежа не совпадают");
            el1 = arr[0]; el2 = arr[1]; el3 = arr[2]; el4 = arr[3];
        }
        public void Deconstruct(out T el1, out T el2, out T el3, out T el4, out T el5)
        {
            if (length != 5) throw new Exception("Размерности массива и кортежа не совпадают");
            el1 = arr[0]; el2 = arr[1]; el3 = arr[2]; el4 = arr[3]; el5 = arr[4];
        }
        public void Deconstruct(out T el1, out T el2, out T el3, out T el4, out T el5, out T el6)
        {
            if (length != 6) throw new Exception("Размерности массива и кортежа не совпадают");
            el1 = arr[0]; el2 = arr[1]; el3 = arr[2]; el4 = arr[3]; el5 = arr[4]; el6 = arr[5];
        }
        public void Deconstruct(out T el1, out T el2, out T el3, out T el4, out T el5, out T el6, out T el7)
        {
            if (length != 7) throw new Exception("Размерности массива и кортежа не совпадают");
            el1 = arr[0]; el2 = arr[1]; el3 = arr[2]; el4 = arr[3]; el5 = arr[4]; el6 = arr[5]; el7 = arr[6];
        }
        public void Deconstruct(out T el1, out T el2, out T el3, out T el4, out T el5, out T el6, out T el7, out T el8)
        {
            if (length != 8) throw new Exception("Размерности массива и кортежа не совпадают");
            el1 = arr[0]; el2 = arr[1]; el3 = arr[2]; el4 = arr[3]; el5 = arr[4]; el6 = arr[5]; el7 = arr[6]; el8 = arr[7];
        }
        public void Deconstruct(out T el1, out T el2, out T el3, out T el4, out T el5, out T el6, out T el7, out T el8, out T el9)
        {
            if (length != 9) throw new Exception("Размерности массива и кортежа не совпадают");
            el1 = arr[0]; el2 = arr[1]; el3 = arr[2]; el4 = arr[3]; el5 = arr[4]; el6 = arr[5]; el7 = arr[6]; el8 = arr[7]; el9 = arr[8];
        }
        #endregion
    }
}
