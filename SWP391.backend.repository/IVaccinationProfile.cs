using SWP391.backend.repository.DTO;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IVaccinationProfile
    {
        Task<List<VaccinationProfile>> GetAll(GetAllDTO request);
        Task<VaccinationProfile> Create(int childId);
        Task<VaccinationProfile> Update(int profileId);
        Task<VaccinationProfile> GetById(int profileId);
        Task<bool> Delete(int profileId);
    }
}
