using System;
using static System.Math;

namespace GrToolBox.GrMath
{
    /// <summary>
    /// m >= nなるm X n行列AのQR分解   A = Q * R
    /// Qはn X nの直交行列，Rはn X nの上三角行列
    /// 
    /// </summary>
    [Serializable]
    public class QRDecomposition
    {
        /*-----------------
          Class variables 
        -----------------*/

        // QR分解の内部保持用の配列
        private double[][] QR { get; set; }

        // 行数，列数，ピボットの符号
        private int M { get; set; }
        private int N { get; set; }

        // 行列Rの対角成分保持用の配列
        private double[] Rdiag { get; set; }

        /*-----------------
          コンストラクタ 
        -----------------*/

        // 行列Aのハウスホルダー法によるQR分解
        public QRDecomposition(Matrix A)
        {
            // Initialize.
            QR = A.GetArrayCopy();
            M = A.M;
            N = A.N;
            Rdiag = new double[N];

            // Main loop.
            for (int k = 0; k < N; k++)
            {
                // Compute 2-norm of k-th column without under/overflow.
                double nrm = 0;
                for (int i = k; i < M; i++)
                {
                    //nrm = hypot(nrm, QR[i][k]);
                    nrm = Sqrt(nrm * nrm + QR[i][k] * QR[i][k]);
                }

                if (nrm != 0.0)
                {
                    // Form k-th Householder vector.
                    if (QR[k][k] < 0)
                    {
                        nrm = -nrm;
                    }
                    for (int i = k; i < M; i++)
                    {
                        QR[i][k] /= nrm;
                    }
                    QR[k][k] += 1.0;

                    // Apply transformation to remaining columns.
                    for (int j = k + 1; j < N; j++)
                    {
                        double s = 0.0;
                        for (int i = k; i < M; i++)
                        {
                            s += QR[i][k] * QR[i][j];
                        }
                        s = -s / QR[k][k];
                        for (int i = k; i < M; i++)
                        {
                            QR[i][j] += s * QR[i][k];
                        }
                    }
                }
                Rdiag[k] = -nrm;
            }
        }

        /*-----------------
          public methods 
         -----------------*/

        // フルランクか否かの判定
        // return: true (Aがフルランク)
        public bool IsFullRank()
        {
            for (int j = 0; j < N; j++)
            {
                if (Rdiag[j] == 0)
                    return false;
            }
            return true;
        }

        // ハウスホルダ行列を返す
        // return: H
        public Matrix GetH()
        {
            Matrix X = new Matrix(M, N);
            double[][] H = X.A;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (i >= j)
                    {
                        H[i][j] = QR[i][j];
                    }
                    else
                    {
                        H[i][j] = 0.0;
                    }
                }
            }
            return X;
        }

        // 上三角行列Rを返す
        // return: R
        public Matrix GetR()
        {
            Matrix X = new Matrix(N, N);
            double[][] R = X.A;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (i < j)
                    {
                        R[i][j] = QR[i][j];
                    }
                    else if (i == j)
                    {
                        R[i][j] = Rdiag[i];
                    }
                    else
                    {
                        R[i][j] = 0.0;
                    }
                }
            }
            return X;
        }

        // 直交行列Qを返す
        // return: Q
        public Matrix GetQ()
        {
            Matrix X = new Matrix(M, N);
            double[][] Q = X.A;
            for (int k = N - 1; k >= 0; k--)
            {
                for (int i = 0; i < M; i++)
                {
                    Q[i][k] = 0.0;
                }
                Q[k][k] = 1.0;
                for (int j = k; j < N; j++)
                {
                    if (QR[k][k] != 0)
                    {
                        double s = 0.0;
                        for (int i = k; i < M; i++)
                        {
                            s += QR[i][k] * Q[i][j];
                        }
                        s = -s / QR[k][k];
                        for (int i = k; i < M; i++)
                        {
                            Q[i][j] += s * QR[i][k];
                        }
                    }
                }
            }
            return X;
        }

        // A*X = B の最小2乗解を求める
        // B: Aの行数と同じ行数を持つベクトル（行列）
        // return: 最小2乗解X (Q*R*X-Bのノルム2乗を最小化するX)
        // 例外: ArgumentException  "Matrix row dimensions must agree"
        // 例外: ArithmeticException  "Matrix is rank deficient"  // TODO: java版はRuntimeException　これで良いか？
        public Matrix Solve(Matrix B)
        {
            if (B.M != M)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!IsFullRank())
            {
                throw new ArithmeticException("Matrix is rank deficient.");
            }

            // Copy right hand side
            int nx = B.N;
            double[][] X = B.GetArrayCopy();

            // Compute Y = transpose(Q)*B
            for (int k = 0; k < N; k++)
            {
                for (int j = 0; j < nx; j++)
                {
                    double s = 0.0;
                    for (int i = k; i < M; i++)
                    {
                        s += QR[i][k] * X[i][j];
                    }
                    s = -s / QR[k][k];
                    for (int i = k; i < M; i++)
                    {
                        X[i][j] += s * QR[i][k];
                    }
                }
            }
            // Solve R*X = Y;
            for (int k = N - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    X[k][j] /= Rdiag[k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i][j] -= X[k][j] * QR[i][k];
                    }
                }
            }
            return new Matrix(X, N, nx).GetMatrix(0, N - 1, 0, nx - 1);
        }

        private static readonly long serialVersionUID = 1;

    }
}
