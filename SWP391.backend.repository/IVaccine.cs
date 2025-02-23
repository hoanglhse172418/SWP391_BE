﻿using SWP391.backend.repository.DTO;
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
        Task<List<Vaccine>> GetAllVaccine(GetAllDTO request);
        Task<Vaccine> Create(CreateVaccineDTO request);
        Task<Vaccine> Update(int vaccineId, UpdateVaccineDTO request);
    }
}
