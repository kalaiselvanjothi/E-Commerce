using ShopVerse.Application.DTOs.User;

namespace ShopVerse.Application.Interfaces;

public interface IAddressService
{
    Task<List<AddressDto>> GetByUserAsync(Guid userId);
    Task<AddressDto> CreateAsync(Guid userId, CreateAddressDto dto);
    Task<AddressDto> UpdateAsync(Guid userId, Guid addressId, CreateAddressDto dto);
    Task DeleteAsync(Guid userId, Guid addressId);
    Task SetDefaultAsync(Guid userId, Guid addressId);
}
