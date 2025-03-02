using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.VaccinationDetail;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SVaccinationDetail : IVaccinationDetail
    {
        private readonly IConfiguration _configuration;

        private readonly swpContext context;

        public SVaccinationDetail(swpContext Context, IConfiguration configuration)
        {
            context = Context;
            _configuration = configuration;
        }

        public async Task<List<VaccinationDetail>> GetAll(GetAllDTO request)
        {
            try
            {
                var query = context.VaccinationDetails.AsQueryable();

                // Filtering
                if (!string.IsNullOrEmpty(request.FilterOn) && !string.IsNullOrEmpty(request.FilterQuery))
                {
                    switch (request.FilterOn.ToLower())
                    {
                        case "vaccinationprofileid":
                            if (int.TryParse(request.FilterQuery, out var profileId))
                                query = query.Where(v => v.VaccinationProfileId == profileId);
                            break;
                        case "diseaseid":
                            if (int.TryParse(request.FilterQuery, out var diseaseId))
                                query = query.Where(v => v.DiseaseId == diseaseId);
                            break;
                        case "vaccineid":
                            if (int.TryParse(request.FilterQuery, out var vaccineId))
                                query = query.Where(v => v.VaccineId == vaccineId);
                            break;
                        case "expectedinjectiondate":
                            if (DateTime.TryParse(request.FilterQuery, out var expectedDate))
                                query = query.Where(v => v.ExpectedInjectionDate.HasValue && v.ExpectedInjectionDate.Value.Date == expectedDate.Date);
                            break;
                        case "actualinjectiondate":
                            if (DateTime.TryParse(request.FilterQuery, out var actualDate))
                                query = query.Where(v => v.ActualInjectionDate.HasValue && v.ActualInjectionDate.Value.Date == actualDate.Date);
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
                        "vaccinationprofileid" => isAscending ? query.OrderBy(v => v.VaccinationProfileId) : query.OrderByDescending(v => v.VaccinationProfileId),
                        "diseaseid" => isAscending ? query.OrderBy(v => v.DiseaseId) : query.OrderByDescending(v => v.DiseaseId),
                        "vaccineid" => isAscending ? query.OrderBy(v => v.VaccineId) : query.OrderByDescending(v => v.VaccineId),
                        "expectedinjectiondate" => isAscending ? query.OrderBy(v => v.ExpectedInjectionDate) : query.OrderByDescending(v => v.ExpectedInjectionDate),
                        "actualinjectiondate" => isAscending ? query.OrderBy(v => v.ActualInjectionDate) : query.OrderByDescending(v => v.ActualInjectionDate),
                        _ => isAscending ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id) // Default sort
                    };
                }

                // Paging
                int pageNumber = request.PageNumber ?? 1;
                int pageSize = request.PageSize ?? 10;

                var totalRecords = await query.CountAsync();
                var vaccinationDetails = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                return vaccinationDetails;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vaccination details: {ex.Message}", ex);
            }
        }

        public async Task<VaccinationDetail> Create(CreateVaccinationDetailDTO request)
        {
            try
            {
                // Lấy thông tin trẻ em
                var child = await context.Children.FindAsync(request.ChildrenId);
                if (child == null)
                {
                    throw new Exception("Child not found.");
                }

                // Lấy thông tin VaccinationProfile của trẻ
                var vaccinationProfile = await context.VaccinationProfiles
                    .FirstOrDefaultAsync(vp => vp.ChildrenId == request.ChildrenId);
                if (vaccinationProfile == null)
                {
                    throw new Exception("Vaccination profile not found.");
                }

                // Lấy thông tin VaccineTemplate
                var template = await context.VaccineTemplates
                    .FirstOrDefaultAsync(vt => vt.DiseaseId == request.DiseaseId);

                if (template == null)
                {
                    throw new Exception("Vaccine template not found.");
                }

                // Xác định ngày tiêm dự kiến (ExpectedInjectionDate) dựa trên template.Month
                DateTime expectedInjectionDate = child.Dob ?? DateTime.UtcNow;

                if (template.Month.HasValue)
                {
                    expectedInjectionDate = child.Dob.Value.AddMonths(template.Month.Value);
                }
                else if (!string.IsNullOrEmpty(template.AgeRange))
                {
                    // Nếu AgeRange có dữ liệu dạng "X tháng" hoặc "Y năm", cần xử lý
                    if (template.AgeRange.Contains("tháng"))
                    {
                        int months = int.Parse(template.AgeRange.Replace(" tháng", "").Trim());
                        expectedInjectionDate = child.Dob.Value.AddMonths(months);
                    }
                    else if (template.AgeRange.Contains("năm"))
                    {
                        int years = int.Parse(template.AgeRange.Replace(" năm", "").Trim());
                        expectedInjectionDate = child.Dob.Value.AddYears(years);
                    }
                }

                // Chuyển đổi tháng từ DTO sang số nguyên (nếu có) - đây là tháng mà người dùng nhập
                if (!int.TryParse(request.month, out int actualMonth))
                {
                    throw new Exception("Invalid month format.");
                }

                // Xác định ngày tiêm thực tế (ActualInjectionDate) dựa trên tháng người dùng nhập
                DateTime actualInjectionDate = child.Dob.Value.AddMonths(actualMonth);

                // Kiểm tra nếu ngày tiêm thực tế < ngày tiêm dự kiến thì báo lỗi
                if (actualInjectionDate < expectedInjectionDate)
                {
                    throw new Exception("Actual injection date must be equal or greater than expected injection date.");
                }

                // Tạo VaccinationDetail
                var vaccinationDetail = new VaccinationDetail
                {
                    VaccinationProfileId = vaccinationProfile.Id,
                    DiseaseId = request.DiseaseId,
                    VaccineId = request.VaccineId,
                    ExpectedInjectionDate = expectedInjectionDate,
                    ActualInjectionDate = actualInjectionDate
                };

                // Lưu vào database
                context.VaccinationDetails.Add(vaccinationDetail);
                await context.SaveChangesAsync();

                return vaccinationDetail;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating vaccination detail: {ex.Message}", ex);
            }
        }

        //public async Task<VaccinationDetail> Update(int id, UpdateVaccinationDetailDTO request)
        //{
        //    var vaccinationDetail = await context.VaccinationDetails.FindAsync(id);
        //    if (vaccinationDetail == null)
        //    {
        //        throw new Exception("Vaccination detail not found.");
        //    }

        //    // Update Disease and Vaccine IDs
        //    if (request.DiseaseId.HasValue)
        //        vaccinationDetail.DiseaseId = request.DiseaseId;

        //    if (request.VaccineId.HasValue)
        //        vaccinationDetail.VaccineId = request.VaccineId;

        //    // Update Expected Injection Date
        //    if (request.ExpectedInjectionDate.HasValue)
        //        vaccinationDetail.ExpectedInjectionDate = request.ExpectedInjectionDate.HasValue ? request.ExpectedInjectionDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null;

        //    // Ensure ActualInjectionDate is not earlier than ExpectedInjectionDate
        //    if (request.ActualInjectionDate.HasValue)
        //    {
        //        var actualInjectionDateTime = request.ActualInjectionDate.Value.ToDateTime(TimeOnly.MinValue);
        //        if (vaccinationDetail.ExpectedInjectionDate.HasValue &&
        //            actualInjectionDateTime < vaccinationDetail.ExpectedInjectionDate.Value)
        //        {
        //            throw new Exception("Actual injection date cannot be earlier than the expected injection date.");
        //        }
        //        vaccinationDetail.ActualInjectionDate = actualInjectionDateTime;
        //    }

        //    await context.SaveChangesAsync();
        //    return vaccinationDetail;
        //}

        public async Task<VaccinationDetail> GetById(int id)
        {
            try
            {
                var vaccinationDetail = await context.VaccinationDetails.FindAsync(id);
                if (vaccinationDetail == null)
                {
                    throw new Exception("Vaccination detail not found.");
                }
                return vaccinationDetail;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vaccination detail: {ex.Message}", ex);
            }
        }

        public async Task<bool> Delete(int id)
        {
            var vaccinationDetail = await context.VaccinationDetails.FindAsync(id);
            if (vaccinationDetail == null)
            {
                throw new Exception("Vaccination detail not found.");
            }
            context.VaccinationDetails.Remove(vaccinationDetail);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
