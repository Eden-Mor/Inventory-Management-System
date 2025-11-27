using IMS_Backend.Models;
using IMS_Shared.Dtos;
using IMS_Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController(AppDbContext context) : ControllerBase
{
    //TODO: Make logs more detailed and useful


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
        await SaveAndLogAsync(LogType.Supplier_Added, $"Added new supplier: {sanitizedName}");

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
                Name = x.Name
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
        var logs = await context.Logs
            .OrderByDescending(x => x.Date)
            .Select(x => new LogDto
            {
                Id = x.Id,
                Date = x.Date,
                TypeEnum = x.TypeEnum,
                Description = x.Description
            })
            .ToListAsync();

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
        await SaveAndLogAsync(LogType.Stock_Added, $"Added new stock: {stock.Name}");

        model.StockId = stock.Id;

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
        await SaveAndLogAsync(LogType.Removed_Item, $"Removed stock: {stock.Name}");

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

        await SaveAndLogAsync(LogType.Edited_Item, $"Edited stock: {stock.Name}");

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

        await SaveAndLogAsync(LogType.Stock_Sold, $"Purchase created by {request.BuyerName} with {request.Items.Count} item(s).");

        return Ok(purchase);
    }

    // -------------------------------
    // GET ORDERS
    // -------------------------------
    [HttpGet("get-orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await context.SupplierOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items)
            .ThenInclude(i => i.Stock)
            .Select(o => new SupplierOrderDto
            {
                Id = o.Id,
                CreatedDate = o.CreatedDate,
                Status = o.Status,
                StatusChangeDate = o.StatusChangeDate,
                Supplier = new SupplierDto
                {
                    Id = o.Supplier.Id,
                    Name = o.Supplier.Name
                },
                Items = o.Items.Select(i => new StockDto
                {
                    StockId = i.Stock.Id,
                    Name = i.Stock.Name,
                    SerialNumber = i.Stock.SerialNumber,
                    BuyPrice = i.Stock.BuyPrice,
                    Amount = i.Amount
                }).ToList()
            })
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.StatusChangeDate ?? x.CreatedDate)
            .ToListAsync();

        return Ok(orders);
    }

    // -------------------------------
    // CREATE SUPPLIER ORDER
    // -------------------------------
    [HttpPost("create-supplier-order")]
    public async Task<IActionResult> CreateSupplierOrder(CreateSupplierOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest("At least one item must be included in the order.");

        if (dto.Items.Any(x=>x.Amount <= 0))
            return BadRequest("All items must have an amount greater than zero.");

        if (context.Suppliers.Find(dto.SupplierId) == null)
            return BadRequest("Supplier does not exist.");

        if (context.Stocks.Any(x => dto.Items.Select(i => i.StockId).Contains(x.Id)) == false)
            return BadRequest("One or more stock items do not exist.");

        var order = new SupplierOrder
        {
            CreatedDate = DateTime.UtcNow,
            SupplierId = dto.SupplierId,
            Status = OrderStatus.Pending,
            Items = dto.Items.Select(x => new SupplierOrderItem
            {
                StockId = x.StockId,
                Amount = x.Amount
            }).ToList()
        };

        context.SupplierOrders.Add(order);
        await SaveAndLogAsync(LogType.Supplier_Order_Added, $"Supplier Order created with {dto.Items.Count} item(s) to supplier {dto.SupplierId}.");

        return Ok(order.Id);
    }

    // -------------------------------
    // MARK ORDER RECEIVED
    // -------------------------------
    [HttpPost("mark-order-received")]
    public async Task<IActionResult> MarkOrderReceived([FromBody] int id)
    {
        var order = await context.SupplierOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        if (order.Status != OrderStatus.Pending)
            return BadRequest("Only pending orders can be marked as received.");

        order.SetStatus(OrderStatus.Received);

        foreach (var item in order.Items)
        {
            var stock = await context.Stocks.FindAsync(item.StockId);
            if (stock != null)
                stock.Amount += item.Amount;
        }

        await SaveAndLogAsync(LogType.Supplier_Order_Received, $"Supplier Order Received from supplier {order.SupplierId} with order number {id}.");

        return Ok();
    }

    // -------------------------------
    // CANCEL ORDER
    // -------------------------------
    [HttpPost("cancel-order")]
    public async Task<IActionResult> CancelOrder([FromBody] int id)
    {
        var order = await context.SupplierOrders.FindAsync(id);
        if (order == null)
            return NotFound();

        if (order.Status != OrderStatus.Pending)
            return BadRequest("Only pending orders can be canceled.");

        order.SetStatus(OrderStatus.Canceled);
        await SaveAndLogAsync(LogType.Supplier_Order_Canceled, $"Supplier Order Canceled from supplier {order.SupplierId} with order number {id}.");

        return Ok();
    }


    // -------------------------------
    // PRIVATE LOG CREATOR
    // -------------------------------
    private void CreateLog(LogType type, string desc)
    {
        context.Logs.Add(new Log
        {
            Date = DateTime.UtcNow,
            TypeEnum = type,
            Description = desc
        });
    }

    private async Task SaveAndLogAsync(LogType type, string desc)
    {
        CreateLog(type, desc);
        await context.SaveChangesAsync();
    }
}