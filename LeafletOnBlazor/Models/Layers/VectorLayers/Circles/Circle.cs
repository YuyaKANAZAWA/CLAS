using LeafletOnBlazor.JsInterops.Events;
using Microsoft.JSInterop;

namespace LeafletOnBlazor
{
    /// <summary>
    /// A class for drawing circle overlays on a map. Extends CircleMarker.
    /// </summary>
    public class Circle : CircleMarker
    {
        public Circle(IJSObjectReference jsReference, IEventedJsInterop eventedJsInterop)
            : base(jsReference, eventedJsInterop)
        {
        }
    }
}
