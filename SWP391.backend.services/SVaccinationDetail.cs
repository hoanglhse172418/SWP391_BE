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

        public async Task<List<VaccinationDetail>> UpdateForDoctor(int profileId, int vaccineId)
        {
            try
            {
                var vaccine = await context.Vaccines
                    .Include(v => v.Diseases)
                    .FirstOrDefaultAsync(v => v.Id == vaccineId);

                if (vaccine == null || !vaccine.Diseases.Any())
                {
                    throw new Exception("No disease found for the given vaccine.");
                }

                var diseaseIds = vaccine.Diseases.Select(d => d.Id).ToList();

                var allVaccinationDetails = await context.VaccinationDetails
                    .Where(vd => vd.VaccinationProfileId == profileId && diseaseIds.Contains(vd.DiseaseId.Value))
                    .OrderBy(vd => vd.DiseaseId)
                    .ThenBy(vd => vd.Month)
                    .ToListAsync();

                if (!allVaccinationDetails.Any())
                {
                    throw new Exception("No vaccination details found for these diseases.");
                }

                List<VaccinationDetail> updatedDetails = new List<VaccinationDetail>();

                foreach (var diseaseId in diseaseIds)
                {
                    var diseaseVaccinationDetails = allVaccinationDetails
                        .Where(vd => vd.DiseaseId == diseaseId)
                        .ToList();

                    if (!diseaseVaccinationDetails.Any()) continue;

                    // 🔍 1️⃣ Tìm mũi tiêm có VaccineId gần nhất
                    var lastDoseWithVaccine = diseaseVaccinationDetails
                        .Where(vd => vd.VaccineId != null)
                        .OrderByDescending(vd => vd.Month)
                        .FirstOrDefault();

                    // 🔍 2️⃣ Lấy danh sách khoảng cách giữa các mũi từ Template
                    var templateDoses = await context.VaccineTemplates
                        .Where(t => t.DiseaseId == diseaseId)
                        .OrderBy(t => t.Month)
                        .ToListAsync();

                    int nextMonth = 0;

                    if (lastDoseWithVaccine != null)
                    {
                        // Nếu đã có mũi tiêm trước đó, tìm khoảng cách phù hợp từ template
                        var nextTemplate = templateDoses
                            .FirstOrDefault(t => t.Month > lastDoseWithVaccine.Month);

                        if (nextTemplate != null)
                        {
                            nextMonth = nextTemplate.Month ?? 0;
                        }
                       
                    }
                    else
                    {
                        // Nếu chưa có mũi nào trước đó, lấy mũi tiêm đầu tiên trong template
                        nextMonth = templateDoses.FirstOrDefault()?.Month ?? 0;
                    }

                    // 🔍 3️⃣ Tìm mũi đầu tiên chưa có VaccineId
                    var firstEmptyDose = diseaseVaccinationDetails.FirstOrDefault(vd => vd.VaccineId == null);

                    if (firstEmptyDose != null)
                    {
                        firstEmptyDose.VaccineId = vaccineId;
                        firstEmptyDose.ActualInjectionDate = DateTime.UtcNow.AddHours(7);
                        firstEmptyDose.Month = nextMonth; // Gán giá trị Month đã tính toán

                        updatedDetails.Add(firstEmptyDose);
                    }
                }

                if (updatedDetails.Any())
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("No updates were made. All vaccines have been assigned.");
                }

                return updatedDetails;
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

                // **Cập nhật ngày tiêm mong đợi cho lần tiêm hiện tại**
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

                // **Cập nhật ExpectedInjectionDate cho tất cả các lần tiêm tiếp theo**
                bool shouldUpdate = false;
                DateOnly lastExpectedDate = expectedDay;

                foreach (var v in vaccinations)
                {
                    if (v.Id == id)
                    {
                        shouldUpdate = true; // Bắt đầu cập nhật từ lần tiêm tiếp theo
                        continue;
                    }

                    if (shouldUpdate)
                    {
                        // Nếu vaccine này có Month thì bỏ qua không cập nhật
                        if (v.Month == null )
                        {
                            lastExpectedDate = lastExpectedDate.AddMonths(1); // Cộng thêm 1 tháng từ lần tiêm trước
                            v.ExpectedInjectionDate = lastExpectedDate.ToDateTime(TimeOnly.MinValue);
                            context.VaccinationDetails.Update(v);
                        }
                    }
                }

                // Lưu toàn bộ thay đổi
                await context.SaveChangesAsync();

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
