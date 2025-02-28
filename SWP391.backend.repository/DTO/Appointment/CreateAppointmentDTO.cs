using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Appointment
{
    public class CreateAppointmentDTO
    {      
        public string ChildFullName { get; set; }
        public string ContactFullName { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string VaccineType { get; set; }
        public string DiaseaseName { get; set; }
        public int? SelectedVaccineId { get; set; }
        public int? SelectedVaccinePackageId { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}
