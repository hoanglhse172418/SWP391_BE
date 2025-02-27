public class CustomerAppointmentsDTO
{
    public List<SingleVaccineAppointmentDTO> SingleVaccineAppointments { get; set; } = new();
    public List<PackageVaccineAppointmentDTO> PackageVaccineAppointments { get; set; } = new();
}

// DTO cho lịch hẹn vắc xin lẻ
public class SingleVaccineAppointmentDTO
{
    public string ChildFullName { get; set; }
    public string ContactPhoneNumber { get; set; }
    public string VaccineName { get; set; }
    public DateTime DateInjection { get; set; }
    public string Status { get; set; }

    public DateTime AppointmentCreatedDate { get; set; }
}

// DTO cho lịch hẹn gói vắc xin
public class PackageVaccineAppointmentDTO
{
    public string? ChildFullName { get; set; }
    public string? ContactPhoneNumber { get; set; }
    public string? VaccinePackageName { get; set; }
    public DateTime DateInjection { get; set; }
    public string? Status { get; set; }
    public DateTime AppointmentCreatedDate { get; set; }
    public List<FollowUpAppointmentDTO> FollowUpAppointments { get; set; } = new();
}

// DTO cho từng mũi trong gói
public class FollowUpAppointmentDTO
{
    public string? VaccineName { get; set; }
    public int DoseNumber { get; set; }
    public DateTime DateInjection { get; set; }
    public string? Status { get; set; }
}
