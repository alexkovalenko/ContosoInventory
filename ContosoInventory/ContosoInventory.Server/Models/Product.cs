using System.ComponentModel.DataAnnotations;

namespace ContosoInventory.Server.Models;

/// <summary>
/// Represents an inventory product.
/// </summary>
public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdatedDate { get; set; }
}