using Microsoft.EntityFrameworkCore;
using ContosoInventory.Server.Data;
using ContosoInventory.Server.Models;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Provides operations for managing inventory products.
/// </summary>
public class ProductService : IProductService
{
    private readonly InventoryContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(InventoryContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ProductResponseDto>> GetAllProductsAsync(int? categoryId = null)
    {
        try
        {
            var query = _context.Products.AsNoTracking();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category ID {CategoryId}.", categoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : MapToDto(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto)
    {
        ValidateCreateDto(dto);

        try
        {
            await EnsureCategoryExistsAsync(dto.CategoryId);
            await EnsureSkuIsUniqueAsync(dto.Sku);

            var utcNow = DateTime.UtcNow;
            var product = new Product
            {
                Name = dto.Name.Trim(),
                Sku = NormalizeSku(dto.Sku),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                CreatedDate = utcNow,
                LastUpdatedDate = utcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created: {ProductName} (ID: {ProductId}, SKU: {Sku}).", product.Name, product.Id, product.Sku);

            return MapToDto(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product '{ProductName}'.", dto.Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        ValidateUpdateDto(dto);

        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return null;
            }

            await EnsureCategoryExistsAsync(dto.CategoryId);
            await EnsureSkuIsUniqueAsync(dto.Sku, id);

            product.Name = dto.Name.Trim();
            product.Sku = NormalizeSku(dto.Sku);
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            product.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product updated: {ProductName} (ID: {ProductId}, SKU: {Sku}).", product.Name, product.Id, product.Sku);

            return MapToDto(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted: {ProductName} (ID: {ProductId}, SKU: {Sku}).", product.Name, product.Id, product.Sku);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> RestockProductAsync(int id, RestockProductDto dto)
    {
        ValidateRestockDto(dto);

        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return null;
            }

            if (product.StockQuantity > int.MaxValue - dto.QuantityToAdd)
            {
                throw new InvalidOperationException("Restock quantity is too large for the current stock quantity.");
            }

            product.StockQuantity += dto.QuantityToAdd;
            product.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product restocked: {ProductName} (ID: {ProductId}) increased by {QuantityToAdd} to {StockQuantity}.",
                product.Name,
                product.Id,
                dto.QuantityToAdd,
                product.StockQuantity);

            return MapToDto(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restocking product with ID {ProductId}.", id);
            throw;
        }
    }

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
        {
            throw new InvalidOperationException($"The category with ID '{categoryId}' does not exist.");
        }
    }

    private async Task EnsureSkuIsUniqueAsync(string sku, int? currentProductId = null)
    {
        var normalizedSku = NormalizeSku(sku);
        var skuExists = await _context.Products.AnyAsync(p => p.Sku == normalizedSku && (!currentProductId.HasValue || p.Id != currentProductId.Value));

        if (skuExists)
        {
            throw new InvalidOperationException($"A product with the SKU '{normalizedSku}' already exists.");
        }
    }

    private static void ValidateCreateDto(CreateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateCommonFields(dto.Name, dto.Sku, dto.Description, dto.Price, dto.StockQuantity, dto.CategoryId);
    }

    private static void ValidateUpdateDto(UpdateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateCommonFields(dto.Name, dto.Sku, dto.Description, dto.Price, dto.StockQuantity, dto.CategoryId);
    }

    private static void ValidateRestockDto(RestockProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.QuantityToAdd <= 0)
        {
            throw new ArgumentException("Restock quantity must be greater than zero.", nameof(dto));
        }
    }

    private static void ValidateCommonFields(string name, string sku, string description, decimal price, int stockQuantity, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU is required.", nameof(sku));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Product description is required.", nameof(description));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));
        }

        if (categoryId <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(categoryId));
        }
    }

    private static string NormalizeSku(string sku)
    {
        return sku.Trim().ToUpperInvariant();
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CreatedDate = product.CreatedDate,
            LastUpdatedDate = product.LastUpdatedDate
        };
    }
}