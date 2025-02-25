using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Appointment
{
    public class UpdateAppointmentStatusDTO
    {
        public int? Id { get; set; }
        public string? Status { get; set; }
    }
}
