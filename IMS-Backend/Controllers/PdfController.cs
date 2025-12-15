using IMS_Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace IMS_Backend.Controllers;

[ApiController]
[Route("api/purchase")]
public class PdfController(AppDbContext context, IWebHostEnvironment env, IConfiguration config, IConverter converter) : ControllerBase
{
    [HttpGet("pdf")]
    public async Task<IActionResult> CreatePdf(int id)
    {
        var htmlFileName = config.GetValue<string>("PdfSettings:HtmlFileName");
        if (htmlFileName == null)
            return StatusCode(500, "PDF configuration is missing.");

        var templatePath = Path.Combine(env.ContentRootPath, "Templates", htmlFileName);
        var html = System.IO.File.ReadAllText(templatePath);

        var purchase = await context.Purchases
            .Include(x => x.Seller)
            .Include(x => x.Items)
            .ThenInclude(ip => ip.Stock)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null)
            return NotFound();

        var pendingStatusText = config.GetValue<string>("PdfSettings:WatermarkText") ?? "Pending";

        // Replace placeholders
        html = html.Replace("{{BusinessName}}", config.GetValue<string>("PdfSettings:BusinessName") ?? "Business Name");
        html = html.Replace("{{SellerName}}", purchase.Seller.Name);
        html = html.Replace("{{CustomerName}}", purchase.BuyerName);
        html = html.Replace("{{OrderId}}", id.ToString());

        if (purchase.Status == PurchaseStatus.Pending)
            html = html.Replace("{{WatermarkText}}", pendingStatusText);
        else
            html = html.Replace("{{WatermarkText}}", string.Empty);

        var dateTimeFormat = config.GetValue<string>("PdfSettings:DateFormat") ?? "MM/dd/yyyy HH:mm";
        html = html.Replace("{{PurchaseDate}}", purchase.PurchaseDate?.ToString(dateTimeFormat) ?? pendingStatusText);

        var taxRate = config.GetValue<decimal>("PdfSettings:TaxRate");

        decimal totalBeforeTax = 0;
        decimal totalTax = 0;

        StringBuilder orderItems = new();
        foreach (var item in purchase.Items)
        {
            var sellPrice = item.Stock?.SellPrice ?? 0;
            var itemPriceBeforeTax = sellPrice / (1 + taxRate);
            itemPriceBeforeTax = Math.Round(itemPriceBeforeTax, 2);
            var itemTaxPrice = sellPrice - itemPriceBeforeTax;
            orderItems.AppendLine($"<tr><td>{item.Stock?.Name ?? "UNKNOWN"}</td><td>{item.Amount}</td><td>{itemPriceBeforeTax}</td><td>{itemTaxPrice}</td><td>{sellPrice * item.Amount}</td></tr>");

            totalBeforeTax += itemPriceBeforeTax * item.Amount;
            totalTax += itemTaxPrice * item.Amount;
        }

        html = html.Replace("{{OrderItems}}", orderItems.ToString());
        html = html.Replace("{{TotalPriceWithoutTax}}", totalBeforeTax.ToString());
        html = html.Replace("{{TaxAmount}}", totalTax.ToString());
        html = html.Replace("{{TotalPrice}}", (totalBeforeTax + totalTax).ToString());

        var logoFileName = config.GetValue<string>("PdfSettings:LogoFileName");
        if (logoFileName != null)
            html = html.Replace("{{ImageData}}", GetTemplateFileBase64(logoFileName));

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4Rotated,
            },
            Objects = {
                new ObjectSettings() {
                    PagesCount = true,
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" },
                }
            }
        };

        byte[] pdfBytes = converter.Convert(doc);

        return File(pdfBytes, "application/pdf", "invoice.pdf");
    }

    private string GetTemplateFileBase64(string fileName)
    {
        var imagePath = Path.Combine(env.ContentRootPath, "Templates", fileName);
        if (!System.IO.File.Exists(imagePath))
            return string.Empty;

        var bytes = System.IO.File.ReadAllBytes(imagePath);
        return Convert.ToBase64String(bytes);
    }
}