using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Appointment
{
    public class InjectionUpdateDTO
    {
        public int AppointmentId { get; set; }
        public DateTime NewDate { get; set; }
    }
}
