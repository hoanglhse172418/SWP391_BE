﻿using SWP391.backend.repository.DTO.Appointment;
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
        Task<List<AppointmentDTO>> GetAllAppointment();
        Task<CustomerAppointmentsDTO> GetCustomerAppointmentsAsync();
        //Task<List<AppointmentDTO>> GetAppointmentByChildId(int Id);
        Task<AppointmentDetailDTO?> GetAppointmentByIdAsync(int appointmentId);
        Task<bool> ConfirmAppointmentAsync(int appointmentId, EditAppointmentDetailDTO dto);
        Task<bool> UpdateAppointmentForDoctorAsync(int appointmentId);
        Task<List<TodayAppointmentDTO>> GetAppointmentsToday();
        Task<List<FutureAppointmentDTO>> GetAppointmentsFuture();
    }
}
