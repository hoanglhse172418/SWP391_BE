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

                int actualMonth = request.Month;
               
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
                    ActualInjectionDate = null,
                    Month= request.Month,
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

        public async Task<VaccinationDetail> Createbydoctor(CreateVaccinationDetailDTO request)
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
                }

                int actualMonth = request.Month;

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
                    ActualInjectionDate = DateTime.UtcNow.AddHours(7), // Nếu muốn lấy giờ Việt Nam (UTC+7)
                    Month = request.Month,
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

        public async Task<VaccinationDetail> Update(int id, UpdateVaccinationDetailDTO request)
        {
            try
            {
                // Tìm chi tiết tiêm chủng theo ID
                var vaccinationDetail = await context.VaccinationDetails.FindAsync(id);
                if (vaccinationDetail == null)
                {
                    throw new Exception("Vaccination detail not found.");
                }

                // Lấy thông tin hồ sơ tiêm chủng
                var vaccinationProfile = await context.VaccinationProfiles.FindAsync(vaccinationDetail.VaccinationProfileId);
                if (vaccinationProfile == null)
                {
                    throw new Exception("Vaccination profile not found.");
                }

                // Lấy thông tin trẻ em
                var child = await context.Children
                    .FirstOrDefaultAsync(c => c.Id == vaccinationProfile.ChildrenId);
                if (child == null || !child.Dob.HasValue)
                {
                    throw new Exception("Child not found or date of birth is missing.");
                }

                // Cập nhật VaccineId nếu có và nếu nó chưa có giá trị trước đó
                if (request.VaccineId.HasValue && vaccinationDetail.VaccineId == null)
                {
                    vaccinationDetail.VaccineId = request.VaccineId.Value;
                }

                // Kiểm tra nếu Month thay đổi, thì mới cập nhật ActualInjectionDate
                if (request.Month > 0 && request.Month != vaccinationDetail.Month)
                {
                    vaccinationDetail.Month = request.Month;
                }

                // Lưu vào database
                context.VaccinationDetails.Update(vaccinationDetail);
                await context.SaveChangesAsync();

                return vaccinationDetail;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating vaccination detail: {ex.Message}", ex);
            }
        }

        public async Task<VaccinationDetail> UpdateForDoctor(int id, int vaccineId)
        {
            try
            {
                // 1️⃣ Tìm chi tiết tiêm chủng theo ID
                var vaccinationDetail = await context.VaccinationDetails.FindAsync(id);
                if (vaccinationDetail == null)
                {
                    throw new Exception("Vaccination detail not found.");
                }

                // 2️⃣ Kiểm tra xem mũi tiêm có DiseaseId hay không
                if (!vaccinationDetail.DiseaseId.HasValue)
                {
                    throw new Exception("This vaccination detail does not have an associated disease.");
                }
                int diseaseId = vaccinationDetail.DiseaseId.Value;

                // 3️⃣ Tìm hồ sơ tiêm chủng của trẻ
                var vaccinationProfile = await context.VaccinationProfiles.FindAsync(vaccinationDetail.VaccinationProfileId);
                if (vaccinationProfile == null)
                {
                    throw new Exception("Vaccination profile not found.");
                }

                // 4️⃣ Lấy giá trị Month từ bảng Template
                var template = await context.VaccineTemplates
                    .FirstOrDefaultAsync(t => t.DiseaseId == diseaseId);
                if (template == null)
                {
                    throw new Exception("No template found for the given disease.");
                }
                int templateMonth = template.Month ?? 0; // Lấy giá trị Month từ bảng Template

                // 5️⃣ Tìm tất cả các mũi tiêm của trẻ có cùng DiseaseId
                var allVaccinationDetails = await context.VaccinationDetails
                    .Where(vd => vd.VaccinationProfileId == vaccinationProfile.Id && vd.DiseaseId == diseaseId)
                    .OrderBy(vd => vd.Month) // Sắp xếp theo mũi tiêm đầu tiên -> mũi tiếp theo
                    .ToListAsync();

                if (!allVaccinationDetails.Any())
                {
                    throw new Exception("No vaccination schedule found for this disease.");
                }

                // 6️⃣ Kiểm tra mũi đầu tiên đã có VaccineId chưa
                var firstDose = allVaccinationDetails.FirstOrDefault();
                if (firstDose != null && firstDose.VaccineId == null)
                {
                    firstDose.VaccineId = vaccineId;
                    firstDose.ActualInjectionDate = DateTime.UtcNow.AddHours(7);
                    firstDose.Month = templateMonth; // Cập nhật giá trị tháng từ Template
                }
                else
                {
                    // Nếu mũi đầu tiên đã có VaccineId, tìm mũi tiếp theo chưa có VaccineId
                    var nextDose = allVaccinationDetails.FirstOrDefault(vd => vd.VaccineId == null);
                    if (nextDose == null)
                    {
                        throw new Exception("All doses have been assigned vaccines.");
                    }

                    nextDose.VaccineId = vaccineId;
                    nextDose.ActualInjectionDate = DateTime.UtcNow.AddHours(7);
                    nextDose.Month = templateMonth; // Cập nhật giá trị tháng từ Template
                }

                // 7️⃣ Lưu vào database
                await context.SaveChangesAsync();

                return firstDose ?? allVaccinationDetails.First(); // Trả về mũi đầu tiên hoặc mũi đã cập nhật
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating vaccination detail: {ex.Message}", ex);
            }
        }

        public async Task<VaccinationDetail> UpdateExpectedDatebyDoctor(int id, DateOnly expectedDay)
        {
            try
            {
                // Tìm chi tiết tiêm chủng theo ID
                var vaccinationDetail = await context.VaccinationDetails.FindAsync(id);
                if (vaccinationDetail == null)
                {
                    throw new Exception("Vaccination detail not found.");
                }

                // Tìm hồ sơ tiêm chủng
                var vaccinationProfile = await context.VaccinationProfiles.FindAsync(vaccinationDetail.VaccinationProfileId);
                if (vaccinationProfile == null)
                {
                    throw new Exception("Vaccination profile not found.");
                }

                // Lấy thông tin trẻ em
                var child = await context.Children
                    .FirstOrDefaultAsync(c => c.Id == vaccinationProfile.ChildrenId);
                if (child == null || !child.Dob.HasValue)
                {
                    throw new Exception("Child not found or date of birth is missing.");
                }

                // Cập nhật ngày tiêm mong đợi cho lần tiêm hiện tại
                if (vaccinationDetail.Month == null || vaccinationDetail.Month == 0)
                {
                    vaccinationDetail.ExpectedInjectionDate = expectedDay.ToDateTime(TimeOnly.MinValue);
                }

                // Lưu thay đổi vào database
                context.VaccinationDetails.Update(vaccinationDetail);
                await context.SaveChangesAsync();

                // **Tìm tất cả các lần tiêm của cùng một bệnh trong hồ sơ này**
                var vaccinations = await context.VaccinationDetails
                    .Where(v => v.VaccinationProfileId == vaccinationProfile.Id &&
                                v.DiseaseId == vaccinationDetail.DiseaseId)
                    .OrderBy(v => v.ExpectedInjectionDate)
                    .ToListAsync();

                // **Cập nhật ExpectedInjectionDate cho các lần tiêm tiếp theo**
                for (int i = 0; i < vaccinations.Count - 1; i++)
                {
                    if (vaccinations[i].Id == id)  // Tìm vị trí lần tiêm hiện tại
                    {
                        var nextVaccination = vaccinations[i + 1];

                        // Nếu nextVaccination.Month có giá trị, không cập nhật ExpectedInjectionDate
                        if (nextVaccination.Month == null || nextVaccination.Month == 0)
                        {
                            // Giả sử khoảng cách giữa các lần tiêm là 1 tháng (có thể thay đổi)
                            nextVaccination.ExpectedInjectionDate = expectedDay.AddMonths(1).ToDateTime(TimeOnly.MinValue);
                            context.VaccinationDetails.Update(nextVaccination);
                            await context.SaveChangesAsync();
                        }
                    }
                }

                return vaccinationDetail;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating vaccination detail: {ex.Message}", ex);
            }
        }

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

            // Thay vì xóa, cập nhật VaccineId và ActualInjectionDate thành null
            vaccinationDetail.VaccineId = null;
            vaccinationDetail.ActualInjectionDate = null;
            vaccinationDetail.Month = null;
            await context.SaveChangesAsync();
            return true;
        }

    }
}
