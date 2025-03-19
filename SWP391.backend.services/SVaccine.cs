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

        public async Task<List<Vaccine>> GetAllVaccine()
        {
            try
            {
                var vaccineList = await context.Vaccines                 
                    .OrderBy(v => v.Id) // Sắp xếp theo Id của vaccine
                    .ToListAsync();

                return vaccineList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vaccine list: {ex.Message}", ex);
            }
        }
        public async Task<List<Vaccine>> GetAllVaccineForUser()
        {
            try
            {
                var vaccineList = await context.Vaccines
                    .Include(v => v.Diseases) // Load danh sách Disease liên kết với Vaccine
                    .OrderBy(v => v.Diseases.Min(d => d.Id)) // Sắp xếp theo Disease.Id tăng dần
                    .ToListAsync();

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
                    CreatedAt = DateTime.UtcNow,
                    //Id = int.Parse("V" + new Random().Next(1000, 9999))
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

        public async Task<List<Vaccine>> GetAllVaccinesByDiasease(string diaseaseName)
        {
            try
            {
                // Tìm disease theo tên và lấy danh sách vaccine liên quan
                var disease = await context.Diseases
                    .Include(d => d.Vaccines)
                    .FirstOrDefaultAsync(d => d.Name == diaseaseName);

                

                // Trả về danh sách vaccine hoặc danh sách rỗng nếu không tìm thấy disease
                return disease?.Vaccines.ToList() ?? new List<Vaccine>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving vaccines: {ex.Message}", ex);
            }
        }
    }
}
