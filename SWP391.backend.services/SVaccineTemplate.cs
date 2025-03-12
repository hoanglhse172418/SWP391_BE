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
                        case "diseaseid":
                            if (int.TryParse(request.FilterQuery, out var diseaseId))
                                query = query.Where(v => v.DiseaseId.HasValue && v.DiseaseId.Value == diseaseId);
                            break;
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

        public async Task<List<VaccineTemplate>> Create(CreateVaccineTemplateDTO request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "Request cannot be null.");

                if (request.DiseaseId == null || !request.DiseaseId.Any())
                    throw new ArgumentException("DiseaseId must contain at least one valid ID.");

                if (request.DoseNumber != null && request.DoseNumber <= 0)
                    throw new ArgumentException("DoseNumber must be a positive integer.");

                if (request.Month != null && request.Month < 0)
                    throw new ArgumentException("Month cannot be negative.");

                // Lấy danh sách Disease hợp lệ từ database
                var existingDiseases = await context.Diseases
                    .Where(d => request.DiseaseId.Contains(d.Id))
                    .ToListAsync();

                if (existingDiseases.Count != request.DiseaseId.Count)
                    throw new InvalidOperationException("One or more DiseaseId do not exist.");

                var vaccineTemplates = new List<VaccineTemplate>();

                foreach (var disease in existingDiseases)
                {
                    var newVaccineTemplate = new VaccineTemplate
                    {
                        DiseaseId = disease.Id,
                        Description = request.Description?.Trim(),
                        Month = request.Month,
                        AgeRange = request.AgeRange?.Trim(),
                        DoseNumber = request.DoseNumber,
                        Notes = request.Notes?.Trim()
                    };

                    vaccineTemplates.Add(newVaccineTemplate);
                }

                await context.VaccineTemplates.AddRangeAsync(vaccineTemplates);
                await context.SaveChangesAsync();

                return vaccineTemplates;
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

        public async Task<List<VaccineTemplateDTO>> GetVaccineTemplatesbyProfileId(int ProfileId)
        {
            // Lấy danh sách vaccine templates liên quan đến profile
            var templates = await context.VaccineTemplates
                .Where(vt => context.VaccinationDetails
                    .Any(vd => vd.VaccinationProfileId == ProfileId && vd.DiseaseId == vt.DiseaseId))
                .OrderBy(vt => vt.DiseaseId)
                .ThenBy(vt => vt.Month)
                .ToListAsync(); // ⚠ Lấy về client-side trước khi xử lý

            // Lấy danh sách ngày tiêm của từng bệnh, trước tiên lấy toàn bộ dữ liệu từ database
            var detailsList = await context.VaccinationDetails
                .Where(vd => vd.VaccinationProfileId == ProfileId)
                .OrderBy(vd => vd.DiseaseId)
                .ThenBy(vd => vd.ExpectedInjectionDate)
                .ToListAsync(); // ⚠ Đưa dữ liệu về client-side trước khi dùng GroupBy

            // Nhóm dữ liệu theo DiseaseId (thực hiện trên bộ nhớ)
            var injectionDates = detailsList
                .GroupBy(vd => vd.DiseaseId)
                .ToDictionary(g => g.Key, g => g.Select(vd => vd.ExpectedInjectionDate).ToList());

            // Đánh số thứ tự cho VaccineTemplate theo từng DiseaseId
            var groupedTemplates = templates
                .GroupBy(vt => vt.DiseaseId)
                .SelectMany(g => g.Select((vt, index) => new { Template = vt, RowNumber = index }))
                .ToList(); // ⚠ Đưa về danh sách trước khi gán ngày

            // Gán ngày tiêm dự kiến theo thứ tự từng bệnh
            var result = new List<VaccineTemplateDTO>();

            foreach (var item in groupedTemplates)
            {
                var template = item.Template;
                var rowNumber = item.RowNumber;

                // Kiểm tra nếu có expected injection dates cho disease này
                if (injectionDates.TryGetValue(template.DiseaseId, out var dates) && rowNumber < dates.Count)
                {
                    result.Add(new VaccineTemplateDTO
                    {
                        Id = template.Id,
                        DiseaseId = template.DiseaseId ?? 0,
                        Description = template.Description,
                        Month = template.Month,
                        AgeRange = template.AgeRange,
                        DoseNumber = template.DoseNumber ?? 0,
                        Notes = template.Notes,
                        ExpectedInjectionDate = dates[rowNumber] // Lấy đúng ngày tiêm theo thứ tự
                    });
                }
                else
                {
                    // Nếu không có ngày tiêm, bỏ qua hoặc gán null
                    result.Add(new VaccineTemplateDTO
                    {
                        Id = template.Id,
                        DiseaseId = template.DiseaseId ?? 0,
                        Description = template.Description,
                        Month = template.Month,
                        AgeRange = template.AgeRange,
                        DoseNumber = template.DoseNumber ?? 0,
                        Notes = template.Notes,
                        ExpectedInjectionDate = null // Hoặc gán ngày mặc định
                    });
                }
            }

            if (!result.Any())
            {
                throw new Exception("No vaccine templates found for this profile.");
            }

            return result;
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
