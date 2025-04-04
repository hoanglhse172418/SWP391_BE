using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Vaccine;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IVaccine
    {
        Task<List<Vaccine>> GetAllVaccine();
        Task<List<Vaccine>> GetAllVaccineForUser();
        Task<Vaccine> GetById(int id);
        Task<Vaccine> Create(CreateVaccineDTO request, string imageUrl);
        Task<Vaccine> Update(int vaccineId, UpdateVaccineDTO request, string imageUrl);
        Task<List<Vaccine>> GetAllVaccinesByDiasease(string diaseaseName);
        Task<bool> Delete(int vaccineId);
    }
}
