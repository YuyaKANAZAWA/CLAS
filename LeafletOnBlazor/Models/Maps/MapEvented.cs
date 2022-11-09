using LeafletOnBlazor.JsInterops.Events;
using Microsoft.JSInterop;

namespace LeafletOnBlazor
{
    public class MapEvented : Evented
    {
        public MapEvented(IJSObjectReference jsReference, IEventedJsInterop eventedJsInterop)
        {
            JsReference = jsReference;
            EventedJsInterop = eventedJsInterop;
        }
    }
}
