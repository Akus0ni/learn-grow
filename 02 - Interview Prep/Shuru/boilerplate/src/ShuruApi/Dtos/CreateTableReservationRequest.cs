using System.ComponentModel.DataAnnotations;
using ShuruApi.Domain;

namespace ShuruApi.Dtos;

public class CreateTableReservationRequest
{
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public Table Table { get; set; }

    [Required]
    public required Customer Customer { get; set; }
}
