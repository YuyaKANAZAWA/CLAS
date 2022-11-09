using LeafletOnBlazor.JsInterops.Base;
using Microsoft.JSInterop;

namespace LeafletOnBlazor.JsInterops.Maps
{
    public interface IMapJsInterop : IBaseJsInterop
    {
        ValueTask<IJSObjectReference> Initialize(string id, MapOptions mapOptions);
    }
}
