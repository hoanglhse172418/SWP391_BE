public class CustomerAppointmentsDTO
{
    public List<SingleVaccineAppointmentDTO> SingleVaccineAppointments { get; set; } = new();
    public List<PackageVaccineAppointmentDTO> PackageVaccineAppointments { get; set; } = new();
}

public class SingleVaccineAppointmentDTO
{
    public int Id { get; set; }
    public int? ChildrenId { get; set; }
    public string? ChildFullName { get; set; }
    public string? ContactPhoneNumber { get; set; }
    public string? DiseaseName { get; set; }
    public int? VaccineId { get; set; }
    public string? VaccineName { get; set; }
    public DateTime? DateInjection { get; set; }
    public string? Status { get; set; }
    public string? ProcessStep { get; set; }
    public int? PaymentId { get; set; }
    public string? InjectionNote { get; set; }
}

public class PackageVaccineAppointmentDTO
{
    public int VaccinePackageId { get; set; }
    public string? VaccinePackageName { get; set; }
    public int? ChildrenId { get; set; }
    public string? ChildFullName { get; set; }
    public string? ContactPhoneNumber { get; set; }
    public List<VaccineItemDTO> VaccineItems { get; set; } = new();
}

public class VaccineItemDTO
{
    public int Id { get; set; }
    public int? VaccineId { get; set; }
    public string? VaccineName { get; set; }
    public int DoseSequence { get; set; }
    public DateTime? DateInjection { get; set; }
    public string? Status { get; set; }
    public string? ProcessStep { get; set; }
    public int? PaymentId { get; set; }
    public string? InjectionNote { get; set;}
}
