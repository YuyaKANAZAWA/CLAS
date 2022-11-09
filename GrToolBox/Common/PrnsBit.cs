using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static GrToolBox.Common.Constants;
using static GrToolBox.Common.CommonUtilities;

namespace GrToolBox.Common
{
    public sealed class PrnsBit
    {
        public BitArray Bit { get; set; } = new BitArray(MAX_SAT); 

        public PrnsBit() { }

        public int Cardinality()
        {
            int n = 0;
            foreach(bool bit in Bit)
            {
                if (bit) n++;
            }
            return n;
        }


        public void Set(int i)
        {
            Bit.Set(i, true);
        }

        public void Clear(int i)
        {
            Bit.Set(i, false);
        }

        public bool Get(int i)
        {
            return Bit.Get(i);
        }


        public int[] GetPrns()
        {
            var list = new List<int>();
            int n = 0;
            foreach (bool bit in Bit)
            {
                if (bit)
                {
                    list.Add(n);
                }
                n++;
            }
            return list.ToArray();
        }


        public string[] GetSnn()
        {
            string[] Snn = new string[this.Cardinality()];
            int prn = 0;
            int cnt = 0;
            foreach (bool b in Bit)
            {
                if (b)
                {
                    Snn[cnt] = Prn2Snn(prn);
                    cnt++;
                }
                prn++;
            }
            return Snn;
        }

        public string[] GetSnnString()
        {
            string[] Snn = new string[this.Cardinality()];
            int prn = 0;
            int cnt = 0;
            foreach (bool b in Bit)
            {
                if (b)
                {
                    Snn[cnt] = Prn2Snn(prn);
                    cnt++;
                }
                prn++;
            }
            return Snn;
        }

        // Snnを衛星システム毎のListに入れて返す
        public List<string>[] GetSnnLists()
        {
            List<string>[] SnnList = new List<string>[MAX_SYS];
            for(int i=0; i<MAX_SYS; i++)
            {
                SnnList[i] = new List<string>();
            }
            string[] snn = GetSnnString();
            foreach(string s in snn)
            {
                SnnList[GetISys(s[0])].Add(s);
            }
            return SnnList;
        }
    }
}
