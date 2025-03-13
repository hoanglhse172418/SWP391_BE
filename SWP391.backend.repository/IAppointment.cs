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
        Task<List<AppointmentDTO>> BookAppointment(CreateAppointmentDTO dto);
        Task<List<AppointmentDTO>> GetAllAppointment();
        Task<CustomerAppointmentsDTO> GetCustomerAppointmentsAsync();
        Task<List<AppointmentDTO>> GetAppointmentByChildId(int childId);
        Task<AppointmentDetailDTO?> GetAppointmentByIdAsync(int appointmentId);
        Task<int> ConfirmAppointmentAsync(int appointmentId, EditAppointmentDetailDTO dto);
        Task<List<TodayAppointmentDTO>> GetAppointmentsToday();
        Task<List<FutureAppointmentDTO>> GetAppointmentsFuture();
        Task<bool> ConfirmInjectionAsync(int appointmentId);
        Task<bool> UpdateMultipleInjectionDatesAsync(List<(int appointmentId, DateTime newDate)> updates);
        Task<CustomerAppointmentsDTO> GetAppointmentsFromBuyingPackageAsync(int childId);
        Task<bool> CancelAppointmentAsync(int appointmentId);
        Task<bool> UpdateInjectionNoteAsync(int appointmentId, EditInjectionNoteDTO dto);
        Task<List<AppointmentDTO>> GetAppointmentsByPackageAndPaymentAsync(int appointmentId);
    }
}
