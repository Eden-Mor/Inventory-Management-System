using HTMLQuestPDF.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace IMS_Backend.Controllers;

[ApiController]
[Route("api/pdf")]
public class PdfController(AppDbContext context, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet("create")]
    public async Task<IActionResult> CreatePdf(int id)
    {
        var templatePath = Path.Combine(env.ContentRootPath, "Templates", "invoice-template.html");
        var html = System.IO.File.ReadAllText(templatePath);

        var purchase = await context.Purchases.FirstOrDefaultAsync(p => p.Id == id);
        if (purchase == null) {
            return NotFound();
        }

        // Replace placeholders
        html = html.Replace("{{CustomerName}}", purchase.BuyerName);
        html = html.Replace("{{OrderId}}", id.ToString());

        // Add image dynamically (see next section)
        html = html.Replace("{{ImageData}}", GetImageBase64());

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Content().HTML((x) => x.SetHtml(html));
            });
        }).GeneratePdf();

        return File(pdfBytes, "application/pdf", "invoice.pdf");
    }

    private string GetImageBase64()
    {
        var imagePath = Path.Combine(env.ContentRootPath, "Templates", "logo.png");
        var bytes = System.IO.File.ReadAllBytes(imagePath);
        return Convert.ToBase64String(bytes);
    }
}