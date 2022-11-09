using static GrToolBox.Data.DataUtilities;

namespace GrToolBox.Data
{
    public class BitCircularBuffer
    {
        private int Size { get; set; }
        private byte[] Buffer { get; set; }
        private bool IsFull { get; set; } = false;
        private bool TempIsFull { get; set; } = false;
        private int Mask { get; set; }

        private Position Top { get; set; } = new Position();
        private Position TempTop { get; set; } = new Position();
        private Position Bottom { get; set; } = new Position();
        private Position TempBottom { get; set; } = new Position();

        private int ThisCount { get; set; } = 0;
        private int TempCount { get; set; } = 0;

        /// <summary>
        /// バッファの長さは2のべき乗に限る：2^(size) bytes
        /// </summary>
        /// <param name="size"></param>
        public BitCircularBuffer(int size)
        {
            Size = 0x01 << size;
            Buffer = new byte[Size];
            Mask = Size - 1;
        }

        public int Count()
        {
            return ThisCount;
        }

        //public int Count()
        //{
        //    if (IsFull) return Size * 8;
        //    int count = Bottom.IByte - Top.IByte;
        //    if (count == 0)
        //    {
        //        if (Top.IBit == Bottom.IBit)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            return Bottom.IBit - Top.IBit;
        //        }
        //    }
        //    else if(count < 0)
        //    {
        //        count = count + Buffer.Length - 1;
        //    }
        //    else
        //    {
        //        count = count - 1;
        //    }
        //    return count * 8 + (8 - Top.IBit) + Bottom.IBit;
        //}


        /// <summary>
        /// 書き込み可能ビット数
        /// </summary>
        /// <returns>書き込み可能ビット数</returns>
        public int NumWritable()
        {
            //return IsFull ? 0 : Size * 8 - Count();
            return IsFull ? 0 : Size * 8 - ThisCount;
        }


        public void AddByteArray(byte[] bytes)
        {
            if (bytes == null) return;
            int from = 0;
            int nBits = 8 * bytes.Length;
            AddByteArray(bytes, from, nBits);
        }


        public void AddByteArray(byte[] bytes, int from, int nBits)
        {
            if (nBits <= 0) return;
            
            ThisCount += nBits;

            int nBitsInLast = Bottom.IBit;
            int nBitsToLast = 8 - nBitsInLast;
            nBitsToLast = (nBits < nBitsToLast) ? nBits : nBitsToLast;
            var tmpbyte = ExtractByteArray(bytes, from, nBitsToLast);
            Buffer[Bottom.IByte] |= (byte)(tmpbyte[0] >> nBitsInLast);
            //Bottom.IBit += nBitsToLast;
            Bottom.Proceed(nBitsToLast, Size);
            from += nBitsToLast;
            nBits -= nBitsToLast;

            var extractBytes = ExtractByteArray(bytes, from, nBits);

            int iByte = Bottom.IByte;
            foreach(byte b in extractBytes)
            {
                Buffer[iByte] = b;
                iByte++;
                if(iByte == Size)
                {
                    iByte = 0;
                }
            }
            Bottom.Proceed(nBits, Size);

            if (IsFull)
            {
                Top.IBit = Bottom.IBit;
                Top.IByte = Bottom.IByte;
                TempTop.IBit = Top.IBit;
                TempTop.IByte = Top.IByte;
            }
            else
            {
                if (Bottom.IByte == Top.IByte && Bottom.IBit == Top.IBit)
                {
                    IsFull = true;
                }
            }           
        }


        private byte[] ExtractByteArray(byte[] bytes, int from, int nBits)
        {
            int nShift = from % 8;
            int indStart = from / 8;
            int indENd = indStart + (from + nBits - 1) / 8;

            int nBytes = (nBits / 8) + (((nBits % 8) > 0) ? 1 : 0);

            int nBitsInLast = nBits % 8;
            nBitsInLast = (nBitsInLast != 0) ? nBitsInLast : 8;

            byte[] shiftedBytes = new byte[nBytes];

            for (int i = 0; i < nBytes; i++)
            {
                int j = indStart + i;
                if (nShift == 0)
                {
                    shiftedBytes[i] = bytes[j];
                }
                else
                {
                    shiftedBytes[i] = (byte)(bytes[j] << nShift);
                    if (j + 1 <= indENd)
                    {
                        shiftedBytes[i] |= (byte)(bytes[j + 1] >> (8 - nShift));
                    }
                    else
                    {
                        shiftedBytes[i] &= (byte)(0b_11111111 << (8 - nBitsInLast));
                    }
                }
            }
            return shiftedBytes;
        }


