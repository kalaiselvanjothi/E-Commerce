using Microsoft.EntityFrameworkCore;
using ShopVerse.Application.DTOs.User;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Entities;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class AddressService : IAddressService
{
    private readonly ShopVerseDbContext _context;

    public AddressService(ShopVerseDbContext context) => _context = context;

    public async Task<List<AddressDto>> GetByUserAsync(Guid userId)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => Map(a))
            .ToListAsync();
    }

    public async Task<AddressDto> CreateAsync(Guid userId, CreateAddressDto dto)
    {
        if (dto.IsDefault)
            await ClearDefaultAsync(userId);

        var address = new Address
        {
            UserId = userId,
            FullName = dto.FullName,
            Phone = dto.Phone,
            Line1 = dto.AddressLine1,
            Line2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.Pincode,
            Country = dto.Country,
            IsDefault = dto.IsDefault
        };

        // If this is the user's first address, make it default
        if (!await _context.Addresses.AnyAsync(a => a.UserId == userId && !a.IsDeleted))
            address.IsDefault = true;

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
        return Map(address);
    }

    public async Task<AddressDto> UpdateAsync(Guid userId, Guid addressId, CreateAddressDto dto)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted)
            ?? throw new KeyNotFoundException("Address not found.");

        if (dto.IsDefault && !address.IsDefault)
            await ClearDefaultAsync(userId);

        address.FullName = dto.FullName;
        address.Phone = dto.Phone;
        address.Line1 = dto.AddressLine1;
        address.Line2 = dto.AddressLine2;
        address.City = dto.City;
        address.State = dto.State;
        address.PostalCode = dto.Pincode;
        address.Country = dto.Country;
        address.IsDefault = dto.IsDefault;
        address.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Map(address);
    }

    public async Task DeleteAsync(Guid userId, Guid addressId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted)
            ?? throw new KeyNotFoundException("Address not found.");

        address.IsDeleted = true;
        address.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task SetDefaultAsync(Guid userId, Guid addressId)
    {
        await ClearDefaultAsync(userId);
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted)
            ?? throw new KeyNotFoundException("Address not found.");

        address.IsDefault = true;
        await _context.SaveChangesAsync();
    }

    private async Task ClearDefaultAsync(Guid userId)
    {
        var defaults = await _context.Addresses
            .Where(a => a.UserId == userId && a.IsDefault && !a.IsDeleted)
            .ToListAsync();
        defaults.ForEach(a => a.IsDefault = false);
    }

    private static AddressDto Map(Address a) => new()
    {
        Id = a.Id,
        FullName = a.FullName,
        Phone = a.Phone,
        AddressLine1 = a.Line1,
        AddressLine2 = a.Line2,
        City = a.City,
        State = a.State,
        Pincode = a.PostalCode,
        Country = a.Country,
        IsDefault = a.IsDefault,
        AddressType = "Home"
    };
}
