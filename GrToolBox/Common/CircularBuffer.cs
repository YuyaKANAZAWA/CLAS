using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GrToolBox.Common
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private int Size { get; set; }
        private T[] Buffer { get; set; }
        private bool IsFull { get; set; } = false;
        private bool TempIsFull { get; set; } = false;
        private int Top { get; set; } = 0;
        private int TempTop { get; set; } = 0;
        private int Bottom { get; set; } = 0;
        private int TempBottom { get; set; } = 0;
        private int Mask { get; set; }

        /// <summary>
        /// バッファの長さは2のべき乗に限る：2^(size)
        /// </summary>
        /// <param name="size"></param>
        public CircularBuffer(int size)
        {
            Size = 0x01 << size;
            Buffer = new T[Size];
            Mask = Size - 1;
        }

        public int Count()
        {
            if (IsFull) return Size;
            int count = Bottom - Top;
            if (count < 0) count += Buffer.Length;
            return count;
        }

        public int NumWritable()
        {
            return IsFull ? 0 : Size - Count();
            //return Size - Count();
        }

        public void Add(T data)
        {
            Buffer[Bottom] = data;
            Bottom = NextIndex(Bottom);
            if (IsFull)
            {
                Top = Bottom;
                TempTop = Top;
            }
            else
            {
                if(Bottom == Top)
                {
                    IsFull = true;
                }
            }
        }

        public void Add(T[] dataArray)
        {
            foreach(T data in dataArray)
            {
                Add(data);
            }
        }

        public T Read()
        {
            T data = Buffer[Top];
            Top = NextIndex(Top);
            if (IsFull) IsFull = false;
            return data;
        }

        public T[] Read(int len)
        {
            int cnt = Count();
            if (len > cnt) len = cnt;
            T[] dataArray = new T[len];
            for(int i = 0; i < len; i++)
            {
                dataArray[i] = Read();
            }
            return dataArray;
        }

        public void SetMark()
        {
            TempTop = Top;
            TempBottom = Bottom;
            TempIsFull = IsFull;
        }

        public void BackToMark()
        {
            Top = TempTop;
            Bottom = TempBottom;
            IsFull = TempIsFull;
        }


        private int NextIndex(int ix)
        {
            //return ++ix % Size;
            return ++ix & Mask;     // 2のべき乗での剰余は，1減じた数とのANDで計算可能
                                    // ex.　5%4=1 -->> (101) & (011) = 001
        }


        public void Clear()
        {
            Top = Bottom = 0;
            IsFull= false;
        }


        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// For Debug
        /// </summary>
        public void CheckDisp()
        {
            Console.WriteLine("writable: " + NumWritable() + "  IsFull: " + IsFull + "  Count: " + Count() + "  Top: " + Top + "  Bottom: " + Bottom + "  Buffer: " + string.Join(", ", Buffer));
        }
    }
}
