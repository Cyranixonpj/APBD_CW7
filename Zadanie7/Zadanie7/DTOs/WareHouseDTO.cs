using System.ComponentModel.DataAnnotations;

namespace Zadanie7.DTOs;

public record WareHouseDTO(
    [Required] int IdProduct,
    [Required] int IdWaregouse,
    [Required] int Amount,
    [Required] DateTime CreatedAt
);
