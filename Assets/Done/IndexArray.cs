using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class IndexArray<T>
    {
        public T[] array;
        public int index;
        public IndexArray(T[] array)
        {
            this.array = array;
            this.index = 0;// array.Length;
        }
        public IndexArray(T[] array, int index)
        {
            this.array = array;
            this.index = index;
        }
        public IndexArray(int size, int index = 0)
        {
            this.array = new T[size];
            this.index = index;
        }
        public void AddItem(T item)
        {
            if (index >= array.Length) Array.Resize<T>(ref array, array.Length * 2);
            array[index++] = item;
        }
    }
}
