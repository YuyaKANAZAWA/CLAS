namespace LeafletOnBlazor
{
    public interface IRectangleFactory
    {
        Task<Rectangle> CreateRectangle(LatLngBounds latLngBounds, RectangleOptions? options);
        Task<Rectangle> CreateRectangleAndAddToMap(LatLngBounds latLngBounds, Map map, RectangleOptions? options);
    }
}
