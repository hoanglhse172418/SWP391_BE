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
    public class SVaccinationProfile : IVaccinationProfile
    {
        private readonly IConfiguration _configuration;

        private readonly swpContext context;

        public SVaccinationProfile(swpContext Context, IConfiguration configuration)
        {
            context = Context;
            _configuration = configuration;
        }

        public async Task<List<VaccinationProfile>> GetAll(GetAllDTO request)
        {
            try
            {
                var query = context.VaccinationProfiles.AsQueryable();

                // Filtering
                if (!string.IsNullOrEmpty(request.FilterOn) && !string.IsNullOrEmpty(request.FilterQuery))
                {
                    switch (request.FilterOn.ToLower())
                    {
                        case "childrenid":
                            if (int.TryParse(request.FilterQuery, out var childrenId))
                                query = query.Where(v => v.ChildrenId == childrenId);
                            break;
                        case "createdat":
                            if (DateTime.TryParse(request.FilterQuery, out var createdAt))
                                query = query.Where(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Date == createdAt.Date);
                            break;
                        case "updatedat":
                            if (DateTime.TryParse(request.FilterQuery, out var updatedAt))
                                query = query.Where(v => v.UpdatedAt.HasValue && v.UpdatedAt.Value.Date == updatedAt.Date);
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
                        "childrenid" => isAscending ? query.OrderBy(v => v.ChildrenId) : query.OrderByDescending(v => v.ChildrenId),
                        "createdat" => isAscending ? query.OrderBy(v => v.CreatedAt) : query.OrderByDescending(v => v.CreatedAt),
                        "updatedat" => isAscending ? query.OrderBy(v => v.UpdatedAt) : query.OrderByDescending(v => v.UpdatedAt),
                        _ => isAscending ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id) // Default sort
                    };
                }

                // Paging
                int pageNumber = request.PageNumber ?? 1;
                int pageSize = request.PageSize ?? 10;

                var totalRecords = await query.CountAsync();
                var vaccinationProfiles = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                return vaccinationProfiles;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vaccination profiles: {ex.Message}", ex);
            }
        }

        public async Task<VaccinationProfile> Create(int childId)
        {
            // Check if the child exists
            var childExists = await context.Children.AnyAsync(c => c.Id == childId);
            if (!childExists)
            {
                throw new Exception($"Child with ID {childId} not found.");
            }

            // Create a new VaccinationProfile linked to the child
            var vaccinationProfile = new VaccinationProfile
            {
                ChildrenId = childId,
                CreatedAt = DateTime.UtcNow
            };

            // Add to database
            context.VaccinationProfiles.Add(vaccinationProfile);
            await context.SaveChangesAsync();

            return vaccinationProfile;
        }

        public async Task<VaccinationProfile> Update(int profileId)
        {
            // Find the VaccinationProfile by ID
            var vaccinationProfile = await context.VaccinationProfiles.FindAsync(profileId);

            if (vaccinationProfile == null)
            {
                throw new Exception($"Vaccination Profile with ID {profileId} not found.");
            }

            // Update fields
            vaccinationProfile.UpdatedAt = DateTime.UtcNow;

            // Save changes to database
            await context.SaveChangesAsync();

            return vaccinationProfile;
        }

        public async Task<VaccinationProfile> GetById(int profileId)
        {
            // Retrieve the VaccinationProfile including related Child details
            var vaccinationProfile = await context.VaccinationProfiles
                .Include(vp => vp.Children) // Include Child details if needed
                .FirstOrDefaultAsync(vp => vp.Id == profileId);

            if (vaccinationProfile == null)
            {
                throw new Exception($"Vaccination Profile with ID {profileId} not found.");
            }

            return vaccinationProfile;
        }

        public async Task <bool>Delete(int profileId)
        {
            // Find the VaccinationProfile by ID
            var vaccinationProfile = await context.VaccinationProfiles.FindAsync(profileId);
            if (vaccinationProfile == null)
            {
                throw new Exception($"Vaccination Profile with ID {profileId} not found.");
            }
            // Remove from database
            context.VaccinationProfiles.Remove(vaccinationProfile);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
