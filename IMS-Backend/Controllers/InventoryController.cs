using IMS_Backend.Models;
using IMS_Shared.Dtos;
using IMS_Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Text;

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
        await SaveAndLogAsync(LogType.Supplier_Added, $"Added new supplier: {sanitizedName}");

        return Ok(supplier.Id);
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

        StringBuilder sb = new();
        sb.AppendLine($"Added new stock: {stock.Name}");
        sb.AppendLine($" - Serial Number: {stock.SerialNumber}");
        sb.AppendLine($" - Buy Price: {stock.BuyPrice}");
        sb.AppendLine($" - Sell Price: {stock.SellPrice}");
        sb.AppendLine($" - Supplier ID: {stock.SupplierId}");
        sb.AppendLine($" - Initial Amount: {stock.Amount}");

        await SaveAndLogAsync(LogType.Stock_Added, sb.ToString());

        return Ok(stock.Id);
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

        StringBuilder sb = new();
        sb.AppendLine($"Removed stock: {stock.Name}");
        sb.AppendLine($" - Serial Number: {stock.SerialNumber}");
        sb.AppendLine($" - Buy Price: {stock.BuyPrice}");
        sb.AppendLine($" - Sell Price: {stock.SellPrice}");
        sb.AppendLine($" - Supplier ID: {stock.SupplierId}");
        sb.AppendLine($" - Amount: {stock.Amount}");

        await SaveAndLogAsync(LogType.Removed_Item, sb.ToString());

        return Ok(id);
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

        if (updated.Amount.HasValue && updated.Amount.Value < 0)
            return BadRequest("Amount cannot be set to a negative number.");

        StringBuilder sb = new();
        sb.AppendLine($"Edited stock: {stock.Name}");

        if (updated.Amount.HasValue)
            stock.Amount = LogIfChanged(sb, stock.Amount, updated.Amount.Value);

        stock.Name = LogIfChanged(sb, stock.Name, updated.Name);
        stock.SerialNumber = LogIfChanged(sb, stock.SerialNumber, updated.SerialNumber);
        stock.BuyPrice = LogIfChanged(sb, stock.BuyPrice, updated.BuyPrice);
        stock.SellPrice = LogIfChanged(sb, stock.SellPrice, updated.SellPrice);
        stock.SupplierId = LogIfChanged(sb, stock.SupplierId, updated.SupplierId);

        await SaveAndLogAsync(LogType.Edited_Item, sb.ToString());

        return Ok(id);
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

        var seller = await context.Sellers.FindAsync(request.SellerId);
        if (seller is null)
            return BadRequest("Seller does not exist.");

        if (seller.Status != SellerStatus.Active)
            return BadRequest("Seller is not active.");

        // Fetch and track stocks
        var stockIds = request.Items.Select(i => i.StockId).Distinct().ToList();
        var stocks = await context.Stocks
            .Where(s => stockIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        // Validate stock availability
        foreach (var item in request.Items)
        {
            if (!stocks.TryGetValue(item.StockId, out var stock))
                return BadRequest($"Stock ID {item.StockId} does not exist.");

            if (stock.Amount < item.Amount)
                return BadRequest($"Not enough stock available for {stock.Name} (currently {stock.Amount}, requested {item.Amount}).\nPlease reload the page.");
        }

        // Create purchase
        var purchase = new Purchase
        {
            SellerId = request.SellerId,
            BuyerName = request.BuyerName,
            Items = request.Items.Select(i => new ItemPurchase
            {
                StockId = i.StockId,
                Amount = i.Amount
            }).ToList()
        };

        context.Purchases.Add(purchase);

        // Deduct inventory
        foreach (var item in request.Items)
        {
            var stock = stocks[item.StockId];
            stock.Amount -= item.Amount;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Created purchase for buyer: {request.BuyerName}, seller: {seller.Name}");
        foreach (var s in request.Items)
            sb.AppendLine($" - Stock: {stocks[s.StockId]?.Name ?? "UNKNOWN STOCK"} (ID: {s.StockId}), Amount: {s.Amount}");

        await SaveAndLogAsync(LogType.Stock_Sold, sb.ToString());

        return Ok(purchase.Id);
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

        if (dto.Items.Any(x => x.Amount <= 0))
            return BadRequest("All items must have an amount greater than zero.");

        var supplier = context.Suppliers.Find(dto.SupplierId);
        if (supplier == null)
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

        StringBuilder sb = new();
        sb.AppendLine($"Created Supplier Order to {supplier.Name} (ID: {dto.SupplierId}) with the following items:");
        foreach (var item in dto.Items)
            sb.AppendLine($" - Stock ID: {item.StockId}, Amount: {item.Amount}");

        await SaveAndLogAsync(LogType.Supplier_Order_Added, sb.ToString());

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
            .Include(o => o.Supplier)
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

        await SaveAndLogAsync(LogType.Supplier_Order_Received, $"Supplier Order Received from supplier {order.Supplier.Name} (ID: {order.SupplierId}) with order number {id}.");

        return Ok(id);
    }

    // -------------------------------
    // CANCEL ORDER
    // -------------------------------
    [HttpPost("cancel-order")]
    public async Task<IActionResult> CancelOrder([FromBody] int id)
    {
        var order = await context.SupplierOrders
            .Include(o => o.Supplier)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        if (order.Status != OrderStatus.Pending)
            return BadRequest("Only pending orders can be canceled.");

        order.SetStatus(OrderStatus.Canceled);
        await SaveAndLogAsync(LogType.Supplier_Order_Canceled, $"Supplier Order Canceled from supplier {order.Supplier.Name} (ID: {order.SupplierId}) with order number {id}.");

        return Ok(id);
    }

    // -------------------------------
    // GET SELLERS
    // -------------------------------
    [HttpGet("sellers")]
    public async Task<IActionResult> GetSellers()
    {
        var sellers = await context.Sellers
            .Select(x => new SellerDto
            {
                Id = x.Id,
                Name = x.Name,
                Status = x.Status
            })
            .ToListAsync();

        return Ok(sellers);
    }

    // -------------------------------
    // GET ACTIVE SELLERS
    // -------------------------------
    [HttpGet("active-sellers")]
    public async Task<IActionResult> GetActiveSellers()
    {
        var sellers = await context.Sellers
            .Where(seller => seller.Status == SellerStatus.Active)
            .Select(x => new SellerDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync();

        return Ok(sellers);
    }

    // -------------------------------
    // ADD SELLER
    // -------------------------------
    [HttpPost("add-seller")]
    public async Task<IActionResult> AddSeller(SellerDto seller)
    {
        context.Sellers.Add(new Seller
        {
            Name = seller.Name,
            Status = SellerStatus.Active
        });

        await SaveAndLogAsync(LogType.Seller_Added, $"Seller \"{seller.Name}\" has been added with status {seller.Status}.");

        return Ok(seller.Id);
    }


    // -------------------------------
    // SELLER STATUS CHANGE
    // -------------------------------
    [HttpPost("set-seller-status")]
    public async Task<IActionResult> SetSellerStatus(SellerDto seller)
    {
        if (seller.Id <= 0)
            return BadRequest("Invalid seller ID.");

        var existingSeller = await context.Sellers.FindAsync(seller.Id);
        if (existingSeller == null)
            return NotFound("Seller not found.");

        if (existingSeller.Status == seller.Status)
            return BadRequest("Seller already has the specified status.");

        var currentStatus = existingSeller.Status;
        existingSeller.Status = seller.Status;

        await SaveAndLogAsync(LogType.Seller_Status_Changed, $"Seller \"{existingSeller.Name}\" status has been changed from {currentStatus} to {seller.Status}.");

        return Ok(seller.Id);
    }

    // -------------------------------
    // GET PURCHASES
    // -------------------------------
    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchases()
    {
        var purchases = await context.Purchases
            .Include(p => p.Items)
            .ThenInclude(i => i.Stock)
            .Select(x => new PurchaseResponseDto
            {
                Id = x.Id,
                SellerId = x.SellerId,
                Items = x.Items.Select(i => new PurchaseItemResponseDto
                {
                    StockName = i.Stock != null ? i.Stock.Name : string.Empty,
                    Amount = i.Amount
                }).ToList(),
                PurchaseDate = x.PurchaseDate,
                BuyerName = x.BuyerName
            })
            .OrderByDescending(x=>x.Id)
            .ToListAsync();

        return Ok(purchases);
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

    private static T LogIfChanged<T>(
        StringBuilder sb,
        T currentValue,
        T newValue,
        [CallerArgumentExpression(nameof(currentValue))] string propertyName = null)
    {
        bool changed = !Equals(currentValue, newValue);
        if (changed)
            sb.AppendLine($" - {propertyName?.Split('.').Last() ?? "UNKNOWN PROPERTY"} changed from: {currentValue} to {newValue}");

        return newValue;
    }
}