using IMS_Shared.Dtos;

namespace IMS.Helpers;

public class InventoryApiClient
{
    private readonly HttpClient _http;

    public InventoryApiClient(HttpClient httpClient)
    {
        _http = httpClient;
        _http.BaseAddress ??= new Uri("https://localhost:5001"); // change to your API base URL
    }

    // POST /api/Inventory/add-supplier
    public async Task<HttpResponseMessage> AddSupplierAsync(SupplierDto dto)
        => await _http.PostAsJsonAsync("/api/Inventory/add-supplier", dto);

    // GET /api/Inventory/get-suppliers
    public async Task<List<SupplierDto>?> GetSuppliersAsync()
        => await _http.GetFromJsonAsync<List<SupplierDto>>("/api/Inventory/get-suppliers");

    // GET /api/Inventory/get-all-stocks
    public async Task<List<StockDto>?> GetAllStocksAsync()
        => await _http.GetFromJsonAsync<List<StockDto>>("/api/Inventory/get-all-stocks");

    // GET /api/Inventory/get-logs
    public async Task<HttpResponseMessage> GetLogsAsync()
        => await _http.GetAsync("/api/Inventory/get-logs");

    // POST /api/Inventory/add-stock
    public async Task<HttpResponseMessage> AddStockAsync(StockDto dto)
        => await _http.PostAsJsonAsync("/api/Inventory/add-stock", dto);

    // DELETE /api/Inventory/remove-stock/{id}
    public async Task<HttpResponseMessage> RemoveStockAsync(int id)
        => await _http.DeleteAsync($"/api/Inventory/remove-stock/{id}");

    // PUT /api/Inventory/edit-stock/{id}
    public async Task<HttpResponseMessage> EditStockAsync(int id, StockDto dto)
        => await _http.PutAsJsonAsync($"/api/Inventory/edit-stock/{id}", dto);

    // POST /api/Inventory/create-purchase
    public async Task<HttpResponseMessage> CreatePurchaseAsync(PurchaseRequestDto dto)
        => await _http.PostAsJsonAsync("/api/Inventory/create-purchase", dto);
}

