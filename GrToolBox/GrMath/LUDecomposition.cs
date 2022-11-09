using System;
using static System.Math;

namespace GrToolBox.GrMath
{
    /// <summary>
    /// 行列AをLUに分解します
    /// m >= nなる m x n 行列Aについて，行列Lは m X n の下三角行列(対角成分1)，行列Uは n X n の上三角行列となります
    /// また，m次元ベクトルpivはAの行の入替を表し，A(piv,:) = L* Uです．
    ///  For an m-by-n matrix A with m >= n, the LU decomposition is an m-by-n
    /// m < nの場合は，Lが m X m，Uが m X n行列となります．
    /// </summary>

    [Serializable]
    public class LUDecomposition
    {
        /*-----------------
          Class variables 
        -----------------*/

        // 内部保持用の配列
        private double[][] LU { get; set; }

        // 行数，列数，ピボットの符号
        private int m { get; set; }
        private int n { get; set; }
        private int pivsign { get; set; }

        // ピボットベクトルの内部保持用配列
        private int[] piv { get; set; }

        /*-----------------
          コンストラクタ 
        -----------------*/
        public LUDecomposition(Matrix A)
        {

            // Use a "left-looking", dot-product, Crout/Doolittle algorithm.

            LU = A.GetArrayCopy();
            m = A.M;
            n = A.N;
            piv = new int[m];
            for (int i = 0; i < m; i++)
            {
                piv[i] = i;
            }
            pivsign = 1;
            double[] LUrowi;
            double[] LUcolj = new double[m];

            // Outer loop.

            for (int j = 0; j < n; j++)
            {

                // Make a copy of the j-th column to localize references.

                for (int i = 0; i < m; i++)
                {
                    LUcolj[i] = LU[i][j];
                }

                // Apply previous transformations.

                for (int i = 0; i < m; i++)
                {
                    LUrowi = LU[i];

                    // Most of the time is spent in the following dot product.

                    int kmax = Min(i, j);
                    double s = 0.0;
                    for (int k = 0; k < kmax; k++)
                    {
                        s += LUrowi[k] * LUcolj[k];
                    }

                    LUrowi[j] = LUcolj[i] -= s;
                }

                // Find pivot and exchange if necessary.

                int p = j;
                for (int i = j + 1; i < m; i++)
                {
                    if (Abs(LUcolj[i]) > Abs(LUcolj[p]))
                    {
                        p = i;
                    }
                }
                if (p != j)
                {
                    for (int k = 0; k < n; k++)
                    {
                        double t = LU[p][k]; LU[p][k] = LU[j][k]; LU[j][k] = t;
                    }
                    int kk = piv[p]; piv[p] = piv[j]; piv[j] = kk;
                    pivsign = -pivsign;
                }

                // Compute multipliers.

                if (j < m & LU[j][j] != 0.0)
                {
                    for (int i = j + 1; i < m; i++)
                    {
                        LU[i][j] /= LU[j][j];
                    }
                }
            }
        }

        /*-----------------
          public methods 
         -----------------*/

        // 正則か否かをチェック
        // return: true: U(すなわちA)が正則
        public bool IsNonsingular()
        {
            for (int j = 0; j < n; j++)
            {
                if (LU[j][j] == 0)
                    return false;
            }
            return true;
        }

        // 下三角行列Ｌを返す
        // return: L
        public Matrix GetL()
        {
            Matrix X = new Matrix(m, n);
            double[][] L = X.A;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i > j)
                    {
                        L[i][j] = LU[i][j];
                    }
                    else if (i == j)
                    {
                        L[i][j] = 1.0;
                    }
                    else
                    {
                        L[i][j] = 0.0;
                    }
                }
            }
            return X;
        }

        // 上三角行列Uを返す
        // return: U
        public Matrix GetU()
        {
            Matrix X = new Matrix(n, n);
            double[][] U = X.A;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i <= j)
                    {
                        U[i][j] = LU[i][j];
                    }
                    else
                    {
                        U[i][j] = 0.0;
                    }
                }
            }
            return X;
        }

        // pivot permutation vectorを返す
        // return: piv
        public int[] GetPivot()
        {
            int[] p = new int[m];
            for (int i = 0; i < m; i++)
            {
                p[i] = piv[i];
            }
            return p;
        }

        // pivot permutation vectorをdouble型配列で返す
        // return: (double)piv
        public double[] GetDoublePivot()
        {
            double[] vals = new double[m];
            for (int i = 0; i < m; i++)
            {
                vals[i] = piv[i];
            }
            return vals;
        }

        // 行列式の値を返す
        // return: det(A)
        // 例外: ArgumentException  "Matrix must be square"
        public double Det()
        {
            if (m != n)
            {
                throw new ArgumentException("Matrix must be square.");
            }
            double d = pivsign;
            for (int j = 0; j < n; j++)
            {
                d *= LU[j][j];
            }
            return d;
        }

        // A*X = Bの解を求める
        // B: 右辺の行列(行数はAの行数に等しい), 列数は1以上の任意
        // return: LU分解に基づく解X, すなわち L*U*X = B(piv,:)
        // 例外: ArgumentException "Matrix row dimensions must agree"
        // 例外: RuntimeException  "Matrix is singular"
        public Matrix Solve(Matrix B)
        {
            if (B.M != m)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!IsNonsingular())
            {
                //throw new RuntimeException("Matrix is singular.");
                throw new ArithmeticException("Matrix is singular.");  // ArithmeticExceptionでよいか？
            }

            // Copy right hand side with pivoting
            int nx = B.N;
            Matrix Xmat = B.GetMatrix(piv, 0, nx - 1);
            double[][] X = Xmat.A;

            // Solve L*Y = B(piv,:)
            for (int k = 0; k < n; k++)
            {
                for (int i = k + 1; i < n; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i][j] -= X[k][j] * LU[i][k];
                    }
                }
            }
            // Solve U*X = Y;
            for (int k = n - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    X[k][j] /= LU[k][k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i][j] -= X[k][j] * LU[i][k];
                    }
                }
            }
            return Xmat;
        }

        private static readonly long serialVersionUID = 1;


    }
}
