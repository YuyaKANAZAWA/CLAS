using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrToolBox.Data.Rinex
{
    public static class RinexUtilities
    {
        /// <summary>
        /// 指数部が"D"で記録された行データからformatで指定された桁数で区切って読み取りdouble型配列で返す
        /// </summary>
        /// <param name="str">行データ</param>
        /// <param name="offset">位置"0"からの読み飛ばし桁数</param>
        /// <param name="format">読み取り桁数を指定する配列</param>
        /// <returns>double型配列</returns>
        public static double[] FormattedreadD(String str, int offset, int[] format)
        {
            int dim = format.Length;
            int start = offset;
            int end;
            double[] ret = new double[dim];
            string tmp;
            str = str.Replace('D', 'E');
            for (int i = 0; i < dim; i++)
            {
                end = start + format[i];
                if (end > str.Length) break;
                tmp = str[start..end].Trim();
                if (!string.IsNullOrEmpty(tmp))
                {
                    ret[i] = double.Parse(tmp);
                }
                start = end;
            }
            return ret;
        }

    }
}
