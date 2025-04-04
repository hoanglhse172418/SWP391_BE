﻿using SWP391.backend.repository.DTO.VaccinePackage;
using SWP391.backend.repository.DTO.VaccinePackageItem;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IVaccinePackage
    {
        Task<VaccinePackage> CreateVaccinePackageAsync(CreateVaccinePackageDTO request);
        Task<VaccinePackage> UpdateVaccinePackageAsync(UpdateVaccinePackageDTO request);
        Task<VaccinePackage?> GetVaccinePackageByIdAsync(int id);
        Task<List<VaccinePackageDTO>> GetAllVaccinePackageAsync();
        Task<bool> DeleteVaccinePackageAsync(int id);
    }
}
