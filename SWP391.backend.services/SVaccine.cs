using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Vaccine;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SVaccine : IVaccine
    {
        private readonly IConfiguration _configuration;
        private readonly swpContext context;

        public SVaccine(IConfiguration configuration, swpContext context)
        {
            _configuration = configuration;
            this.context = context;
        }

        public async Task<List<Vaccine>> GetAllVaccine(GetAllDTO request)
        {
            try
            {
                var vaccineList = await context.Vaccines.ToListAsync();
                return vaccineList;
            }
            catch (Exception ex) 
            {
                throw new Exception($"Error fetching vaccine list: {ex.Message}", ex);
            }
        }

        public async Task<Vaccine> GetById(int id)
        {
            try
            {
                var vaccine = await context.Vaccines.FirstOrDefaultAsync(v => v.Id == id);
                return vaccine;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get vaccine: {ex.Message}", ex);
            }
        }
        public async Task<Vaccine> Create(CreateVaccineDTO request, string imageUrl)
        {
            try
            {
                var newVaccine = new Vaccine
                {
                    Name = request.VaccineName,
                    Manufacture = request.Manufacture,
                    Description = request.Description,
                    ImageUrl = imageUrl,
                    RecAgeStart = request.RecAgeStart,
                    RecAgeEnd = request.RecAgeEnd,
                    InStockNumber = request.InStockNumber,
                    Price = request.Price,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };
                context.Vaccines.Add(newVaccine);
                await context.SaveChangesAsync();
                return newVaccine;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error create vaccine: {ex.Message}", ex);
            }
        }

        public async Task<Vaccine> Update(int vaccineId, UpdateVaccineDTO request, string imageUrl)
        {
            try
            {
                var foundVaccine = await context.Vaccines.FindAsync(vaccineId);
                if(foundVaccine == null)
                {
                    throw new KeyNotFoundException($"Vaccine with ID: {vaccineId} not found");
                }

                foundVaccine.Name = string.IsNullOrEmpty(request.VaccineName) ? foundVaccine.Name : request.VaccineName;
                foundVaccine.Manufacture = string.IsNullOrEmpty(request.Manufacture) ? foundVaccine.Manufacture : request.Manufacture;
                foundVaccine.Description = string.IsNullOrEmpty(request.Description) ? foundVaccine.Description : request.Description;
                foundVaccine.ImageUrl = string.IsNullOrEmpty(imageUrl) ? foundVaccine.ImageUrl : imageUrl;
                foundVaccine.RecAgeStart = string.IsNullOrEmpty(request.RecAgeStart.ToString()) ? foundVaccine.RecAgeStart : request.RecAgeStart;
                foundVaccine.RecAgeEnd = string.IsNullOrEmpty(request.RecAgeEnd.ToString()) ? foundVaccine.RecAgeEnd : request.RecAgeEnd;
                foundVaccine.Notes = string.IsNullOrEmpty(request.Notes) ? foundVaccine.Notes : request.Notes;
                foundVaccine.InStockNumber = string.IsNullOrEmpty(request.InStockNumber.ToString()) ? foundVaccine.InStockNumber : request.InStockNumber;
                foundVaccine.Price = string.IsNullOrEmpty(request.Price) ? foundVaccine.Price : request.Price;
                foundVaccine.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return foundVaccine;
            }
            catch (Exception ex) 
            {
                throw new Exception($"Error update vaccine: {ex.Message}", ex);
            }
        }
    }
}
