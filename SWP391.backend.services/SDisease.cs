using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SDisease : IDisease
    {
        private readonly IConfiguration _configuration;

        private readonly swpContext context;

        public SDisease(swpContext Context, IConfiguration configuration)
        {
            context = Context;
            _configuration = configuration;
        }

        public async Task<List<Disease>> GetAll(GetAllDTO request)
        {
            try
            {
                var query = context.Diseases.AsQueryable();

                // Filtering
                if (!string.IsNullOrEmpty(request.FilterOn) && !string.IsNullOrEmpty(request.FilterQuery))
                {
                    switch (request.FilterOn.ToLower())
                    {
                        case "name":
                            query = query.Where(d => d.Name != null && d.Name.Contains(request.FilterQuery));
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
                        "name" => isAscending ? query.OrderBy(d => d.Name) : query.OrderByDescending(d => d.Name),
                        _ => isAscending ? query.OrderBy(d => d.Id) : query.OrderByDescending(d => d.Id) // Default sort by ID
                    };
                }

                // Paging
                int pageNumber = request.PageNumber ?? 1;
                int pageSize = request.PageSize ?? 10;

                var totalRecords = await query.CountAsync();
                var diseases = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                return diseases;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching diseases: {ex.Message}", ex);
            }
        }

        public async Task<Disease> Create(string name)
        {
            try
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name), "Name cannot be null.");

                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Disease name cannot be null or empty.");

                // Normalize name (trim + proper case)
                name = name.Trim();

                // Check if the disease already exists
                var existingDisease = await context.Diseases
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower());

                if (existingDisease != null)
                    throw new InvalidOperationException("This disease already exists!");

                // Create new disease entity
                var newDisease = new Disease
                {
                    Name = name
                };

                await context.Diseases.AddAsync(newDisease);
                await context.SaveChangesAsync();

                return newDisease;
            }
            catch (Exception ex)
            {
                throw new Exception($"Disease creation failed: {ex.Message}", ex);
            }
        }

        public async Task<Disease> Update(int id, string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be null.");

                var disease = await context.Diseases.FirstOrDefaultAsync(d => d.Id == id);
                if (disease == null)
                    throw new KeyNotFoundException("Disease name cannot be null or empty.");

                // Chuẩn hóa tên (xóa khoảng trắng thừa)
                name = name.Trim();

                // Kiểm tra xem đã tồn tại bệnh khác có cùng tên chưa
                var existingDisease = await context.Diseases
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower() && d.Id != id);

                if (existingDisease != null)
                    throw new InvalidOperationException("This disease already exists!");

                // Cập nhật thông tin
                disease.Name = name;

                context.Diseases.Update(disease);
                await context.SaveChangesAsync();

                return disease;
            }
            catch (Exception ex)
            {
                throw new Exception($"Disease update failed: {ex.Message}", ex);
            }
        }

        public async Task<Disease> GetById(int id)
        {
            try
            {
                var disease = await context.Diseases.FirstOrDefaultAsync(d => d.Id == id);
                if (disease == null)
                    throw new KeyNotFoundException("Disease not found.");
                return disease;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching disease: {ex.Message}", ex);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var disease = await context.Diseases.FirstOrDefaultAsync(d => d.Id == id);
                if (disease == null)
                    throw new KeyNotFoundException("Disease not found.");
                context.Diseases.Remove(disease);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Disease deletion failed: {ex.Message}", ex);
            }
        }
    }
}
