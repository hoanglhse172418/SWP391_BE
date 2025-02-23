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
                throw new Exception($"Error fetching vaccine: {ex.Message}", ex);
            }
        }

        public async Task<Vaccine> Create(CreateVaccineDTO request)
        {
            try
            {
                var newVaccine = new Vaccine
                {
                    Name = request.VaccineName,
                    Manufacture = request.Manufacture,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    RecAgeStart = request.RecAgeStart,
                    RecAgeEnd = request.RecAgeEnd,
                    InStockNumber = request.InStockNumber,
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
    }
}
