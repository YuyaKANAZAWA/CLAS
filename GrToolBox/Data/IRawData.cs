using GrToolBox.Data.GrNavData;
using GrToolBox.Satellite;
using System.Text;

namespace GrToolBox.Data
{
    public interface IRawData
    {
        int GetID();
        Satellites? GetMeas();
        GrNavBase? GetNav();
        void PrintTo(StringBuilder sb);
    }
}
