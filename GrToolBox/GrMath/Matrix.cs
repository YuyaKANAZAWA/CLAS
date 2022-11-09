using System;
using static System.Math;

namespace GrToolBox.GrMath
{
    [Serializable]
    public class Matrix
    {
        //private double[,] A { get; set; }
        public double[][] A { get; set; }

        public int M { get; set; } // number of rows
        public int N { get; set; } // number of columns

        // コンストラクタ
        // mXn行列を作成 0で初期化
        // m: 行数
        // n: 列数
        public Matrix(int m, int n)
        {
            M = m;
            N = n;
            A = new double[m][];
            for (int i = 0; i < m; i++)
            {
                A[i] = new double[n];
            }
        }

        // コンストラクタ
        // mXn行列を作成，スカラ－引数sの値で初期化
        // m: 行数
        // n: 列数
        public Matrix(int m, int n, double s)
        {
            M = m;
            N = n;
            A = new double[m][];
            for (int i = 0; i < m; i++)
            {
                A[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    A[i][j] = s;
                }
            }
        }

        // コンストラクタ
        // 2次元配列からMatrixを作成
        // A: double型2次元配列
        // 例外: ArgumentException "All rows must have the same length"
        public Matrix(double[][] A)
        {
            M = A.Length;
            N = A[0].Length;
            for (int i = 0; i < M; i++)
            {
                if (A[i].Length != N)
                {
                    throw new ArgumentException("All rows must have the same length.");
                }
            }
            this.A = A;
        }

        // コンストラクタ
        // 2次元配列からMatrixを作成(サイズチェック無し)
        // A: double型2次元配列
        public Matrix(double[][] A, int m, int n)
        {
            this.A = A;
            M = m;
            N = n;
        }


        // 1次元配列から行列作成
        // vals: double型1次元配列，列方向に展開されたもの
        // m: 行数
        // 例外: ArgumentOutOfRangeException "Array length must be a multiple of m."
        public Matrix(double[] vals, int m)
        {
            M = m;
            N = m != 0 ? vals.Length / m : 0;
            if (m * N != vals.Length)
            {
                throw new ArgumentOutOfRangeException("Array length must be a multiple of m.");
            }
            A = new double[m][];
            for (int i = 0; i < m; i++)
            {
                A[i] = new double[N];
                for (int j = 0; j < N; j++)
                {
                    A[i][j] = vals[i + j * m];
                }
            }
        }

        /*-----------------
          public methods 
         -----------------*/

        // 2次元配列のコピーからMatrixを作成
        // A:   double型2次元配列
        // 例外: IllegalArgumentException All rows must have the same length
        public static Matrix ConstructWithCopy(double[][] A)
        {
            int m = A.Length;
            int n = A[0].Length;
            Matrix X = new Matrix(m, n);
            double[][] C = X.A;
            for (int i = 0; i < m; i++)
            {
                if (A[i].Length != n)
                {
                    throw new ArgumentException("All rows must have the same length.");
                }
                C[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    C[i][j] = A[i][j];
                }
            }
            return X;
        }

        // deep copy of a matrix
        public Matrix Copy()
        {
            Matrix X = new Matrix(M, N);
            double[][] C = X.A;
            for (int i = 0; i < M; i++)
            {
                C[i] = new double[N];
                for (int j = 0; j < N; j++)
                {
                    C[i][j] = A[i][j];
                }
            }
            return X;
        }

        // Clone the Matrix object
        public object Clone()
        {
            return Copy();
        }

        // getArray: JAVA版の内部行列Aへのアクセサ 
        // == C#版: Matrix.A


        // 内部行列Aのコピー
        // return: 行列要素の2次元配列へのコピー
        public double[][] GetArrayCopy()
        {
            double[][] C = new double[M][];
            for (int i = 0; i < M; i++)
            {
                C[i] = new double[N];
                for (int j = 0; j < N; j++)
                {
                    C[i][j] = A[i][j];
                }
            }
            return C;
        }

