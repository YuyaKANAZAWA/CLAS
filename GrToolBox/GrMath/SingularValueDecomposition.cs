using System;
using static System.Math;

namespace GrToolBox.GrMath
{
    /// <summary>
    /// m >= nなるm X n行列Aの特異値分解は，A = U* S* V'
    /// Uはm X n の直交行列，Sはn X n の対角行列，Vはn X nの直交行列
    /// 特異値sigma[k] = S[k][k]は降順 sigma[0] >= sigma[1] >= ... >= sigma[n-1] に格納されます．
    /// 条件数とランクの計算にはこのクラスを利用します．
    /// </summary>

    [Serializable]
    public class SingularValueDecomposition
    {
        /*-----------------
          Class variables 
        -----------------*/

        // 行列UとV
        private double[][] U { get; set; }
        private double[][] V { get; set; }

        // 特異値のベクトル
        private double[] s { get; set; }

        // 行数M，列数N
        private int M { get; set; }
        private int N { get; set; }

        /*-----------------
          コンストラクタ 
        -----------------*/

        // Arg の特異値分解を行う
        public SingularValueDecomposition(Matrix Arg)
        {

            // Derived from LINPACK code.
            // Initialize.
            double[][] A = Arg.GetArrayCopy();
            M = Arg.M;
            N = Arg.N;

            // Apparently the failing cases are only a proper subset of (m<n),
            // so let's not throw error.  Correct fix to come later?
            if (M < N)
            {
                throw new ArgumentException("This SVD only works for m >= n");
            }

            int nu = Min(M, N);
            s = new double[Min(M + 1, N)];
            U = new double[M][];
            for (int i = 0; i < M; i++)
            {
                U[i] = new double[nu];
            }
            V = new double[N][];
            for (int i = 0; i < N; i++)
            {
                V[i] = new double[N];
            }
            double[] e = new double[N];
            double[] work = new double[M];
            bool wantu = true;
            bool wantv = true;

            // Reduce A to bidiagonal form, storing the diagonal elements
            // in s and the super-diagonal elements in e.

            int nct = Min(M - 1, N);
            int nrt = Max(0, Min(N - 2, M));
            for (int k = 0; k < Max(nct, nrt); k++)
            {
                if (k < nct)
                {

                    // Compute the transformation for the k-th column and
                    // place the k-th diagonal in s[k].
                    // Compute 2-norm of k-th column without under/overflow.
                    s[k] = 0;
                    for (int i = k; i < M; i++)
                    {
                        //s[k] = hypot(s[k], A[i][k]);
                        s[k] = Sqrt(s[k] * s[k] + A[i][k] * A[i][k]);
                    }
                    if (s[k] != 0.0)
                    {
                        if (A[k][k] < 0.0)
                        {
                            s[k] = -s[k];
                        }
                        for (int i = k; i < M; i++)
                        {
                            A[i][k] /= s[k];
                        }
                        A[k][k] += 1.0;
                    }
                    s[k] = -s[k];
                }
                for (int j = k + 1; j < N; j++)
                {
                    if (k < nct & s[k] != 0.0)
                    {

                        // Apply the transformation.

                        double t = 0;
                        for (int i = k; i < M; i++)
                        {
                            t += A[i][k] * A[i][j];
                        }
                        t = -t / A[k][k];
                        for (int i = k; i < M; i++)
                        {
                            A[i][j] += t * A[i][k];
                        }
                    }

                    // Place the k-th row of A into e for the
                    // subsequent calculation of the row transformation.

                    e[j] = A[k][j];
                }
                if (wantu & k < nct)
                {

                    // Place the transformation in U for subsequent back
                    // multiplication.

                    for (int i = k; i < M; i++)
                    {
                        U[i][k] = A[i][k];
                    }
                }
                if (k < nrt)
                {
                    // Compute the k-th row transformation and place the
                    // k-th super-diagonal in e[k].
                    // Compute 2-norm without under/overflow.
                    e[k] = 0;
                    for (int i = k + 1; i < N; i++)
                    {
                        //e[k] = hypot(e[k], e[i]);
                        e[k] = Sqrt(e[k] * e[k] + e[i] * e[i]);
                    }
                    if (e[k] != 0.0)
                    {
                        if (e[k + 1] < 0.0)
                        {
                            e[k] = -e[k];
                        }
                        for (int i = k + 1; i < N; i++)
                        {
                            e[i] /= e[k];
                        }
                        e[k + 1] += 1.0;
                    }
                    e[k] = -e[k];
                    if (k + 1 < M & e[k] != 0.0)
                    {
                        // Apply the transformation.
                        for (int i = k + 1; i < M; i++)
                        {
                            work[i] = 0.0;
                        }
                        for (int j = k + 1; j < N; j++)
                        {
                            for (int i = k + 1; i < M; i++)
                            {
                                work[i] += e[j] * A[i][j];
                            }
                        }
                        for (int j = k + 1; j < N; j++)
                        {
                            double t = -e[j] / e[k + 1];
                            for (int i = k + 1; i < M; i++)
                            {
                                A[i][j] += t * work[i];
                            }
                        }
                    }
                    if (wantv)
                    {

                        // Place the transformation in V for subsequent
                        // back multiplication.
                        for (int i = k + 1; i < N; i++)
                        {
                            V[i][k] = e[i];
                        }
                    }
                }
            }

            // Set up the final bidiagonal matrix or order p.

            int p = Min(N, M + 1);
            if (nct < N)
            {
                s[nct] = A[nct][nct];
            }
            if (M < p)
            {
                s[p - 1] = 0.0;
            }
            if (nrt + 1 < p)
            {
                e[nrt] = A[nrt][p - 1];
            }
            e[p - 1] = 0.0;

            // If required, generate U.
            if (wantu)
            {
                for (int j = nct; j < nu; j++)
                {
                    for (int i = 0; i < M; i++)
                    {
                        U[i][j] = 0.0;
                    }
                    U[j][j] = 1.0;
                }
                for (int k = nct - 1; k >= 0; k--)
                {
                    if (s[k] != 0.0)
                    {
                        for (int j = k + 1; j < nu; j++)
                        {
                            double t = 0;
                            for (int i = k; i < M; i++)
                            {
                                t += U[i][k] * U[i][j];
                            }
                            t = -t / U[k][k];
                            for (int i = k; i < M; i++)
                            {
                                U[i][j] += t * U[i][k];
                            }
                        }
                        for (int i = k; i < M; i++)
                        {
                            U[i][k] = -U[i][k];
                        }
                        U[k][k] = 1.0 + U[k][k];
                        for (int i = 0; i < k - 1; i++)
                        {
                            U[i][k] = 0.0;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < M; i++)
                        {
                            U[i][k] = 0.0;
                        }
                        U[k][k] = 1.0;
                    }
                }
            }

            // If required, generate V.

            if (wantv)
            {
                for (int k = N - 1; k >= 0; k--)
                {
                    if (k < nrt & e[k] != 0.0)
                    {
                        for (int j = k + 1; j < nu; j++)
                        {
                            double t = 0;
                            for (int i = k + 1; i < N; i++)
                            {
                                t += V[i][k] * V[i][j];
                            }
                            t = -t / V[k + 1][k];
                            for (int i = k + 1; i < N; i++)
                            {
                                V[i][j] += t * V[i][k];
                            }
                        }
                    }
                    for (int i = 0; i < N; i++)
                    {
                        V[i][k] = 0.0;
                    }
                    V[k][k] = 1.0;
                }
            }

            // Main iteration loop for the singular values.

            int pp = p - 1;
            int iter = 0;
            double eps = Pow(2.0, -52.0);
            double tiny = Pow(2.0, -966.0);
            while (p > 0)
            {
                int k, kase;

                // Here is where a test for too many iterations would go.

                // This section of the program inspects for
                // negligible elements in the s and e arrays.  On
                // completion the variables kase and k are set as follows.

                // kase = 1     if s(p) and e[k-1] are negligible and k<p
                // kase = 2     if s(k) is negligible and k<p
                // kase = 3     if e[k-1] is negligible, k<p, and
                //              s(k), ..., s(p) are not negligible (qr step).
                // kase = 4     if e(p-1) is negligible (convergence).

                for (k = p - 2; k >= -1; k--)
                {
                    if (k == -1)
                    {
                        break;
                    }
                    if (Abs(e[k]) <= tiny + eps * (Abs(s[k]) + Abs(s[k + 1])))
                    {
                        e[k] = 0.0;
                        break;
                    }
                }
                if (k == p - 2)
                {
                    kase = 4;
                }
                else
                {
                    int ks;
                    for (ks = p - 1; ks >= k; ks--)
                    {
                        if (ks == k)
                        {
                            break;
                        }
                        double t = (ks != p ? Abs(e[ks]) : 0.0) + (ks != k + 1 ? Abs(e[ks - 1]) : 0.0);  // JAVA版は"0."
                        if (Abs(s[ks]) <= tiny + eps * t)
                        {
                            s[ks] = 0.0;
                            break;
                        }
                    }
                    if (ks == k)
                    {
                        kase = 3;
                    }
                    else if (ks == p - 1)
                    {
                        kase = 1;
                    }
                    else
                    {
                        kase = 2;
                        k = ks;
                    }
                }
                k++;

                // Perform the task indicated by kase.

                switch (kase)
                {
                    // Deflate negligible s(p).
                    case 1:
                        {
                            double f = e[p - 2];
                            e[p - 2] = 0.0;
                            for (int j = p - 2; j >= k; j--)
                            {
                                //double t = hypot(s[j], f);
                                double t = Sqrt(s[j] * s[j] + f * f);
                                double cs = s[j] / t;
                                double sn = f / t;
                                s[j] = t;
                                if (j != k)
                                {
                                    f = -sn * e[j - 1];
                                    e[j - 1] = cs * e[j - 1];
                                }
                                if (wantv)
                                {
                                    for (int i = 0; i < N; i++)
                                    {
                                        t = cs * V[i][j] + sn * V[i][p - 1];
                                        V[i][p - 1] = -sn * V[i][j] + cs * V[i][p - 1];
                                        V[i][j] = t;
                                    }
                                }
                            }
                        }
                        break;

                    // Split at negligible s(k).

                    case 2:
                        {
                            double f = e[k - 1];
                            e[k - 1] = 0.0;
                            for (int j = k; j < p; j++)
                            {
                                //double t = hypot(s[j], f);
                                double t = Sqrt(s[j] * s[j] + f * f);
                                double cs = s[j] / t;
                                double sn = f / t;
                                s[j] = t;
                                f = -sn * e[j];
                                e[j] = cs * e[j];
                                if (wantu)
                                {
                                    for (int i = 0; i < M; i++)
                                    {
                                        t = cs * U[i][j] + sn * U[i][k - 1];
                                        U[i][k - 1] = -sn * U[i][j] + cs * U[i][k - 1];
                                        U[i][j] = t;
                                    }
                                }
                            }
                        }
                        break;

                    // Perform one qr step.

                    case 3:
                        {
                            // Calculate the shift.
                            double scale = Max(Max(Max(Max(
                                    Abs(s[p - 1]), Abs(s[p - 2])), Abs(e[p - 2])),
                                    Abs(s[k])), Abs(e[k]));
                            double sp = s[p - 1] / scale;
                            double spm1 = s[p - 2] / scale;
                            double epm1 = e[p - 2] / scale;
                            double sk = s[k] / scale;
                            double ek = e[k] / scale;
                            double b = ((spm1 + sp) * (spm1 - sp) + epm1 * epm1) / 2.0;
                            double c = sp * epm1 * (sp * epm1);
                            double shift = 0.0;
                            if (b != 0.0 | c != 0.0)
                            {
                                shift = Sqrt(b * b + c);
                                if (b < 0.0)
                                {
                                    shift = -shift;
                                }
                                shift = c / (b + shift);
                            }
                            double f = (sk + sp) * (sk - sp) + shift;
                            double g = sk * ek;

                            // Chase zeros.

                            for (int j = k; j < p - 1; j++)
                            {
                                //double t = hypot(f, g);
                                double t = Sqrt(f * f + g * g);
                                double cs = f / t;
                                double sn = g / t;
                                if (j != k)
                                {
                                    e[j - 1] = t;
                                }
                                f = cs * s[j] + sn * e[j];
                                e[j] = cs * e[j] - sn * s[j];
                                g = sn * s[j + 1];
                                s[j + 1] = cs * s[j + 1];
                                if (wantv)
                                {
                                    for (int i = 0; i < N; i++)
                                    {
                                        t = cs * V[i][j] + sn * V[i][j + 1];
                                        V[i][j + 1] = -sn * V[i][j] + cs * V[i][j + 1];
                                        V[i][j] = t;
                                    }
                                }
                                //t = hypot(f, g);
                                t = Sqrt(f * f + g * g);
                                cs = f / t;
                                sn = g / t;
                                s[j] = t;
                                f = cs * e[j] + sn * s[j + 1];
                                s[j + 1] = -sn * e[j] + cs * s[j + 1];
                                g = sn * e[j + 1];
                                e[j + 1] = cs * e[j + 1];
                                if (wantu && j < M - 1)
                                {
                                    for (int i = 0; i < M; i++)
                                    {
                                        t = cs * U[i][j] + sn * U[i][j + 1];
                                        U[i][j + 1] = -sn * U[i][j] + cs * U[i][j + 1];
                                        U[i][j] = t;
                                    }
                                }
                            }
                            e[p - 2] = f;
                            iter++;
                        }
                        break;

                    // Convergence.

                    case 4:
                        {

                            // Make the singular values positive.

                            if (s[k] <= 0.0)
                            {
                                s[k] = s[k] < 0.0 ? -s[k] : 0.0;
                                if (wantv)
                                {
                                    for (int i = 0; i <= pp; i++)
                                    {
                                        V[i][k] = -V[i][k];
                                    }
                                }
                            }

                            // Order the singular values.

                            while (k < pp)
                            {
                                if (s[k] >= s[k + 1])
                                {
                                    break;
                                }
                                double t = s[k];
                                s[k] = s[k + 1];
                                s[k + 1] = t;
                                if (wantv && k < N - 1)
                                {
                                    for (int i = 0; i < N; i++)
                                    {
                                        t = V[i][k + 1];
                                        V[i][k + 1] = V[i][k];
                                        V[i][k] = t;
                                    }
                                }
                                if (wantu && k < M - 1)
                                {
                                    for (int i = 0; i < M; i++)
                                    {
                                        t = U[i][k + 1];
                                        U[i][k + 1] = U[i][k];
                                        U[i][k] = t;
                                    }
                                }
                                k++;
                            }
                            iter = 0;
                            p--;
                        }
                        break;
                }
            }
        }

