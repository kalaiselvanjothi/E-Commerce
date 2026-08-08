using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShopVerse.Application.Interfaces;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class PdfInvoiceService : IInvoiceService
{
    private readonly ShopVerseDbContext _context;

    public PdfInvoiceService(ShopVerseDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new KeyNotFoundException("Order not found.");

        var payment = order.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        var invoiceNumber = $"INV-{order.CreatedAt:yyyyMMdd}-{order.OrderNumber.Replace("SV", "")}";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                // ── HEADER ───────────────────────────────────────────────────
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(brandCol =>
                        {
                            brandCol.Item().Text("SHOPVERSE").FontSize(22).Bold().FontColor("#4F46E5");
                            brandCol.Item().Text("Premium Shopping Platform").FontSize(9).FontColor(Colors.Grey.Medium);
                            brandCol.Item().Text("www.shopverse.com | support@shopverse.com").FontSize(8).FontColor(Colors.Grey.Medium);
                        });

                        row.RelativeItem().AlignRight().Column(invCol =>
                        {
                            invCol.Item().Text("TAX INVOICE").FontSize(18).Bold().FontColor(Colors.Grey.Darken4);
                            invCol.Item().Text($"Invoice #: {invoiceNumber}").FontSize(10).Bold().FontColor("#4F46E5");
                            invCol.Item().Text($"Order #: {order.OrderNumber}").FontSize(9);
                            invCol.Item().Text($"Date: {order.CreatedAt:MMM dd, yyyy}").FontSize(9);
                        });
                    });

                    col.Item().PaddingTop(12).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // ── CONTENT ──────────────────────────────────────────────────
                page.Content().PaddingVertical(16).Column(col =>
                {
                    // Addresses Grid
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(billCol =>
                        {
                            billCol.Item().Text("CUSTOMER DETAILS").FontSize(9).Bold().FontColor(Colors.Grey.Medium);
                            billCol.Item().Text($"{order.User.FirstName} {order.User.LastName}").Bold();
                            billCol.Item().Text(order.User.Email!);
                            if (!string.IsNullOrEmpty(order.User.PhoneNumber))
                                billCol.Item().Text($"Phone: {order.User.PhoneNumber}");
                        });

                        row.RelativeItem().Column(shipCol =>
                        {
                            shipCol.Item().Text("SHIPPING ADDRESS").FontSize(9).Bold().FontColor(Colors.Grey.Medium);
                            if (order.ShippingAddress != null)
                            {
                                shipCol.Item().Text(order.ShippingAddress.FullName).Bold();
                                shipCol.Item().Text($"{order.ShippingAddress.Line1}, {order.ShippingAddress.Line2}");
                                shipCol.Item().Text($"{order.ShippingAddress.City}, {order.ShippingAddress.State} - {order.ShippingAddress.PostalCode}");
                                shipCol.Item().Text($"Phone: {order.ShippingAddress.Phone}");
                            }
                        });

                        row.RelativeItem().Column(payCol =>
                        {
                            var isPaid = payment?.Status == Domain.Enums.PaymentStatus.Completed || order.Payments.Any(p => p.Status == Domain.Enums.PaymentStatus.Completed);

                            payCol.Item().Text("PAYMENT INFORMATION").FontSize(9).Bold().FontColor(Colors.Grey.Medium);
                            payCol.Item().Text($"Method: {payment?.Method.ToString() ?? "Online Payment / Razorpay"}").Bold();
                            payCol.Item().Text($"Status: {(isPaid ? "Completed (Success)" : "Pending")}").Bold().FontColor(isPaid ? Colors.Green.Medium : Colors.Orange.Medium);
                            if (!string.IsNullOrEmpty(payment?.RazorpayPaymentId))
                                payCol.Item().Text($"Payment ID: {payment.RazorpayPaymentId}").FontSize(8);
                        });
                    });

                    col.Item().PaddingTop(20);

                    // Line Items Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(4);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#F8FAFC").Padding(6).Text("#").Bold().FontSize(9);
                            header.Cell().Background("#F8FAFC").Padding(6).Text("Product Description").Bold().FontSize(9);
                            header.Cell().Background("#F8FAFC").Padding(6).AlignRight().Text("Qty").Bold().FontSize(9);
                            header.Cell().Background("#F8FAFC").Padding(6).AlignRight().Text("Unit Price").Bold().FontSize(9);
                            header.Cell().Background("#F8FAFC").Padding(6).AlignRight().Text("Amount").Bold().FontSize(9);
                        });

                        int index = 1;
                        foreach (var item in order.Items)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6).Text(index++.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6).Column(itemCol =>
                            {
                                itemCol.Item().Text(item.ProductName).Bold().FontSize(9);
                                if (!string.IsNullOrEmpty(item.VariantInfo))
                                    itemCol.Item().Text($"Variant: {item.VariantInfo}").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6).AlignRight().Text(item.Quantity.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6).AlignRight().Text($"₹{item.UnitPrice:N2}").FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6).AlignRight().Text($"₹{item.TotalPrice:N2}").Bold().FontSize(9);
                        }
                    });

                    col.Item().PaddingTop(16);

                    // Financial Summary Box
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(2);
                        row.RelativeItem(2).Column(sumCol =>
                        {
                            sumCol.Item().Row(r => { r.RelativeItem().Text("Subtotal:"); r.RelativeItem().AlignRight().Text($"₹{order.SubTotal:N2}"); });
                            if (order.DiscountAmount > 0)
                            {
                                sumCol.Item().Row(r => { r.RelativeItem().Text($"Discount ({order.CouponCode ?? "Promo"}):").FontColor(Colors.Green.Medium); r.RelativeItem().AlignRight().Text($"-₹{order.DiscountAmount:N2}").FontColor(Colors.Green.Medium); });
                            }
                            sumCol.Item().Row(r => { r.RelativeItem().Text("Shipping Charges:"); r.RelativeItem().AlignRight().Text(order.ShippingAmount > 0 ? $"₹{order.ShippingAmount:N2}" : "FREE"); });
                            sumCol.Item().Row(r => { r.RelativeItem().Text("GST Tax (18%):"); r.RelativeItem().AlignRight().Text($"₹{order.TaxAmount:N2}"); });

                            sumCol.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                            sumCol.Item().PaddingTop(6).Row(r => { r.RelativeItem().Text("Grand Total:").FontSize(12).Bold(); r.RelativeItem().AlignRight().Text($"₹{order.TotalAmount:N2}").FontSize(12).Bold().FontColor("#4F46E5"); });
                        });
                    });
                });

                // ── FOOTER ───────────────────────────────────────────────────
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Thank you for shopping with ShopVerse! This is a computer-generated tax invoice.").FontSize(8).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.DefaultTextStyle(t => t.FontSize(8).FontColor(Colors.Grey.Medium));
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
