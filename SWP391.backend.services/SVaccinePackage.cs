using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO.VaccinePackage;
using SWP391.backend.repository.DTO.VaccinePackageItem;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SVaccinePackage : IVaccinePackage
    {
        private readonly IConfiguration _configuration;

        private readonly swpContext _swpContext;

        public SVaccinePackage(IConfiguration configuration, swpContext swpContext)
        {
            _configuration = configuration;
            _swpContext = swpContext;
        }

        public async Task<VaccinePackage> CreateVaccinePackageAsync(CreateVaccinePackageDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Package name is required.");

            if (request.VaccinePackageItems == null || !request.VaccinePackageItems.Any())
                throw new ArgumentException("At least one vaccine item is required.");

            var vaccinePackage = new VaccinePackage
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            _swpContext.VaccinePackages.Add(vaccinePackage);
            await _swpContext.SaveChangesAsync();



            // Fetch vaccine prices from the database
            var vaccineIds = request.VaccinePackageItems.Select(v => v.VaccineId).ToList();
            var vaccineData = await _swpContext.Vaccines
                .Where(v => vaccineIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, v => v.Price); // Assuming Price is stored as string

            var packageItems = new List<VaccinePackageItem>();

            foreach (var item in request.VaccinePackageItems)
            {
                if (!vaccineData.TryGetValue(item.VaccineId, out var priceStr) || string.IsNullOrWhiteSpace(priceStr))
                {
                    throw new ArgumentException($"Vaccine with ID {item.VaccineId} not found or missing price.");
                }

                if (!decimal.TryParse(priceStr, out var pricePerDose))
                {
                    throw new ArgumentException($"Invalid price format for Vaccine ID {item.VaccineId}.");
                }

                packageItems.Add(new VaccinePackageItem
                {
                    VaccinePackageId = vaccinePackage.Id,
                    VaccineId = item.VaccineId,
                    DoseNumber = item.DoseNumber,
                    PricePerDose = pricePerDose
                });
            }

            _swpContext.VaccinePackageItems.AddRange(packageItems);
            await _swpContext.SaveChangesAsync();

            // Calculate total price based on doses
            vaccinePackage.TotalPrice = packageItems.Sum(item => item.PricePerDose * item.DoseNumber);
            _swpContext.VaccinePackages.Update(vaccinePackage);
            await _swpContext.SaveChangesAsync();

            return vaccinePackage;
        }


        public async Task<VaccinePackage> UpdateVaccinePackageAsync(UpdateVaccinePackageDTO request)
        {
            var vaccinePackage = await _swpContext.VaccinePackages
                .Include(p => p.VaccinePackageItems)
                .FirstOrDefaultAsync(p => p.Id == request.Id);

            if (vaccinePackage == null)
                throw new ArgumentException($"Vaccine package with ID {request.Id} not found.");

            // Update package name
            vaccinePackage.Name = request.Name;
            vaccinePackage.UpdatedAt = DateTime.UtcNow;

            // Remove old package items
            _swpContext.VaccinePackageItems.RemoveRange(vaccinePackage.VaccinePackageItems);

            // Fetch new vaccine prices
            var vaccineIds = request.VaccinePackageItems.Select(v => v.VaccineId).ToList();
            var vaccineData = await _swpContext.Vaccines
                .Where(v => vaccineIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, v => v.Price);

            var packageItems = new List<VaccinePackageItem>();

            foreach (var item in request.VaccinePackageItems)
            {
                if (!vaccineData.TryGetValue(item.VaccineId, out var priceStr) || string.IsNullOrWhiteSpace(priceStr))
                {
                    throw new ArgumentException($"Vaccine with ID {item.VaccineId} not found or missing price.");
                }

                if (!decimal.TryParse(priceStr, out var pricePerDose))
                {
                    throw new ArgumentException($"Invalid price format for Vaccine ID {item.VaccineId}.");
                }

                packageItems.Add(new VaccinePackageItem
                {
                    VaccinePackageId = vaccinePackage.Id,
                    VaccineId = item.VaccineId,
                    DoseNumber = item.DoseNumber,
                    PricePerDose = pricePerDose
                });
            }

            _swpContext.VaccinePackageItems.AddRange(packageItems);

            // Update total price based on new items
            vaccinePackage.TotalPrice = packageItems.Sum(item => item.PricePerDose * item.DoseNumber);

            await _swpContext.SaveChangesAsync();
            return vaccinePackage;
        }


        public async Task<VaccinePackage?> GetVaccinePackageByIdAsync(int id)
        {
            return await _swpContext.VaccinePackages
                .Include(p => p.VaccinePackageItems)
                .ThenInclude(v => v.Vaccine)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<VaccinePackage?>> GetAllAsync()
        {
            return await _swpContext.VaccinePackages
                .Include(p => p.VaccinePackageItems)
                .ToListAsync();
        }

        public async Task<bool> DeleteVaccinePackageAsync(int id)
        {
            var vaccinePackage = await _swpContext.VaccinePackages
                .Include(p => p.VaccinePackageItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (vaccinePackage == null)
                throw new ArgumentException($"Vaccine package with ID {id} not found.");

            // Xóa các VaccinePackageItem liên quan
            _swpContext.VaccinePackageItems.RemoveRange(vaccinePackage.VaccinePackageItems);

            // Xóa VaccinePackage
            _swpContext.VaccinePackages.Remove(vaccinePackage);

            await _swpContext.SaveChangesAsync();
            return true;
        }

    }
}
