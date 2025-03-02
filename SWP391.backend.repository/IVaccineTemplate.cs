using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.VaccineTemplate;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IVaccineTemplate
    {
        Task<List<VaccineTemplate>> GetAll(GetAllDTO request);
        Task<List<VaccineTemplate>> Create(CreateVaccineTemplateDTO request);
        Task<VaccineTemplate> Update(int id, UpdateVaccineTemplateDTO request);
        Task<VaccineTemplate> GetById(int id);
        Task<bool> Delete(int id);
    }
}
