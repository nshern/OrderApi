public class OrderEvents
{
    public delegate void OrderPlacedHandler(Order order);
    public event OrderPlacedHandler OrderPlaced;

    public void RaiseOrderPlaced(Order order)
    {
        OrderPlaced?.Invoke(order);
    }
}
