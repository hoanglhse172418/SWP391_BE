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
                        case "UserId":
                            if (int.TryParse(request.FilterQuery, out var userId))
                                query = query.Where(c => c.UserId == userId);
                            break;
                        case "childrenfullname":
                            query = query.Where(c => c.ChildrenFullname != null && c.ChildrenFullname.Contains(request.FilterQuery));
                            break;
                        case "fatherfullname":
                            query = query.Where(c => c.FatherFullName != null && c.FatherFullName.Contains(request.FilterQuery));
                            break;
                        case "motherfullname":
                            query = query.Where(c => c.MotherFullName != null && c.MotherFullName.Contains(request.FilterQuery));
                            break;
                        case "fatherphonenumber":
                            query = query.Where(c => c.FatherPhoneNumber != null && c.FatherPhoneNumber.Contains(request.FilterQuery));
                            break;
                        case "motherphonenumber":
                            query = query.Where(c => c.MotherPhoneNumber != null && c.MotherPhoneNumber.Contains(request.FilterQuery));
                            break;
                        case "address":
                            query = query.Where(c => c.Address != null && c.Address.Contains(request.FilterQuery));
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
                        "fatherfullname" => isAscending ? query.OrderBy(c => c.FatherFullName) : query.OrderByDescending(c => c.FatherFullName),
                        "motherfullname" => isAscending ? query.OrderBy(c => c.MotherFullName) : query.OrderByDescending(c => c.MotherFullName),
                        "fatherphonenumber" => isAscending ? query.OrderBy(c => c.FatherPhoneNumber) : query.OrderByDescending(c => c.FatherPhoneNumber),
                        "motherphonenumber" => isAscending ? query.OrderBy(c => c.MotherPhoneNumber) : query.OrderByDescending(c => c.MotherPhoneNumber),
                        "address" => isAscending ? query.OrderBy(c => c.Address) : query.OrderByDescending(c => c.Address),
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
            var child = new Child
            {
                UserId = request.UserId,
                ChildrenFullname = request.ChildrenFullname,
                Dob = request.Dob,
                Gender = request.Gender,
                CreatedAt = DateTime.UtcNow,
                FatherFullName = request.FatherFullName,
                MotherFullName = request.MotherFullName,
                FatherPhoneNumber = request.FatherPhoneNumber,
                MotherPhoneNumber = request.MotherPhoneNumber,
                Address = request.Address
            };

            // Add child to database
            context.Children.Add(child);
            await context.SaveChangesAsync();

            // Create a VaccinationProfile linked to the newly created child
            var vaccinationProfile = new VaccinationProfile
            {
                ChildrenId = child.Id,
                CreatedAt = DateTime.UtcNow,
            };

            // Add VaccinationProfile to database
            context.VaccinationProfiles.Add(vaccinationProfile);
            await context.SaveChangesAsync();

            return child;
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
                child.Dob = request.Dob ?? child.Dob;
                child.Gender = request.Gender ?? child.Gender;
                child.FatherFullName = request.FatherFullName ?? child.FatherFullName;
                child.MotherFullName = request.MotherFullName ?? child.MotherFullName;
                child.FatherPhoneNumber = request.FatherPhoneNumber ?? child.FatherPhoneNumber;
                child.MotherPhoneNumber = request.MotherPhoneNumber ?? child.MotherPhoneNumber;
                child.Address = request.Address ?? child.Address;
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