        public bool GetBit()
        {
            if (ThisCount == 0)
            {
                throw new IndexOutOfRangeException();
            }
            bool ret = ((Buffer[Top.IByte] >> (7 - Top.IBit)) & 0x_01) > 0;
            Top.Proceed(1, Size);

            ThisCount--;

            return ret;
        }

        /// <summary>
        /// <para>BitCircularBufferの現在位置から指定ビット数のデータを取り出し，byte配列で返す．8bitに満たないbyteは，左詰でゼロ埋めされる</para>
        /// <para>Ex. {Buffer[0]=0b_1100_0011, Buffer[1]=0b_1110_1010}と格納されているとき，6ビット取り出すと，0b_1100_0000 が返る</para>
        /// </summary>
        /// <param name="nbits">取り出しbit数</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public byte[] GetByteArray(int nbits)
        {
            if (ThisCount < nbits)
            {
                throw new IndexOutOfRangeException();
            }
            byte[] bytes = new byte[nbits / 8 + 1];

            int i_bit = 0;
            int i_byte = 0;
            for(int i = 0; i < nbits; i++)
            {
                if(i_bit == 8)
                {
                    i_bit = 0;
                    i_byte++;
                }
                if (GetBit())
                {
                    bytes[i_byte] |= (byte)(0x_01 << (7 - i_bit));
                }
                i_bit++;
            }
            return bytes;
        }


        public uint GetUint(int nbits)
        {
            if (ThisCount < nbits)
            {
                throw new IndexOutOfRangeException();
            }
            byte[] bytes = GetByteArray(nbits);
            return BitToUint(bytes, 0, nbits);
        }

        public int GetInt(int nbits)
        {
            if (ThisCount < nbits)
            {
                throw new IndexOutOfRangeException();
            }
            byte[] bytes = GetByteArray(nbits);
            return BitToInt(bytes, 0, nbits);
        }

        /// <summary>
        /// BitCircularBufferの先頭読み出し位置を指定ビット数進める
        /// </summary>
        /// <param name="nbits">進めるビット数</param>
        public void ProceedBit(int nbits)
        {
            Top.Proceed(nbits, Size);
        }

        public void SetMark()
        {
            TempTop.CopyFrom(Top);
            TempBottom.CopyFrom(Bottom);
            TempIsFull = IsFull;

            TempCount = ThisCount;
        }

        public void BackToMark()
        {
            Top.CopyFrom(TempTop);
            Bottom.CopyFrom(TempBottom);
            IsFull = TempIsFull;

            ThisCount = TempCount;
        }

        private int NextIndex(int ix)
        {
            //return ++ix % Size;
            return ++ix & Mask;     // 2のべき乗での剰余は，1減じた数とのANDで計算可能
                                    // ex.　5%4=1 -->> (101) & (011) = 001
        }

        public void Clear()
        {
            Top.IByte = Bottom.IByte = 0;
            Top.IBit = Bottom.IBit = 0;
            IsFull = false;

            ThisCount = 0;
        }

        /// <summary>
        /// For Debug
        /// </summary>
        public void CheckDisp()
        {
            Console.WriteLine("writable: " + NumWritable() + "  IsFull: " + IsFull + "  Count: " + Count() + "  Top: " + Top + "  Bottom: " + Bottom + "  Buffer: " + string.Join(", ", Buffer));
        }

        private class Position
        {
            public int IByte { get; set; }
            public int IBit { get; set; }

            /// <summary>
            /// Indexを指定ビット数進める
            /// </summary>
            /// <param name="nbits">進めるbit数</param>
            /// <param name="size">circular bufferのサイズ（バイト数）</param>
            public void Proceed(int nbits, int size)
            {
                int mask = size - 1;
                for(int i = 0; i < nbits; i++)
                {
                    IBit++;
                    if(IBit == 8)
                    {
                        IBit = 0;
                        IByte++;
                        IByte &= mask;
                    }
                }
            }

            public void CopyFrom(Position p)
            {
                IByte = p.IByte;
                IBit = p.IBit;
            }
        }

    }
}
