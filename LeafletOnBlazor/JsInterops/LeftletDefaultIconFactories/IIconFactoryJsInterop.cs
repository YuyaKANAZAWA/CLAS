using Microsoft.JSInterop;

namespace LeafletOnBlazor.JsInterops.LeftletDefaultIconFactories
{
    public interface IIconFactoryJsInterop
    {
        ValueTask<IJSObjectReference> CreateDefaultIcon();
    }
}
