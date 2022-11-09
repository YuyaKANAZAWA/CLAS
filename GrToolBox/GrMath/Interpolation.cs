namespace GrToolBox.GrMath
{
    public static class Interpolation
    {
        /// <summary>
        /// 1次元データの補間
        /// X-Yのデータのペアが与えられたとき，Xqの値に対するYqを内挿により求めます．Xデータは等間隔である必要があります．
        /// </summary>
        /// <param name="Xmin">Xデータの最小値</param>
        /// <param name="Xmax">Xデータの最大値</param>
        /// <param name="dX">Xデータの間隔</param>
        /// <param name="Y">Yデータ</param>
        /// <param name="Xq">求めたい点のX値</param>
        /// <returns>Xqに対するY値</returns>
        public static double Interp1(double Xmin, double Xmax, double dX, double[] Y, double Xq)
        {
            int nX = Y.Length;
            double XLeft = Xmin;
            double XRight = XLeft + dX;
            int iXLeft = 0;
            int iXRight = 0;
            for (int i = 0; i < nX - 1; i++)
            {
                if (XLeft <= Xq && Xq <= XRight)
                {
                    iXLeft = i;
                    iXRight = i + 1;
                    break;
                }
                XLeft += dX;
                XRight += dX;
            }
            double valLeft = Y[iXLeft];
            double valRight = Y[iXRight];

            double val = valLeft + (valRight - valLeft) / dX * (Xq - XLeft);

            return val;

        }

        /// <summary>
        /// 1次元データの補間
        /// X-Yのデータのペアが与えられたとき，Xqの値に対するYqを内挿により求めます．
        /// XとYの要素数は同じで有る必要があります．
        /// </summary>
        /// <param name="X">Xデータの配列</param>
        /// <param name="Y">Yデータの配列</param>
        /// <param name="Xq">Yデータを内挿したいX値</param>
        /// <returns>Xqに対するY値</returns>
        public static double Interp1(double[] X, double[] Y, double Xq)
        {
            int nX = Y.Length;
            double XLeft = 0.0;
            double XRight;
            double dX = 0.0;
            int iXLeft = 0;
            int iXRight = 0;
            for (int i = 0; i < nX - 1; i++)
            {
                XLeft = X[i];
                XRight = X[i + 1];
                dX = XRight - XLeft;
                if (XLeft <= Xq && Xq <= XRight)
                {
                    iXLeft = i;
                    iXRight = i + 1;
                    break;
                }
            }
            double valLeft = Y[iXLeft];
            double valRight = Y[iXRight];

            double val = valLeft + (valRight - valLeft) / dX * (Xq - XLeft);

            return val;

        }

        /// <summary>
        /// 2次元データの補間
        ///  --o--o--o--o--o--
        ///    |  |  |  |  |
        ///  --o--o--o--o--o--  
        ///    |  |  |  |  |
        ///  --o--o--o--o--o--
        ///  こんなグリッドデータを補完する
        ///  
        /// </summary>
        /// <param name="Xmin">横軸の最小値</param>
        /// <param name="Xmax">横軸の最大値</param>
        /// <param name="dX">横軸の間隔</param>
        /// <param name="Ymin">縦軸の最小値</param>
        /// <param name="Ymax">縦軸の最大値</param>
        /// <param name="dY">縦軸の間隔</param>
        /// <param name="Grid">グリッド点の値</param>
        /// <param name="Xq">求めたい点の横軸座標</param>
        /// <param name="Yq">求めたい点の縦軸座標</param>
        /// <returns>(Xq,Yq)を囲む4点のグリッドデータから補完して求めた値</returns>
        public static double Interp2(double Xmin, double Xmax, double dX,
                                     double Ymin, double Ymax, double dY,
                                     double[][] Grid, double Xq, double Yq)
        {
            // Xq, Yqを囲む4点を探す
            int nX = Grid[0].Length;
            double XLeft = Xmin;
            double XRight = XLeft + dX;
            int iXLeft = 0;
            int iXRight = 0;
            for (int i = 0; i < nX - 1; i++)
            {
                if (XLeft <= Xq && Xq <= XRight)
                {
                    iXLeft = i;
                    iXRight = i + 1;
                    break;
                }
                XLeft += dX;
                XRight += dX;
            }

            int nY = Grid.Length;
            double YLower = Ymin;
            double YUpper = YLower + dY;
            int iYLower = 0;
            int iYUpper = 0;
            for (int i = 0; i < nY - 1; i++)
            {
                if (YLower <= Yq && Yq <= YUpper)
                {
                    iYLower = i;
                    iYUpper = i + 1;
                    break;
                }
                YLower += dY;
                YUpper += dY;
            }
            double[][] GridPoint = { new double[2] {XLeft, YLower},
                                     new double[2] {XLeft, YUpper},
                                     new double[2] {XRight,YLower},
                                     new double[2] {XRight,YUpper} };
            double[] GridValue = { Grid[iYLower][iXLeft],       // Lower Left
                               Grid[iYUpper][iXLeft],           // Upper Left
                               Grid[iYLower][iXRight],          // Lower Right
                               Grid[iYUpper][iXRight] };        // Upper Right

            return Bilinear_Interp(GridPoint, GridValue, Xq, Yq);
        }

        /// <summary>
        /// 配列Xの値と配列Yの値のペアに対してGridで値が与えられているとき，(Xq,Yq)点における値を双一次補完により求めます．
        /// 配列Xの要素数は配列Gridの列数，Yの要素数はGridの行数に等しくなければなりません．
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Grid"></param>
        /// <param name="Xq"></param>
        /// <param name="Yq"></param>
        /// <returns></returns>
        public static double Interp2(double[] X, double[] Y, double[][] Grid, double Xq, double Yq)
        {
            // Xqを挟む配列Xの要素を探す
            int nX = Y.Length;
            double XLeft = 0.0;
            double XRight = 0.0;
            //double dX = 0.0;
            int iXLeft = 0;
            int iXRight = 0;
            for (int i = 0; i < nX - 1; i++)
            {
                XLeft = X[i];
                XRight = X[i + 1];
                //dX = XRight - XLeft;
                if (XLeft <= Xq && Xq <= XRight)
                {
                    iXLeft = i;
                    iXRight = i + 1;
                    break;
                }
            }
            // Yqを挟む配列Yの要素を探す
            int nY = Y.Length;
            double YLower = 0.0;
            double YUpper = 0.0;
            //double dY = 0.0;
            int iYLower = 0;
            int iYUpper = 0;
            for (int i = 0; i < nY - 1; i++)
            {
                YLower = Y[i];
                YUpper = Y[i + 1];
                //dY = YUpper - YLower;
                if (YLower <= Yq && Yq <= YUpper)
                {
                    iYLower = i;
                    iYUpper = i + 1;
                    break;
                }
            }
            double[][] GridPoint = { new double[2] {XLeft, YLower},
                                     new double[2] {XLeft, YUpper},
                                     new double[2] {XRight,YLower},
                                     new double[2] {XRight,YUpper} };
            double[] GridValue = { Grid[iYLower][iXLeft],       // Lower Left
                               Grid[iYUpper][iXLeft],           // Upper Left
                               Grid[iYLower][iXRight],          // Lower Right
                               Grid[iYUpper][iXRight] };        // Upper Right

            return Bilinear_Interp(GridPoint, GridValue, Xq, Yq);

        }


        public static double Bilinear_Interp(double[][] GridPoint, double[] GridValue, double Xq, double Yq)
        {
            double[][] _A = new double[4][];
            for (int i = 0; i < 4; i++)
            {
                _A[i] = new double[4];
                _A[i][0] = 1.0;
                _A[i][1] = GridPoint[i][0];
                _A[i][2] = GridPoint[i][1];
                _A[i][3] = GridPoint[i][0] * GridPoint[i][1];
            }
            Matrix A = new Matrix(_A);
            Matrix Q = new Matrix(GridValue, 4);
            Matrix sol = A.Solve(Q);
            double val = sol.Get(0, 0) + Xq * sol.Get(1, 0) + Yq * sol.Get(2, 0) + Xq * Yq * sol.Get(3, 0);
            return val;
        }








    }
}



