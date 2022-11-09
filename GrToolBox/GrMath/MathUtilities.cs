using System.Collections;

namespace GrToolBox.GrMath
{
    public static class MathUtilities
    {
        /// <summary>
        /// ベクトルの外積の計算
        /// </summary>
        /// <param name="a"> 3次元ベクトルA </param>
        /// <param name="b"> 3次元ベクトルB </param>
        /// <returns> cr = A X B </returns>
        public static double[] Cross(double[] a, double[] b)
        {
            double[] cr = new double[3];
            cr[0] = a[1] * b[2] - a[2] * b[1];
            cr[1] = a[2] * b[0] - a[0] * b[2];
            cr[2] = a[0] * b[1] - a[1] * b[0];
            return cr;
        }


        /// <summary>
        /// 2次元配列からbitsがtrueの列をもつ配列を抽出
        /// </summary>
        /// <param name="A"></param>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static double[][] DelCols(double[][] A, BitArray bits)
        {
            int nCol = 0;
            foreach (bool b in bits)
            {
                if (b) nCol++;
            }

            double[][] B = new double[A.Length][];
            for (int i = 0; i < B.Length; i++)
            {
                B[i] = new double[nCol];
                int k = 0;
                for (int j = 0; j < bits.Length; j++)
                {
                    if (bits.Get(j))
                    {
                        B[i][k] = A[i][j];
                        k++;
                    }
                }
            }
            return B;
        }





    }














}
