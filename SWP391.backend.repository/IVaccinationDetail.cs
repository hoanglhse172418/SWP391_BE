﻿using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.VaccinationDetail;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IVaccinationDetail
    {
        Task<List<VaccinationDetail>> GetAll(GetAllDTO request);
        Task<VaccinationDetail> Create(CreateVaccinationDetailDTO request);
        Task<VaccinationDetail> Createbydoctor(CreateVaccinationDetailDTO request);
        Task<VaccinationDetail> Update(int id, UpdateVaccinationDetailDTO request);
        Task<List<VaccinationDetail>> UpdateForDoctor(int ProfileId, int vaccineId);
        Task<VaccinationDetail> UpdateExpectedDatebyDoctor(int id, DateOnly expectedDay);
        Task<VaccinationDetail> GetById(int id);
        Task<bool> Delete(int id);
    }
}