        // 内部行列Aの1次元配列への展開
        // 行列Aの要素を列方向に展開した1次元配列
        public double[] GetColumnPackedCopy()
        {
            double[] vals = new double[M * N];
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    vals[i + j * M] = A[i][j];
                }
            }
            return vals;
        }

        // 内部行列Aの1次元配列への展開
        // 行列Aの要素を行方向に展開した1次元配列
        public double[] GetRowPackedCopy()
        {
            double[] vals = new double[M * N];
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    vals[i * N + j] = A[i][j];
                }
            }
            return vals;
        }

        // getRowDimension (JAVA版,行数へのアクセサ)        -->> Matrix.MでもOK
        public int GetRowDimension()
        {
            return M;
        }

        // getColumnDimension (JAVA版,列数へのアクセサ)     -->> Matrix.NでもOK
        public int GetColumnDimension()
        {
            return N;
        }

        // Get a single element (JAVA版,行列要素へのアクセサ)-->> Matrix.A[i,j]でもOK
        public double Get(int i, int j)
        {
            return A[i][j];
        }

        // ブロック行列の取得(指定範囲)
        // i0: 開始行インデックス
        // i1: 終了行インデックス
        // j0: 開始列インデックス
        // j1: 終了列インデックス
        // return: A(i0:i1,j0:j1)
        // 例外: ArgumentOutOfRangeException "Submatrix indices"
        public Matrix GetMatrix(int i0, int i1, int j0, int j1)
        {
            Matrix X = new Matrix(i1 - i0 + 1, j1 - j0 + 1);
            double[][] B = X.A;
            try
            {
                for (int i = i0; i <= i1; i++)
                {
                    for (int j = j0; j <= j1; j++)
                    {
                        B[i - i0][j - j0] = A[i][j];
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Submatrix indices");
            }
            return X;
        }

        // ブロック行列の取得(インデックスベクトルで指定された行と列)
        // r: 行インデックスの配列
        // c: 列インデックスの配列
        // return: A(r(:),c(:))
        // 例外: ArgumentOutOfRangeException "Submatrix indices"
        public Matrix GetMatrix(int[] r, int[] c)
        {
            Matrix X = new Matrix(r.Length, c.Length);
            double[][] B = X.A;
            try
            {
                for (int i = 0; i < r.Length; i++)
                {
                    for (int j = 0; j < c.Length; j++)
                    {
                        B[i][j] = A[r[i]][c[j]];
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Submatrix indices");
            }
            return X;
        }

        // ブロック行列の取得(指定範囲の行と，インデックスベクトルで指定された列)
        // i0: 開始行インデックス
        // i1: 終了行インデックス
        // c:  列インデックスの配列
        // return: A(i0:i1,c(:))
        // 例外: ArgumentOutOfRangeException "Submatrix indices"
        public Matrix GetMatrix(int i0, int i1, int[] c)
        {
            Matrix X = new Matrix(i1 - i0 + 1, c.Length);
            double[][] B = X.A;
            try
            {
                for (int i = i0; i <= i1; i++)
                {
                    for (int j = 0; j < c.Length; j++)
                    {
                        B[i - i0][j] = A[i][c[j]];
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Submatrix indices");
            }
            return X;
        }

        // ブロック行列の取得(インデックスベクトルで指定された行と，指定範囲の列)
        // r: 行インデックスの配列
        // j0: 開始列インデックス
        // j1: 終了列インデックス
        // return: A(r(:),j0:j1)
        // 例外: ArgumentOutOfRangeException "Submatrix indices"
        public Matrix GetMatrix(int[] r, int j0, int j1)
        {
            Matrix X = new Matrix(r.Length, j1 - j0 + 1);
            double[][] B = X.A;
            try
            {
                for (int i = 0; i < r.Length; i++)
                {
                    for (int j = j0; j <= j1; j++)
                    {
                        B[i][j - j0] = A[r[i]][j];
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Submatrix indices");
            }
            return X;
        }

        // set (JAVA版,行列要素のセッタ) -->> Matrix.A[i][j] = s


        // ブロック行列をセット(指定範囲)
        // i0:  開始行インデックス
        // i1:  終了行インデックス
        // j0:  開始列インデックス
        // j1:  終了列インデックス
        // X:   A(i0:i1,j0:j1)
        // 例外: ArgumentOutOfRangeException "Submatrix indices"
        public void SetMatrix(int i0, int i1, int j0, int j1, Matrix X)
        {
            try
            {
                for (int i = i0; i <= i1; i++)
                {
                    for (int j = j0; j <= j1; j++)
                    {
                        A[i][j] = X.A[i - i0][j - j0];
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Submatrix indices");
            }
        }

        // ブロック行列をセット(インデックスベクトルで指定された行と列)
        // r: 行インデックスの配列
        // c: 列インデックスの配列
        // X: A(r(:),c(:))
        // 例外: ArgumentOutOfRangeException "Submatrix indices"
        public void SetMatrix(int[] r, int[] c, Matrix X)
        {
            try
            {
                for (int i = 0; i < r.Length; i++)
                {
                    for (int j = 0; j < c.Length; j++)
                    {
                        A[r[i]][c[j]] = X.A[i][j];
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Submatrix indices");
            }
        }

        // ブロック行列をセット(インデックスベクトルで指定された行と，指定範囲の列)
        // r:   行インデックスの配列
        // j0:  開始列インデックス
        // j1:  終了列インデックス
        // X:    A(r(:),j0:j1)
        // 例外: ArgumentOutOfRangeException "Submatrix indices"
        public void SetMatrix(int[] r, int j0, int j1, Matrix X)
        {
            try
            {
                for (int i = 0; i < r.Length; i++)
                {
                    for (int j = j0; j <= j1; j++)
                    {
                        A[r[i]][j] = X.A[i][j - j0];
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Submatrix indices");
            }
        }

        // ブロック行列をセット(指定範囲の行と，インデックスベクトルで指定された列)
        // i0: 　開始行インデックス
        // i1: 　終了行インデックス
        // c:  　列インデックスの配列
        // X:  　A(i0:i1,c(:))
        // 例外: ArgumentOutOfRangeException "Submatrix indices"
        public void SetMatrix(int i0, int i1, int[] c, Matrix X)
        {
            try
            {
                for (int i = i0; i <= i1; i++)
                {
                    for (int j = 0; j < c.Length; j++)
                    {
                        A[i][c[j]] = X.A[i - i0][j];
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Submatrix indices");
            }
        }

        // Matrix transpose
        // return:    A^T
        public Matrix Transpose()
        {
            Matrix X = new Matrix(N, M);
            double[][] C = X.A;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[j][i] = A[i][j];
                }
            }
            return X;
        }

        // 1-norm
        // return: maximum column sum
        public double Norm1()
        {
            double f = 0;
            for (int j = 0; j < N; j++)
            {
                double s = 0;
                for (int i = 0; i < M; i++)
                {
                    s += Abs(A[i][j]);
                }
                f = Max(f, s);
            }
            return f;
        }

        // 2-norm
        // return: maximum singular value
        public double Norm2()
        {
            return new SingularValueDecomposition(this).Norm2();
        }

        // Infinity norm
        // return: maximum row sum
        public double NormInf()
        {
            double f = 0;
            for (int i = 0; i < M; i++)
            {
                double s = 0;
                for (int j = 0; j < N; j++)
                {
                    s += Abs(A[i][j]);
                }
                f = Max(f, s);
            }
            return f;
        }

        // Frobenius norm
        // return: sqrt of sum of squares of all elements
        public double NormF()
        {
            double f = 0;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    //f = hypot(f, A[i][j]);
                    f = Sqrt(f * f + A[i][j] * A[i][j]);   //TODO: 可能であればhypotで置き換える
                }
            }
            return f;
        }

        // unary minus
        // return:  -A
        public Matrix UMinus()
        {
            Matrix X = new Matrix(M, N);
            double[][] C = X.A;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[i][j] = -A[i][j];
                }
            }
            return X;
        }

        // C = A + B
        // B: another matrix
        public Matrix Plus(Matrix B)
        {
            CheckMatrixDimensions(B);
            Matrix X = new Matrix(M, N);
            double[][] C = X.A;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[i][j] = A[i][j] + B.A[i][j];
                }
            }
            return X;
        }

        // A = A + B
        // B: another matrix
        // return: AをA+Bで置き換え
        public Matrix PlusEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    A[i][j] = A[i][j] + B.A[i][j];
                }
            }
            return this;
        }

        // C = A - B
        // B: another matrix
        // return: A - B
        public Matrix Minus(Matrix B)
        {
            CheckMatrixDimensions(B);
            Matrix X = new Matrix(M, N);
            double[][] C = X.A;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[i][j] = A[i][j] - B.A[i][j];
                }
            }
            return X;
        }

        // A = A - B
        // B: another matrix
        // return: AをA-Bで置き換え
        public Matrix MinusEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    A[i][j] = A[i][j] - B.A[i][j];
                }
            }
            return this;
        }

        // 行列要素毎の積, C = A.*B
        // B: another matrix
        // return:  A.*B
        public Matrix ArrayTimes(Matrix B)
        {
            CheckMatrixDimensions(B);
            Matrix X = new Matrix(M, N);
            double[][] C = X.A;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[i][j] = A[i][j] * B.A[i][j];
                }
            }
            return X;
        }

        // 行列要素毎の積, A = A.*B
        // B: another matrix
        // return: AをA.*Bで置き換え
        public Matrix ArrayTimesEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    A[i][j] = A[i][j] * B.A[i][j];
                }
            }
            return this;
        }

        // 行列要素毎の(右)除算, C = A./B
        // B: another matrix
        // return: A./B
        public Matrix ArrayRightDivide(Matrix B)
        {
            CheckMatrixDimensions(B);
            Matrix X = new Matrix(M, N);
            double[][] C = X.A;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[i][j] = A[i][j] / B.A[i][j];
                }
            }
            return X;
        }

        // 行列要素毎の(右)除算, A = A./B
        // B: another matrix
        // return: AをA./Bで置き換え
        public Matrix ArrayRightDivideEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    A[i][j] = A[i][j] / B.A[i][j];
                }
            }
            return this;
        }

        // 行列要素毎の(左)除算, C = A.\B
        // B: another matrix
        // return: A.\B
        public Matrix ArrayLeftDivide(Matrix B)
        {
            CheckMatrixDimensions(B);
            Matrix X = new Matrix(M, N);
            double[][] C = X.A;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[i][j] = B.A[i][j] / A[i][j];
                }
            }
            return X;
        }

        // 行列要素毎の(左)除算, A = A.\B
        // B: another matrix
        // return: AをA.\Bで置き換え
        public Matrix ArrayLeftDivideEquals(Matrix B)
        {
            CheckMatrixDimensions(B);
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    A[i][j] = B.A[i][j] / A[i][j];
                }
            }
            return this;
        }

        // 行列のスカラ倍, C = s*A
        // s: scalar
        // return:  s*A
        public Matrix Times(double s)
        {
            Matrix X = new Matrix(M, N);
            double[][] C = X.A;
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    C[i][j] = s * A[i][j];
                }
            }
            return X;
        }

        // 行列のスカラ倍, A = s*A
        // s: scalar
        // return: Aをs*Aで置き換え
        public Matrix TimesEquals(double s)
        {
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    A[i][j] = s * A[i][j];
                }
            }
            return this;
        }

        // 行列の積, A*B
        // B: another matrix
        // return: A*B
        // 例外: ArgumentException "Matrix inner dimensions must agree"
        public Matrix Times(Matrix B)
        {
            if (B.M != N)
            {
                throw new ArgumentException("Matrix inner dimensions must agree.");
            }
            Matrix X = new Matrix(M, B.N);
            double[][] C = X.A;
            double[] Bcolj = new double[N];
            for (int j = 0; j < B.N; j++)
            {
                for (int k = 0; k < N; k++)
                {
                    Bcolj[k] = B.A[k][j];
                }
                for (int i = 0; i < M; i++)
                {
                    double[] Arowi = A[i];
                    double s = 0;
                    for (int k = 0; k < N; k++)
                    {
                        s += Arowi[k] * Bcolj[k];
                    }
                    C[i][j] = s;
                }
            }
            return X;
        }

        // LU分解
        // return: LUDecomposition
        // see LUDecomposition
        public LUDecomposition Lu()
        {
            return new LUDecomposition(this);
        }

        // QR分解
        // return: QRDecomposition
        // see QRDecomposition
        public QRDecomposition Qr()
        {
            return new QRDecomposition(this);
        }

        // コレスキ分解
        // return: CholeskyDecomposition
        // see CholeskyDecomposition
        public CholeskyDecomposition Chol()
        {
            return new CholeskyDecomposition(this);
        }

        // 特異値分解
        // return: SingularValueDecomposition
        // see SingularValueDecomposition
        public SingularValueDecomposition Svd()
        {
            return new SingularValueDecomposition(this);
        }

        // 固有値分解
        // return: EigenvalueDecomposition
        // see EigenvalueDecomposition
        public EigenvalueDecomposition Eig()
        {
            return new EigenvalueDecomposition(this);
        }

        // A*X = B の解を求める
        // B: 右辺のB
        // return: Aが正方の場合は方程式の解，正方でない場合は最小２乗解
        public Matrix Solve(Matrix B)
        {
            return M == N ? new LUDecomposition(this).Solve(B) :
                    new QRDecomposition(this).Solve(B);
        }

        // X*A = Bの解を求める, A'*X' = B'の解と同じ
        // B: 右辺のB
        // return: Aが正方の場合は方程式の解，正方でない場合は最小２乗解

        public Matrix SolveTranspose(Matrix B)
        {
            return Transpose().Solve(B.Transpose());
        }

        // 逆行列(擬似逆行列)を求める
        // return: Aが正方の場合逆行列，正方でない場合擬似逆行列
        public Matrix Inverse()
        {
            return Solve(Identity(M, M));
        }

        // 行列式の計算
        // return: 行列式の値
        public double Det()
        {
            return new LUDecomposition(this).Det();
        }

        // rankを求める
        // return: 行列の階数, SVDから求める
        public int Rank()
        {
            return new SingularValueDecomposition(this).Rank();
        }

        // 行列の条件数(2-ノルムに基づく)
        // return: 最大値特異値と最小特異値の比
        public double Cond()
        {
            return new SingularValueDecomposition(this).Cond();
        }

        // 行列のトレース
        // return: 対角要素の和
        public double Trace()
        {
            double t = 0;
            for (int i = 0; i < Min(M, N); i++)
            {
                t += A[i][i];
            }
            return t;
        }

        // ランダム値で行列を生成
        // m: 行数
        // n: 列数
        // return: ランダム値(一様乱数)を要素とするm行n列の行列
        public static Matrix Random(int m, int n)
        {
            Matrix A = new Matrix(m, n);
            Random rnd = new Random();
            double[][] X = A.A;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    X[i][j] = rnd.NextDouble();
                }
            }
            return A;
        }

        // 単位行列を生成
        // m: 行数
        // n: 列数
        // return: 対角要素を1とするm行n列の行列
        public static Matrix Identity(int m, int n)
        {
            Matrix A = new Matrix(m, n);
            double[][] X = A.A;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    X[i][j] = i == j ? 1.0 : 0.0;
                }
            }
            return A;
        }

        // 行列を標準出力に表示
        // w: 列の桁数
        // d: 小数点以下の桁数
        public void Print(int w, int d)
        {
            string fmt = $"{{0,{w}:F{d}}}  ";
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Console.Write(string.Format(fmt, A[i][j]));
                    //Console.Write(string.Format("{0,7:F3}", A[i][j]));
                    //Console.Write("{0:5F3}", A[i][j]);
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }


        // private method
        // check size(A) == size(B) 
        private void CheckMatrixDimensions(Matrix B)
        {
            if (B.M != M || B.N != N)
            {
                throw new ArgumentException("Matrix dimensions must agree.");
            }
        }
    }
}
