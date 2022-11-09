using Microsoft.JSInterop;

namespace LeafletOnBlazor.JsInterops.Events
{
    public interface IEventedJsInterop
    {
        ValueTask OnCallback(DotNetObjectReference<Evented> eventedClass, IJSObjectReference eventedReference, string eventType);
    }
}
