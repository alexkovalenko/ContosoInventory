using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Defines operations for managing inventory products.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves all products, optionally filtered by category.
    /// </summary>
    /// <param name="categoryId">The optional category identifier.</param>
    /// <returns>A list of product DTOs.</returns>
    Task<List<ProductResponseDto>> GetAllProductsAsync(int? categoryId = null);

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product DTO, or null if not found.</returns>
    Task<ProductResponseDto?> GetProductByIdAsync(int id);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="dto">The product creation data.</param>
    /// <returns>The created product DTO.</returns>
    /// <exception cref="ArgumentException">Thrown when the input is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a business rule is violated.</exception>
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">The updated product data.</param>
    /// <returns>The updated product DTO, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when the input is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a business rule is violated.</exception>
    Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto dto);

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>True if the product was deleted, false if not found.</returns>
    Task<bool> DeleteProductAsync(int id);

    /// <summary>
    /// Increases the stock quantity for a product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">The restock request.</param>
    /// <returns>The updated product DTO, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when the input is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a business rule is violated.</exception>
    Task<ProductResponseDto?> RestockProductAsync(int id, RestockProductDto dto);
}