using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zadanie7.DTOs;
using Zadanie7.Repositories;

namespace Zadanie7.Controllers;
[ApiController]
[Route("[controller]")]
public class WareHouseController : ControllerBase
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WareHouseController(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(WareHouseDTO wareHouseDto)
    {
        
        if (wareHouseDto == null)
        {

            return BadRequest("Bad Request");
        }

        try
        {
            int result = await _warehouseRepository.Add(wareHouseDto);
            if (result==0)
            {
                return BadRequest("Bad request");
            }
            
            
            return Created($"Warehouse/{result}", wareHouseDto);

        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
        
    }
}