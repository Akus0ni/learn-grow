using System.ComponentModel.DataAnnotations;
using ShuruApi.Domain;

namespace ShuruApi.Dtos;

public class CreateTableRequest
{
    [Required]
    public string TableNumber { get; set; }

    [Required]
    public int SeatingCapacity { get; set; }

    [Required]
    public Location Location { get; set; }
}
