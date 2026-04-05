namespace ContosoInventory.Shared.DTOs;

/// <summary>
/// Represents the data required to update a product.
/// </summary>
public class UpdateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999.99")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}