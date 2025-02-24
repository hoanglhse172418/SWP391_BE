using SWP391.backend.repository.DTO.Appointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IAppointment
    {
        Task<AppointmentDTO> BookAppointment(CreateAppointmentDTO dto);
        Task<AppointmentDTO> GetAppointment(int id);
        Task<List<AppointmentDTO>> GetAppointmentsToday();
        Task<List<AppointmentDTO>> GetAppointmentsFuture();
    }
}
