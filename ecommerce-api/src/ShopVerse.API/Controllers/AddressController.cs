using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.User;
using ShopVerse.Application.Interfaces;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize]
[Produces("application/json")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService) => _addressService = addressService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _addressService.GetByUserAsync(GetUserId());
        return Ok(ApiResponse<List<AddressDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
    {
        var address = await _addressService.CreateAsync(GetUserId(), dto);
        return CreatedAtAction(nameof(GetAll), ApiResponse<AddressDto>.Ok(address, "Address added"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateAddressDto dto)
    {
        var address = await _addressService.UpdateAsync(GetUserId(), id, dto);
        return Ok(ApiResponse<AddressDto>.Ok(address, "Address updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _addressService.DeleteAsync(GetUserId(), id);
        return Ok(ApiResponse<object>.Ok(null!, "Address deleted"));
    }

    [HttpPatch("{id:guid}/set-default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        await _addressService.SetDefaultAsync(GetUserId(), id);
        return Ok(ApiResponse<object>.Ok(null!, "Default address updated"));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
