using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Child;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SChild : IChild
    {
        private readonly IConfiguration _configuration;

        private readonly swpContext context;

        public SChild(swpContext Context, IConfiguration configuration)
        {
            context = Context;
            _configuration = configuration;
        }

        public async Task<List<Child>> GetAll(GetAllDTO request)
        {
            try
            {
                var query = context.Children.AsQueryable();

                // Filtering
                if (!string.IsNullOrEmpty(request.FilterOn) && !string.IsNullOrEmpty(request.FilterQuery))
                {
                    switch (request.FilterOn.ToLower())
                    {
                        case "childrenfullname":
                            query = query.Where(c => c.ChildrenFullname != null && c.ChildrenFullname.Contains(request.FilterQuery));
                            break;
                        case "parentfullname":
                            query = query.Where(c => c.ParentFullname != null && c.ParentFullname.Contains(request.FilterQuery));
                            break;
                        case "gender":
                            query = query.Where(c => c.Gender != null && c.Gender.Equals(request.FilterQuery, StringComparison.OrdinalIgnoreCase));
                            break;
                        case "dob":
                            if (DateTime.TryParse(request.FilterQuery, out var dob))
                                query = query.Where(c => c.Dob.HasValue && c.Dob.Value.Date == dob.Date);
                            break;
                        case "createdat":
                            if (DateTime.TryParse(request.FilterQuery, out var createdAt))
                                query = query.Where(c => c.CreatedAt.HasValue && c.CreatedAt.Value.Date == createdAt.Date);
                            break;
                        case "updatedat":
                            if (DateTime.TryParse(request.FilterQuery, out var updatedAt))
                                query = query.Where(c => c.UpdatedAt.HasValue && c.UpdatedAt.Value.Date == updatedAt.Date);
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
                        "childrenfullname" => isAscending ? query.OrderBy(c => c.ChildrenFullname) : query.OrderByDescending(c => c.ChildrenFullname),
                        "parentfullname" => isAscending ? query.OrderBy(c => c.ParentFullname) : query.OrderByDescending(c => c.ParentFullname),
                        "gender" => isAscending ? query.OrderBy(c => c.Gender) : query.OrderByDescending(c => c.Gender),
                        "dob" => isAscending ? query.OrderBy(c => c.Dob) : query.OrderByDescending(c => c.Dob),
                        "createdat" => isAscending ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
                        "updatedat" => isAscending ? query.OrderBy(c => c.UpdatedAt) : query.OrderByDescending(c => c.UpdatedAt),
                        _ => isAscending ? query.OrderBy(c => c.ChildrenFullname) : query.OrderByDescending(c => c.ChildrenFullname) // Default sort
                    };
                }

                // Paging
                int pageNumber = request.PageNumber ?? 1;
                int pageSize = request.PageSize ?? 10;

                var totalRecords = await query.CountAsync();
                var children = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                return children;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching children: {ex.Message}", ex);
            }
        }

        public async Task<Child> Create(CreateChildDTO request)
        {
            try
            {
                var child = new Child
                {
                    UserId = request.UserId,
                    ChildrenFullname = request.ChildrenFullname,
                    ParentFullname = request.ParentFullname,
                    Dob = request.Dob,
                    Gender = request.Gender,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add Child to the database
                context.Children.Add(child);
                await context.SaveChangesAsync(); // Ensure child ID is generated

                // Create Vaccination Profile
                var vaccinationProfile = new VaccinationProfile
                {
                    ChildrenId = child.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.VaccinationProfiles.Add(vaccinationProfile);
                await context.SaveChangesAsync(); // Ensure vaccinationProfile ID is generated

                // Create Vaccination Details (Assuming default vaccines are needed)
                var vaccinationDetails = request.VaccinationDetails.Select(detail => new VaccinationDetail
                {
                    VaccinationProfileId = vaccinationProfile.Id,
                    DiseaseId = detail.DiseaseId,
                    VaccineId = detail.VaccineId,
                    ExpectedInjectionDate = detail.ExpectedInjectionDate,
                    ActualInjectionDate = null // Injection has not been administered yet
                }).ToList();

                if (vaccinationDetails.Any())
                {
                    context.VaccinationDetails.AddRange(vaccinationDetails);
                    await context.SaveChangesAsync();
                }

                return child;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating child: {ex.Message}", ex);
            }
        }

        public async Task<Child> Update(int Id, UpdateChildDTO request)
        {
            try
            {
                var child = await context.Children.FindAsync(Id);
                if (child == null)
                {
                    throw new KeyNotFoundException($"Child with ID {Id} not found.");
                }

                // Update child details
                child.ChildrenFullname = request.ChildrenFullname ?? child.ChildrenFullname;
                child.ParentFullname = request.ParentFullname ?? child.ParentFullname;
                child.Dob = request.Dob ?? child.Dob;
                child.Gender = request.Gender ?? child.Gender;
                child.UpdatedAt = DateTime.UtcNow;

                // Update Vaccination Profile (if needed)
                var vaccinationProfile = await context.VaccinationProfiles.FirstOrDefaultAsync(vp => vp.ChildrenId == Id);
                if (vaccinationProfile != null)
                {
                    vaccinationProfile.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
                return child;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating child: {ex.Message}", ex);
            }
        }

        public async Task<Child> GetById(int Id)
        {
            try
            {
                var child = await context.Children
                    .Include(c => c.User)  // Include User (if needed)
                    .Include(c => c.VaccinationProfiles)
                        .ThenInclude(vp => vp.VaccinationDetails)
                    .FirstOrDefaultAsync(c => c.Id == Id);

                if (child == null)
                {
                    throw new KeyNotFoundException($"Child with ID {Id} not found.");
                }

                return child;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching child details: {ex.Message}", ex);
            }
        }

        public async Task<bool> Delete(int Id)
        {
            try
            {
                var child = await context.Children.FindAsync(Id);
                if (child == null)
                {
                    throw new KeyNotFoundException($"Child with ID {Id} not found.");
                }
                context.Children.Remove(child);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting child: {ex.Message}", ex);
            }
        }
    }
}
