using Microsoft.JSInterop;

namespace LeafletOnBlazor
{
    public class Icon : JsReferenceBase
    {
        public Icon(IJSObjectReference jsReference)
        {
            JsReference = jsReference;
        }
    }
}
