using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafletOnBlazor
{
    public class CircleMarkerOptions : PathOptions
    {
        public double Radius { get; init; } = 10d;
    }
}
