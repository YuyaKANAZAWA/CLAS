using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrToolBox.GrMath
{
    public class KalmanFilter
    {
        private int Nx { get; set; } = 0;    // 状態変数ベクトルの次元
        private int Ny { get; set; } = 0;    // 観測ベクトルの次元

        //public double[][] F { get; set; }
        //public double[][] H { get; set; }
        //public double[][] R { get; set; }
        //public double[][] Q { get; set; }
        //public double[][] P { get; set; }
        public double[] Xf { get; set; }
        public double[] Xp { get; set; }
        public double[][] Pf { get; set; }
        public double[][] Pp { get; set; }
        //public double[] Y { get; set; }
        //public double[] Innovation { get; set; }
        //private bool HaveInnovation { get; set; } = false;

        public Matrix Pmat { get; set; }
        public Matrix Fmat { get; set; }
        public Matrix Qmat { get; set; }
        public Matrix Rmat { get; set; }
        public Matrix Hmat { get; set; }
        public Matrix Xvec { get; set; }
        public Matrix Yvec { get; set; }    // innovation相当を格納

        public KalmanFilter() { }



        public void SetX(double[] x)
        {
            Xvec = new Matrix(x, x.Length);     // 値コピーで作成
            Nx = x.Length;
        }

        public void SetY(double[] y)
        {
            Yvec = new Matrix(y, y.Length);     // 値コピーで作成
            Ny = y.Length;
        }

        public void SetH(double[][] h)
        {
            Hmat = new Matrix(h);               // 参照で作成される
        }

        public void SetR(double[][] r)
        {
            Rmat = new Matrix(r);               // 参照で作成される
        }

        public void SetRdiag(double[] rdiag)        // 観測雑音共分散行列Rは正方に限る，値コピーで作成される
        {
            Rmat = new Matrix(rdiag.Length, rdiag.Length);
            for(int i = 0; i < rdiag.Length; i++)
            {
                Rmat.A[i][i] = rdiag[i];
            }
        }

        public void SetP(double[][] p)
        {
            Pmat = new Matrix(p);               // 参照で作成される
        }

        public void SetPdiag(double[] pdiag)        // 推定誤差共分散行列P，正方に限る，値コピーで作成される
        {
            Pmat = new Matrix(pdiag.Length, pdiag.Length);
            for (int i = 0; i < pdiag.Length; i++)
            {
                Pmat.A[i][i] = pdiag[i];
            }
        }

        public void SetQ(double[][] q)
        {
            Qmat = new Matrix(q);           // 参照で作成される
        }

        public void SetQdiag(double[] qdiag)    // システムノイズ共分散行列Q，正方に限る，値コピーで作成される
        {
            Qmat = new Matrix(qdiag.Length, qdiag.Length);
            for (int i = 0; i < qdiag.Length; i++)
            {
                Qmat.A[i][i] = qdiag[i];
            }
        }

        public void SetF(double[][] f)
        {
            Fmat = new Matrix(f);
        }

        // 観測更新用チェックを作る，次元，イノベーション，推定誤差共分散，観測雑音共分散
        private void CheckForUpdate()
        {
            if (Xvec == null)
            {
                throw new ArithmeticException("KF: State vector is not be set");
            }
            if (Fmat == null)
            {
                throw new ArithmeticException("KF: F is not be set");
            }
            if (Qmat == null)
            {
                throw new ArithmeticException("KF: Q is not be set");
            }
            if (Yvec == null)
            {
                throw new ArithmeticException("KF: Y or Innovation is not be set");
            }
            if (Hmat == null)
            {
                throw new ArithmeticException("KF: H not set");
            }
            if (Pmat == null)
            {
                throw new ArithmeticException("KF: P not set");
            }
            if(Nx != Fmat.M || Nx != Fmat.N)
            {
                throw new ArithmeticException("KF: F must be (N_state)X(N_state)");
            }
            if(Ny != Rmat.M || Ny != Rmat.N)
            {
                throw new ArithmeticException("KF: R must be (N_meas)X(N_meas)");
            }
            if(Ny != Hmat.M || Nx != Hmat.N)
            {
                throw new ArithmeticException("KF: H must be (N_meas)X(N_state)");
            }
            if(Nx != Qmat.M || Nx != Qmat.N)
            {
                throw new ArithmeticException("KF: Q must be (N_state)X(N_state)");
            }
        }

        private void DbgArrayDisp(double[][] a, string name)
        {
            System.Console.WriteLine("\n" + name + " = ");
            foreach (var row in a)
            {
                foreach (var element in row)
                {
                    System.Console.Write($"{element:0.000} ");
                }
                System.Console.Write("\n");
            }

        }

        private void DbgVecDisp(double[] a, string name)
        {
            System.Console.WriteLine("\n" + name + " = ");
            foreach (var element in a)
            {
                System.Console.Write($"{element:0.000} ");
            }
            System.Console.Write("\n");
        }

        public void MeasUpdate()
        {
            CheckForUpdate();
            // Kalman gain (K = P H^T (HPH^T + R)^{-1})
            Matrix Kmat = Pmat.Times(Hmat.Transpose()).Times((Hmat.Times(Pmat).Times(Hmat.Transpose()).Plus(Rmat)).Inverse());
            //Kmat.Print(6, 3);

            // x=x+K(Y)
            Xvec.PlusEquals(Kmat.Times(Yvec));

            // P=P-KHP
            Pmat = Pmat.MinusEquals(Kmat.Times(Hmat.Times(Pmat)));

            // フィルタ値を保存
            Xf = Xvec.GetRowPackedCopy();
            Pf = Pmat.GetArrayCopy();
        }

        public void TimeUpdate()
        {
            // X = F X
            Xvec = Fmat.Times(Xvec);
            // P = F P F^T + Q
            Pmat = Fmat.Times(Pmat.Times(Fmat.Transpose())).Plus(Qmat);
            // 予測値を保存
            Xp = Xvec.GetRowPackedCopy();
            Pp = Pmat.GetArrayCopy();
        }

        // UD 観測更新
        //public void MeasUpdate()
        //{
        //    CheckForUpdate();

        //    DbgArrayDisp(H, "H in KF");
        //    DbgVecDisp(Rdiag, "Rdiag in KF");


        //    double[] FF = new double[Nx];
        //    double[] GG = new double[Nx];
        //    double alpha, beta, gamma, lambda;

        //    for (int L = 0; L < Ny; L++)                     // DO 1 loop
        //    {
        //        //if (!HaveInnovation)
        //        //{
        //        //    for (int j = 0; j < Nx; j++)            // DO 2 loop
        //        //    {
        //        //        Innovation[L] -= H[L][j] * X[j];
        //        //    }
        //        //}
        //        for (int j = Nx - 1; j > 0; j--)            // DO 10 loop
        //        {
        //            FF[j] = H[L][j];
        //            for (int k = 0; k < j; k++)           // DO 5 loop
        //            {
        //                FF[j] += H[L][k] * Pud[k][j];
        //            }
        //            GG[j] = Pud[j][j] * FF[j];
        //        }
        //        FF[0] = H[L][0];
        //        GG[0] = Pud[0][0] * FF[0];

        //        // FF:=(U^T)*(H^T), GG:=D*(U^T)*(H^T)

        //        alpha = Rdiag[L] + FF[0] * GG[0];
        //        gamma = 1.0 / alpha;
        //        Pud[0][0] = Rdiag[L] * gamma * Pud[0][0];
        //        for (int j = 1; j < Nx; j++)                 // DO 20 loop
        //        {
        //            beta = alpha;
        //            alpha += FF[j] * GG[j];
        //            lambda = FF[j] * gamma;
        //            gamma = 1.0 / alpha;
        //            Pud[j][j] = beta * gamma * Pud[j][j];
        //            for (int i = 0; i < j; i++)          // DO 20 loop
        //            {
        //                beta = Pud[i][j];
        //                Pud[i][j] = beta - lambda * GG[i];
        //                GG[i] += beta * GG[j];   ///////
        //            }
        //        }
        //        Innovation[L] = Innovation[L] * gamma;
        //        for (int j = 0; j < Nx; j++)                 // DO 30 loop
        //        {
        //            X[j] += GG[j] * Innovation[L];
        //        }
        //    }


        //    //double[] FF = new double[Nx];
        //    //double[] GG = new double[Nx];
        //    //double sum, a, b, c, d, uij;

        //    //for (int k = 0; k < Ny; k++)
        //    //{
        //    //    for (int j = 0; j < Nx; j++)
        //    //    {
        //    //        sum = H[k][j];
        //    //        for (int i = 0; i < j; i++)
        //    //            sum += H[k][i] * Pud[i][j];
        //    //        FF[j] = sum;
        //    //        GG[j] = sum * Pud[j][j];
        //    //    }
        //    //    a = Rdiag[k];
        //    //    c = 1.0 / a;
        //    //    for (int j = 0; j < Nx; j++)
        //    //    {
        //    //        b = a;
        //    //        a += FF[j] * GG[j];
        //    //        d = c * FF[j];
        //    //        c = 1.0 / a;
        //    //        Pud[j][j] *= b * c;
        //    //        for (int i = 0; i < j; i++)
        //    //        {
        //    //            uij = Pud[i][j];
        //    //            Pud[i][j] -= d * GG[i];
        //    //            GG[i] += uij * GG[j];
        //    //        }
        //    //    }
        //    //    for (int j = 0; j < Nx; j++)
        //    //        X[j] += c * GG[j] * Innovation[k];
        //    //}


        //}


        // UD 時間更新
        //public void TimeUpdate()
        //{
        //    Nw = Nx;
        //    Nxw = Nx + Nw;
        //    double[] V = new double[Nxw];
        //    double[] Z = new double[Nxw];
        //    double[] QQ = new double[Nxw];
        //    double[][] W = new double[Nx][];
        //    for(int i = 0; i < Nx; i++)
        //    {
        //        W[i] = new double[Nxw];
        //    }

        //    double sum;
        //    for(int i = 0; i < Nx; i++)         // DO 40 loop
        //    {
        //        sum = 0.0;
        //        for(int j = 0; j < Nx; j++)     // DO 35 loop
        //        {
        //            sum += F[i][j] * X[j];
        //        }
        //        V[i] = sum;
        //    }

        //    for(int j = Nx - 1; j > 0; j--)     // DO 50 loop
        //    {
        //        QQ[j] = Pud[j][j];
        //        X[j] = V[j];
        //        for(int i = 0; i < Nx; i++)     // Do 50 loop
        //        {
        //            sum = F[i][j];
        //            for(int k = 0; k < j; k++)  // DO 45 loop /////
        //            {
        //                sum += F[i][k] * Pud[k][j];
        //            }
        //            W[i][j] = sum;
        //        }
        //    }

        //    for(int i = 0; i < Nx; i++)             // DO 55 loop
        //    {
        //        for(int j = 0; j < Nw; j++)         // DO 56 loop
        //        {
        //            W[i][j+Nx] = G[i][j];
        //        }
        //        W[i][0] = F[i][0];
        //    }
        //    QQ[0] = Pud[0][0];
        //    X[0] = V[0];

        //    // W:=[FU | G], X:= FX
        //    for( int j = Nx - 1; j > 0; j--)            // DO 90 loop
        //    {
        //        sum = 0.0;
        //        for(int k = 0; k < Nxw; k++)        // DO 80 loop
        //        {
        //            V[k] = W[j][k];
        //            Z[k] = V[k] * QQ[k];
        //            sum += Z[k] * V[k];
        //        }
        //        Pud[j][j] = sum;
        //        for(int i = 0; i < j; i++)      // DO 70 loop
        //        {
        //            sum = 0.0;
        //            for(int k = 0; k < Nxw; k++)    // DO 65 loop
        //            {
        //                sum += W[i][k] * Z[k];
        //            }
        //            sum /= Pud[j][j];
        //            for(int k = 0; k < Nxw; k++)    // DO 60 loop
        //            {
        //                W[i][k] -= sum * V[k];
        //            }
        //            Pud[i][j] = sum;
        //        }
        //    }
        //    sum = 0.0;
        //    for(int k =0; k < Nxw; k++)
        //    {
        //        sum += QQ[k] * W[0][k] * W[0][k];
        //    }
        //    Pud[0][0] = sum;
        //}




    }
}
