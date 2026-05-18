using System.ComponentModel.DataAnnotations;

namespace ShuruApi.Dtos;

public class UpdateItemRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = default!;

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}
