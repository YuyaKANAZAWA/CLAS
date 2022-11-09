namespace GrToolBox.Data
{
    public class DataUtilities
    {
        /// <summary>
        /// バイト配列の指定位置（ビット）から指定ビット数読み取ってuint型で返す（Big-Endian, most-significant byte transmitted first）
        /// </summary>
        /// <param name="bytes">バイト配列</param>
        /// <param name="from">開始位置（ビット）</param>
        /// <param name="nBits">読み取りビット数</param>
        /// <returns></returns>
        public static uint BitToUint(byte[] bytes, int from, int nBits)
        {
            uint data = 0;
            for (int i = from; i < from + nBits; i++)
            {
                data = (uint)((data << 1) + (uint)((bytes[i / 8] >> (7 - i % 8)) & 0x01));    // %8 の剰余演算をビット演算置き換え検討
                //data = (uint)((data<<1) + ((bytes[i/8]>>(i%8))&0x01));
            }
            return data;
        }



        /// <summary>
        /// バイト配列の指定位置（ビット）から指定ビット数読み取ってuint型で返す（Little-Endian, lest-significant byte transmitted first）
        /// </summary>
        /// <param name="bytes">バイト配列</param>
        /// <param name="from">開始位置（ビット）</param>
        /// <param name="nBits">読み取りビット数</param>
        /// <returns></returns>
        public static uint BitToUint_L(byte[] bytes, int from, int nBits)
        {
            uint data = 0;
            //for (int i = from; i < from + nBits; i++)
            for (int i = from + nBits - 1; i >= from ; i--)
                {
                    data = (uint)((data << 1) + ((bytes[i / 8] >> (i % 8)) & 0x01));    // %8 の剰余演算をビット演算置き換え検討
                // data = (data<<1) + ((bytes[i/8]>>(i%8))&0x01);
            }
            return data;
        }


        /// <summary>
        /// バイト配列の指定位置（ビット）を読み取ってbool型で返す
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static bool BitToBool(byte[] bytes, int from)
        {
            bool data = false;
            data = (((bytes[from / 8] >> (7 - from % 8)) & 0x01)) > 0;    // %8 の剰余演算をビット演算置き換え検討
            return data;
        }

        /// <summary>
        /// バイト配列の指定位置（ビット）から指定ビット数読み取ってint型で返す（Big-Endian, most-significant byte transmitted first）
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="from"></param>
        /// <param name="nBits"></param>
        /// <returns></returns>
        public static int BitToInt(byte[] bytes, int from, int nBits)
        {
            uint data = BitToUint(bytes, from, nBits);
            if((data & (1u << (nBits - 1))) == 0)
            {   //最上位ビットが1でないなら
                return (int)data;
            }
            else
            {
                return (int)(data | (~0u << (nBits % 32 - 1)));
            }
        }

        /// <summary>
        /// バイト配列の指定位置（ビット）から指定ビット数読み取ってint型で返す（Little-Endian, lest-significant byte transmitted first）
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="from"></param>
        /// <param name="nBits"></param>
        /// <returns></returns>
        public static int BitToInt_L(byte[] bytes, int from, int nBits)
        {
            uint data = BitToUint_L(bytes, from, nBits);
            if ((data & (1u << (nBits - 1))) == 0)
            {   //最上位ビットが1でないなら
                return (int)data;
            }
            else
            {
                return (int)(data | (~0u << (nBits % 32 - 1)));
            }
        }


        /// <summary>
        /// バイト配列の指定位置（ビット）から指定ビット数のintS型（RTCM）を読み取ってint型で返す（Big-Endian, most-significant byte transmitted first）
        /// </summary>
        /// <param name="bytes">データバイト配列</param>
        /// <param name="from">読み取り開始ビット位置（このビットが符号ビットとなる）</param>
        /// <param name="nBits">読み取りビット数（nBits-1）ビットがデータ部</param>
        /// <returns></returns>
        public static int BitToIntS(byte[] bytes, int from, int nBits)
        {
            int sign = BitToBool(bytes, from)? -1 : 1;
            int data = (int)BitToUint(bytes, from + 1, nBits - 1);
            return sign * data;
        }


        /// <summary>
        /// データが与えられた範囲内[min,max]にある場合True．minとmaxは範囲に含む．
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="x">データ</param>
        /// <param name="min">範囲（小さい方）</param>
        /// <param name="max">範囲（大きい方）</param>
        /// <returns></returns>
        public static bool CheckData<T>(T x, T min, T max) where T : IComparable
        {
            return !(x.CompareTo(min) < 0 || x.CompareTo(max) > 0);
        }


    }

    public class GrBitArray
    {
        /// <summary>
        /// 格納されているビット数
        /// </summary>
        public int NBits { get; set; } = 0;
        private List<byte> Bytes { get; set; } = new List<byte>();


        public GrBitArray() { }

        public void AddByteArray(byte[] bytes)
        {
            AddByteArray(bytes, 0, bytes.Length * 8);
        }

        /// <summary>
        /// バイト配列の指定ビット位置から，指定ビット数のデータをバイト配列としてストアする．最終要素の8ビットに満たない部分はゼロ埋めされる．
        /// 既に格納されたデータがあり，その最終要素が8ビットに満たない場合は，新たに加えられたデータがビット単位で最終要素に結合される．
        /// 格納されているビット数は，プロパティNBitsに記録される．
        /// Ex. 0b_1111_1100 が格納(NBits=6)されていて，0b_1100_1100 がfrom:0, nBits:4 でAddされる場合の結果は，0b_1111_1111_0000 (NBits=10)
        /// </summary>
        /// <param name="bytes">バイト配列</param>
        /// <param name="from">bytes内の読み出しビット位置（インデックス）</param>
        /// <param name="nBits">読み出しビット数</param>
        public void AddByteArray(byte[] bytes, int from, int nBits)
        {
            int nBitsInLast = NBits % 8;
            if (nBitsInLast != 0)
            {
                int nBitsToLast = 8 - nBitsInLast;
                nBitsToLast = (nBits < nBitsToLast)? nBits : nBitsToLast;
                var tmpbyte = ExtractByteArray(bytes, from, nBitsToLast);
                Bytes[Bytes.Count - 1] |= (byte)(tmpbyte[0] >> nBitsInLast);
                NBits += nBitsToLast;
                from += nBitsToLast;
                nBits -= nBitsToLast;
            }

            if(nBits > 0)
            {
                var extractBytes = ExtractByteArray(bytes, from, nBits);
                Bytes.AddRange(extractBytes);
                NBits += nBits;

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

        public byte[] GetByteArray()
        {
            return Bytes.ToArray();
        }

        public void Clear()
        {
            Bytes.Clear();
        }
    }
}
