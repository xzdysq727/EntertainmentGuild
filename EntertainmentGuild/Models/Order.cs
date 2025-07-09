public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string ShippingMethod { get; set; }
    public string PaymentMethod { get; set; }
    public int AddressId { get; set; }
}
