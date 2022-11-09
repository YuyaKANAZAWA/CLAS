using GrToolBox.Data;
using GrToolBox.Common;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");



byte[] b1 = { 0b_0000_1111 };

BitCircularBuffer bcb = new BitCircularBuffer(2);


int c = bcb.Count();
int n = bcb.NumWritable();
bcb.AddByteArray(b1, 0, 8);
c = bcb.Count();
n = bcb.NumWritable();

bcb.AddByteArray(b1, 0, 8);
bcb.AddByteArray(b1, 0, 8);

int iii = bcb.GetInt(6);
iii = bcb.GetInt(6);

bcb.AddByteArray(b1, 0, 8);
bcb.AddByteArray(b1, 0, 8);
c = bcb.Count();
int cc = bcb.Count();
var kk = bcb.GetInt(6);
kk = bcb.GetInt(3);

Console.WriteLine();
Console.WriteLine();
