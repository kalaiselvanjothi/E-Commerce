namespace ShopVerse.Application.Interfaces;

public interface IInvoiceService
{
    Task<byte[]> GenerateInvoicePdfAsync(Guid userId, Guid orderId);
}
