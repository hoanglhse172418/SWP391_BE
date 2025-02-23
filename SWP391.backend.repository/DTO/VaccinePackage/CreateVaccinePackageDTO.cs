using SWP391.backend.repository.DTO.VaccinePackageItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.VaccinePackage
{
    public class CreateVaccinePackageDTO
    {
        public string Name { get; set; } = string.Empty;
        public List<VaccinePackageItemDTO> VaccinePackageItems { get; set; } = new();
    }
}