        /*-----------------
          public methods 
         -----------------*/

        // A = U* S* V^TのUを返す
        // return: U
        public Matrix GetU()
        {
            return new Matrix(U, M, Min(M + 1, N));
        }

        // A = U* S* V^TのVを返す
        // return: V
        public Matrix GetV()
        {
            return new Matrix(V, N, N);
        }

        // 特異値が格納された1次元配列を返す
        // return: Sの対角要素
        public double[] GetSingularValues()
        {
            return s;
        }

        // A = U* S* V^TのSを返す
        // return: S
        public Matrix GetS()
        {
            Matrix X = new Matrix(N, N);
            double[][] S = X.A;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    S[i][j] = 0.0;
                }
                S[i][i] = s[i];
            }
            return X;
        }

        // 2-norm
        // return: max(S)
        public double Norm2()
        {
            return s[0];
        }

        // 条件数
        // max(S)/min(S)
        public double Cond()
        {
            return s[0] / s[Min(M, N) - 1];
        }

        // 行列の階数
        // return: Number of nonnegligible singular values
        public int Rank()
        {
            double eps = Pow(2.0, -52.0);
            double tol = Max(M, N) * s[0] * eps;
            int r = 0;
            foreach (double v in s)
            {
                if (v > tol)
                {
                    r++;
                }
            }
            return r;
        }

        private static readonly long serialVersionUID = 1;

    }
}
