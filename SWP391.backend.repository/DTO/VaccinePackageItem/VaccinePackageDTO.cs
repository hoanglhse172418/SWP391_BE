using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.VaccinePackageItem
{
    public class VaccinePackageDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<VaccinePackageItemDTO> VaccinePackageItems { get; set; } = new();
    }

}
