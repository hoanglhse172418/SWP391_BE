﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Vaccine
{
    public class VaccineDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Manufacture { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int? RecAgeStart { get; set; }
        public int? RecAgeEnd { get; set; }
        public int? InStockNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Price { get; set; }
        public List<int> DiseaseIds { get; set; }
    }
}
