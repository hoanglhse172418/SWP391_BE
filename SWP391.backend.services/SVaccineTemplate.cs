using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.VaccineTemplate;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SVaccineTemplate : IVaccineTemplate
    {
        private readonly IConfiguration _configuration;

        private readonly swpContext context;

        public SVaccineTemplate(swpContext Context, IConfiguration configuration)
        {
            context = Context;
            _configuration = configuration;
        }

        public async Task<List<VaccineTemplate>> GetAll(GetAllDTO request)
        {
            try
            {
                var query = context.VaccineTemplates.AsQueryable();

                // Filtering
                if (!string.IsNullOrEmpty(request.FilterOn) && !string.IsNullOrEmpty(request.FilterQuery))
                {
                    switch (request.FilterOn.ToLower())
                    {
                        case "description":
                            query = query.Where(v => v.Description != null && v.Description.Contains(request.FilterQuery));
                            break;
                        case "agerange":
                            query = query.Where(v => v.AgeRange != null && v.AgeRange.Contains(request.FilterQuery));
                            break;
                        case "notes":
                            query = query.Where(v => v.Notes != null && v.Notes.Contains(request.FilterQuery));
                            break;
                        case "month":
                            if (int.TryParse(request.FilterQuery, out var month))
                                query = query.Where(v => v.Month.HasValue && v.Month.Value == month);
                            break;
                        case "dosenumber":
                            if (int.TryParse(request.FilterQuery, out var doseNumber))
                                query = query.Where(v => v.DoseNumber.HasValue && v.DoseNumber.Value == doseNumber);
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid filter field: {request.FilterOn}");
                    }
                }

                // Sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    bool isAscending = request.IsAscending ?? true;
                    query = request.SortBy.ToLower() switch
                    {
                        "description" => isAscending ? query.OrderBy(v => v.Description) : query.OrderByDescending(v => v.Description),
                        "agerange" => isAscending ? query.OrderBy(v => v.AgeRange) : query.OrderByDescending(v => v.AgeRange),
                        "month" => isAscending ? query.OrderBy(v => v.Month) : query.OrderByDescending(v => v.Month),
                        "dosenumber" => isAscending ? query.OrderBy(v => v.DoseNumber) : query.OrderByDescending(v => v.DoseNumber),
                        _ => isAscending ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id) // Default sort by Id
                    };
                }

                // Paging
                int pageNumber = request.PageNumber ?? 1;
                int pageSize = request.PageSize ?? 10;

                var totalRecords = await query.CountAsync();
                var vaccineTemplates = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                return vaccineTemplates;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vaccine templates: {ex.Message}", ex);
            }
        }

        public async Task<VaccineTemplate> Create(CreateVaccineTemplateDTO request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "Request cannot be null.");

                if (request.DiseaseId == null || request.DiseaseId <= 0)
                    throw new ArgumentException("DiseaseId must be a valid positive number.");

                if (request.DoseNumber != null && request.DoseNumber <= 0)
                    throw new ArgumentException("DoseNumber must be a positive integer.");

                if (request.Month != null && request.Month < 0)
                    throw new ArgumentException("Month cannot be negative.");

                // Check if the disease exists in the database
                var existingDisease = await context.Diseases.FindAsync(request.DiseaseId);
                if (existingDisease == null)
                    throw new InvalidOperationException("DiseaseId does not exist.");

                // Create new VaccineTemplate
                var newVaccineTemplate = new VaccineTemplate
                {
                    DiseaseId = request.DiseaseId,
                    Description = request.Description?.Trim(),
                    Month = request.Month,
                    AgeRange = request.AgeRange?.Trim(),
                    DoseNumber = request.DoseNumber,
                    Notes = request.Notes?.Trim()
                };

                await context.VaccineTemplates.AddAsync(newVaccineTemplate);
                await context.SaveChangesAsync();

                return newVaccineTemplate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new Exception($"Vaccine template creation failed: {ex.Message}", ex);
            }
        }

        public async Task<VaccineTemplate> Update(int id, UpdateVaccineTemplateDTO request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "Request cannot be null.");

                // Find existing VaccineTemplate by ID
                var existingVaccineTemplate = await context.VaccineTemplates.FindAsync(id);
                if (existingVaccineTemplate == null)
                    throw new KeyNotFoundException($"VaccineTemplate with ID {id} not found.");

                // Validate DiseaseId if provided
                if (request.DiseaseId != null && request.DiseaseId > 0)
                {
                    var existingDisease = await context.Diseases.FindAsync(request.DiseaseId);
                    if (existingDisease == null)
                        throw new InvalidOperationException("DiseaseId does not exist.");
                    existingVaccineTemplate.DiseaseId = request.DiseaseId;
                }

                // Update fields if values are provided
                if (!string.IsNullOrWhiteSpace(request.Description))
                    existingVaccineTemplate.Description = request.Description.Trim();

                if (request.Month != null && request.Month >= 0)
                    existingVaccineTemplate.Month = request.Month;

                if (!string.IsNullOrWhiteSpace(request.AgeRange))
                    existingVaccineTemplate.AgeRange = request.AgeRange.Trim();

                if (request.DoseNumber != null && request.DoseNumber > 0)
                    existingVaccineTemplate.DoseNumber = request.DoseNumber;

                if (!string.IsNullOrWhiteSpace(request.Notes))
                    existingVaccineTemplate.Notes = request.Notes.Trim();

                // Save changes to the database
                context.VaccineTemplates.Update(existingVaccineTemplate);
                await context.SaveChangesAsync();

                return existingVaccineTemplate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new Exception($"Vaccine template update failed: {ex.Message}", ex);
            }
        }

        public async Task<VaccineTemplate> GetById(int id)
        {
            try
            {
                var vaccineTemplate = await context.VaccineTemplates.FindAsync(id);
                if (vaccineTemplate == null)
                    throw new KeyNotFoundException($"VaccineTemplate with ID {id} not found.");
                return vaccineTemplate;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vaccine template: {ex.Message}", ex);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                // Find existing VaccineTemplate by ID
                var existingVaccineTemplate = await context.VaccineTemplates.FindAsync(id);
                if (existingVaccineTemplate == null)
                    throw new KeyNotFoundException($"VaccineTemplate with ID {id} not found.");
                // Delete VaccineTemplate
                context.VaccineTemplates.Remove(existingVaccineTemplate);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new Exception($"Vaccine template deletion failed: {ex.Message}", ex);
            }
        }
    }
}
