using System;
using static System.Math;

namespace GrToolBox.GrMath
{
    /// <summary>
    ///  正定値対称行列Aについて，コレスキ分解を行います (A = L * L^T) .
    ///  行列Aが正定値対称行列でない場合，コンストラクタは部分的な分解を返し，内部フラグをセットします．
    /// </summary>

    [Serializable]
    public class CholeskyDecomposition
    {
        /*-----------------
          Class variables 
        -----------------*/

        // 内部保持用の配列
        private double[][] L { get; set; }

        // 行数，列数（正方行列）
        private int N { get; set; }

        // フラグ（正定値対称行列に対しtrue）
        private bool IsSPD { get; set; }

        /*-----------------
          コンストラクタ 
        -----------------*/

        // 正定値対称行列に対するコレスキ分解
        // Arg: 正定値対称行列
        public CholeskyDecomposition(Matrix Arg)
        {
            // Initialize.
            double[][] A = Arg.A;
            N = Arg.M;
            L = new double[N][];
            for (int i = 0; i < N; i++)
            {
                L[i] = new double[N];
            }
            IsSPD = Arg.N == N;
            // Main loop.
            for (int j = 0; j < N; j++)
            {
                double[] Lrowj = L[j];
                double d = 0.0;
                for (int k = 0; k < j; k++)
                {
                    double[] Lrowk = L[k];
                    double s = 0.0;
                    for (int i = 0; i < k; i++)
                    {
                        s += Lrowk[i] * Lrowj[i];
                    }
                    Lrowj[k] = s = (A[j][k] - s) / L[k][k];
                    d += s * s;
                    IsSPD = IsSPD & A[k][j] == A[j][k];
                }
                d = A[j][j] - d;
                IsSPD = IsSPD & d > 0.0;
                L[j][j] = Sqrt(Max(d, 0.0));
                for (int k = j + 1; k < N; k++)
                {
                    L[j][k] = 0.0;
                }
            }
        }

        // 下三角行列Ｌを返す
        // return: L
        public Matrix GetL()
        {
            return new Matrix(L, N, N);
        }


        // A*X = B を解く
        // B: Aと同じ行数の行列
        // return: L*L'*X = Bを満たすX
        // 例外: ArgumentException  "Matrix row dimensions must agree"
        // 例外: ArithmeticException  "Matrix is not symmetric positive definite"  // TODO: java版はRuntimeException　これで良いか？
        public Matrix Solve(Matrix B)
        {
            if (B.M != N)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!IsSPD)
            {
                throw new ArithmeticException("Matrix is not symmetric positive definite.");
            }

            // Copy right hand side.
            double[][] X = B.GetArrayCopy();
            int nx = B.N;

            // Solve L*Y = B;
            for (int k = 0; k < N; k++)
            {
                for (int j = 0; j < nx; j++)
                {
                    for (int i = 0; i < k; i++)
                    {
                        X[k][j] -= X[i][j] * L[k][i];
                    }
                    X[k][j] /= L[k][k];
                }
            }

            // Solve L'*X = Y;
            for (int k = N - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    for (int i = k + 1; i < N; i++)
                    {
                        X[k][j] -= X[i][j] * L[i][k];
                    }
                    X[k][j] /= L[k][k];
                }
            }
            return new Matrix(X, N, nx);
        }

        private static readonly long serialVersionUID = 1;
    }
}
