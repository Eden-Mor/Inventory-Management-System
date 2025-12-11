using IMS_Shared.Dtos;
using System.Net.Http;

namespace IMS.Helpers;

public class InventoryApiClient
{
    private readonly HttpClient _http;

    public InventoryApiClient(HttpClient httpClient)
    {
        _http = httpClient;
        _http.BaseAddress ??= new Uri("https://localhost:5001"); // change to your API base URL
    }

    private static async Task<ApiResult<TResponse>> HandleResponse<TResponse>(HttpResponseMessage msg)
    {
        if (!msg.IsSuccessStatusCode)
        {
            string err = await msg.Content.ReadAsStringAsync();
            return ApiResult<TResponse>.Fail(err);
        }

        if (typeof(TResponse) == typeof(byte[]))
        {
            var dataBytes = await msg.Content.ReadAsByteArrayAsync();
            if (dataBytes is not TResponse dataObj)
                return ApiResult<TResponse>.Fail("Failed to convert byte array to the specified type.");

            return ApiResult<TResponse>.Ok(dataObj);
        }

        var data = await msg.Content.ReadFromJsonAsync<TResponse>();
        return ApiResult<TResponse>.Ok(data);
    }

    public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string url)
    {
        var msg = await _http.GetAsync(url);
        return await HandleResponse<TResponse>(msg);
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        var msg = await _http.PostAsJsonAsync(url, body);
        return await HandleResponse<TResponse>(msg);
    }

    public async Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest body)
    {
        var msg = await _http.PutAsJsonAsync(url, body);
        return await HandleResponse<TResponse>(msg);
    }

    public async Task<ApiResult<TResponse>> DeleteAsync<TRequest, TResponse>(string url, TRequest body)
    {
        var msg = await _http.DeleteAsync(url);
        return await HandleResponse<TResponse>(msg);
    }

    // POST /api/Inventory/add-supplier
    public async Task<ApiResult<int>> AddSupplierAsync(SupplierDto dto)
        => await PostAsync<SupplierDto, int>("/api/Inventory/add-supplier", dto);

    // GET /api/Inventory/get-suppliers
    public async Task<ApiResult<List<SupplierDto>>> GetSuppliersAsync()
        => await GetAsync<List<SupplierDto>>("/api/Inventory/get-suppliers");

    // GET /api/Inventory/get-all-stocks
    public async Task<ApiResult<List<StockDto>>> GetAllStocksAsync()
        => await GetAsync<List<StockDto>>("/api/Inventory/get-all-stocks");

    // GET /api/Inventory/get-logs
    public async Task<ApiResult<List<LogDto>>> GetLogsAsync()
        => await GetAsync<List<LogDto>>("/api/Inventory/get-logs");

    // POST /api/Inventory/add-stock
    public async Task<ApiResult<int>> AddStockAsync(StockDto dto)
        => await PostAsync<StockDto, int>("/api/Inventory/add-stock", dto);

    // DELETE /api/Inventory/remove-stock/{id}
    public async Task<ApiResult<int>> RemoveStockAsync(int id)
        => await DeleteAsync<string, int>($"/api/Inventory/remove-stock/{id}", string.Empty);

    // PUT /api/Inventory/edit-stock/{id}
    public async Task<ApiResult<int>> EditStockAsync(int id, StockDto dto)
        => await PutAsync<StockDto, int>($"/api/Inventory/edit-stock/{id}", dto);

    // POST /api/Inventory/create-purchase
    public async Task<ApiResult<int>> CreatePurchaseAsync(PurchaseRequestDto dto)
        => await PostAsync<PurchaseRequestDto, int>("/api/Inventory/create-purchase", dto);

    // GET /api/Inventory/get-orders
    public async Task<ApiResult<List<SupplierOrderDto>>> GetSupplierOrdersAsync()
        => await GetAsync<List<SupplierOrderDto>>("/api/Inventory/get-orders");

    // POST /api/Inventory/mark-order-received
    public async Task<ApiResult<int>> MarkOrderReceivedAsync(int id)
        => await PostAsync<int, int>($"/api/Inventory/mark-order-received", id);

    // POST /api/Inventory/cancel-order
    public async Task<ApiResult<int>> CancelOrderAsync(int id)
        => await PostAsync<int, int>("/api/Inventory/cancel-order", id);

    // POST /api/Inventory/create-supplier-order
    public async Task<ApiResult<int>> CreateSupplierOrderAsync(CreateSupplierOrderDto dto)
        => await PostAsync<CreateSupplierOrderDto, int>("/api/Inventory/create-supplier-order", dto);

    // GET /api/Inventory/sellers
    public async Task<ApiResult<List<SellerDto>>> GetSellersAsync()
        => await GetAsync<List<SellerDto>>("/api/Inventory/sellers");

    // GET /api/Inventory/active-sellers
    public async Task<ApiResult<List<SellerDto>>> GetActiveSellersAsync()
        => await GetAsync<List<SellerDto>>("/api/Inventory/active-sellers");

    // POST /api/Inventory/add-seller
    public async Task<ApiResult<int>> AddSellerAsync(SellerDto dto)
        => await PostAsync<SellerDto, int>("/api/Inventory/add-seller", dto);

    // POST /api/Inventory/set-seller-status
    public async Task<ApiResult<int>> ChangeSellerStatusAsync(SellerDto dto)
        => await PostAsync<SellerDto, int>("/api/Inventory/set-seller-status", dto);

    // GET /api/Inventory/active-sellers
    public async Task<ApiResult<List<PurchaseResponseDto>>> GetPurchasesAsync()
        => await GetAsync<List<PurchaseResponseDto>>("/api/Inventory/purchases");

    // GET /api/purchase/pdf
    public async Task<ApiResult<byte[]>> GetPurchaseReceipt(int id)
        => await GetAsync<byte[]>($"/api/purchase/pdf/?id={id}");
}

public interface IApiResult 
{
    bool Success { get; set; }
    string Error { get; set; }
}

public class ApiResult<T> : IApiResult
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Error { get; set; } = string.Empty;

    public static ApiResult<T> Ok(T? data) => new() { Success = true, Data = data };
    public static ApiResult<T> Fail(string error) => new() { Success = false, Error = error };
}