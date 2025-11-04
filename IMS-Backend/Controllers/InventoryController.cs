using IMS_Backend.Models;
using IMS_Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController(AppDbContext context) : ControllerBase
{

    // -------------------------------
    // ADD SUPPLIER
    // -------------------------------
    [HttpPost("add-supplier")]
    public async Task<IActionResult> AddSupplier([FromBody] SupplierDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            return BadRequest("Supplier name is required.");

        var sanitizedName = model.Name.Trim();
        bool exists = await context.Suppliers
            .AnyAsync(s => s.Name.ToLower() == sanitizedName.ToLower());

        if (exists)
            return BadRequest("A supplier with that name already exists.");

        var supplier = new Supplier
        {
            Name = sanitizedName
        };

        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();

        await CreateLog(LogType.Supplier_Added, $"Added new supplier: {sanitizedName}");
        return Ok(supplier);
    }

    // -------------------------------
    // GET SUPPLIERS
    // -------------------------------
    [HttpGet("get-suppliers")]
    public async Task<IActionResult> GetSuppliers()
    {
        var suppliers = await context.Suppliers
            .Select(x => new SupplierDto() 
            { 
                Id = x.Id, 
                Name= x.Name
            })
            .ToListAsync();
        
        return Ok(suppliers);
    }

    // -------------------------------
    // GET ALL STOCKS
    // -------------------------------
    [HttpGet("get-all-stocks")]
    public async Task<IActionResult> GetAllStocks()
    {
        var stocks = await context.Stocks
            .Select(x => new StockDto
            {
                StockId = x.Id,
                Amount = x.Amount,
                BuyPrice = x.BuyPrice,
                Name = x.Name,
                SellPrice = x.SellPrice,
                SerialNumber = x.SerialNumber,
                SupplierId = x.SupplierId
            })
            .ToListAsync();
        return Ok(stocks);
    }

    // -------------------------------
    // GET ALL LOGS
    // -------------------------------
    [HttpGet("get-logs")]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await context.Logs.ToListAsync();
        return Ok(logs);
    }

    // -------------------------------
    // ADD STOCK
    // -------------------------------
    [HttpPost("add-stock")]
    public async Task<IActionResult> AddStock([FromBody] StockDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            return BadRequest("Stock name is required.");

        // Create stock and link it to the inventory
        var stock = new Stock
        {
            Name = model.Name,
            SerialNumber = model.SerialNumber,
            BuyPrice = model.BuyPrice,
            SellPrice = model.SellPrice,
            SupplierId = model.SupplierId,
            Amount = model.Amount ?? 0
        };

        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        model.StockId = stock.Id;

        await CreateLog(LogType.Stock_Added, $"Added new stock: {stock.Name}");
        return Ok(model);
    }

    // -------------------------------
    // REMOVE STOCK
    // -------------------------------
    [HttpDelete("remove-stock/{id}")]
    public async Task<IActionResult> RemoveStock(int id)
    {
        var stock = await context.Stocks.FindAsync(id);
        if (stock == null)
            return NotFound("Stock not found.");

        context.Stocks.Remove(stock);
        await context.SaveChangesAsync();

        await CreateLog(LogType.Removed_Item, $"Removed stock: {stock.Name}");
        return Ok();
    }

    // -------------------------------
    // EDIT STOCK
    // -------------------------------
    [HttpPut("edit-stock/{id}")]
    public async Task<IActionResult> EditStock(int id, [FromBody] StockDto updated)
    {
        var stock = await context.Stocks.FindAsync(id);
        if (stock == null)
            return NotFound("Stock not found.");

        stock.Name = updated.Name;
        stock.SerialNumber = updated.SerialNumber;
        stock.BuyPrice = updated.BuyPrice;
        stock.SellPrice = updated.SellPrice;
        stock.SupplierId = updated.SupplierId;

        if (updated.Amount.HasValue && updated.Amount.Value < 0)
            return BadRequest("Amount cannot be set to a negative number.");

        if (updated.Amount.HasValue)
            stock.Amount = updated.Amount.Value;

        await context.SaveChangesAsync();

        await CreateLog(LogType.Edited_Item, $"Edited stock: {stock.Name}");
        return Ok(stock);
    }

    // -------------------------------
    // ADD INVENTORY
    // -------------------------------
    [HttpPost("add-inventory")]
    public async Task<IActionResult> AddInventory([FromBody] InventoryDto model)
    {
        var stock = await context.Stocks.FindAsync(model.StockId);
        if (stock == null)
            return NotFound("Stock not found.");

        stock.Amount += model.Amount;

        await context.SaveChangesAsync();
        await CreateLog(LogType.Added_Item, $"Added {model.Amount} to inventory for {stock.Name}");
        return Ok(stock);
    }

    // -------------------------------
    // CREATE PURCHASE
    // -------------------------------
    [HttpPost("create-purchase")]
    public async Task<IActionResult> CreatePurchase([FromBody] PurchaseRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.BuyerName))
            return BadRequest("Buyer name is required.");

        if (request.Items == null || request.Items.Count == 0)
            return BadRequest("At least one item must be included in the purchase.");

        // Check all stock exists and has enough inventory
        foreach (var item in request.Items)
        {
            var stock = await context.Stocks.FirstOrDefaultAsync(i => i.Id == item.StockId);
            if (stock == null || stock.Amount < item.Amount)
                return BadRequest($"Not enough stock available for StockId {item.StockId}.");
        }

        // Create purchase
        var purchase = new Purchase
        {
            BuyerName = request.BuyerName,
            Items = [.. request.Items.Select(i => new ItemPurchase
            {
                StockId = i.StockId,
                Amount = i.Amount
            })]
        };

        context.Purchases.Add(purchase);

        // Deduct inventory
        foreach (var item in request.Items)
        {
            var inv = await context.Stocks.FirstAsync(i => i.Id == item.StockId);
            inv.Amount -= item.Amount;
        }

        await context.SaveChangesAsync();

        await CreateLog(LogType.Stock_Sold, $"Purchase created by {request.BuyerName} with {request.Items.Count} item(s).");
        return Ok(purchase);
    }

    // -------------------------------
    // PRIVATE LOG CREATOR
    // -------------------------------
    private async Task CreateLog(LogType type, string desc)
    {
        context.Logs.Add(new Log
        {
            Date = DateTime.UtcNow,
            TypeEnum = type,
            Description = desc
        });
        await context.SaveChangesAsync();
    }
}