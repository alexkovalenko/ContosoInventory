namespace ContosoInventory.Shared.DTOs;

/// <summary>
/// Represents the quantity used to restock a product.
/// </summary>
public class RestockProductDto
{
    [Range(1, int.MaxValue)]
    public int QuantityToAdd { get; set; }
}