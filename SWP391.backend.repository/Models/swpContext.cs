using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace SWP391.backend.repository.Models
{
    public partial class swpContext : DbContext
    {
        public swpContext()
        {
        }

        public swpContext(DbContextOptions<swpContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Appointment> Appointments { get; set; } = null!;
        public virtual DbSet<Child> Children { get; set; } = null!;
        public virtual DbSet<Disease> Diseases { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<PaymentDetail> PaymentDetails { get; set; } = null!;
        public virtual DbSet<Room> Rooms { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<VaccinationDetail> VaccinationDetails { get; set; } = null!;
        public virtual DbSet<VaccinationProfile> VaccinationProfiles { get; set; } = null!;
        public virtual DbSet<Vaccine> Vaccines { get; set; } = null!;
        public virtual DbSet<VaccinePackage> VaccinePackages { get; set; } = null!;
        public virtual DbSet<VaccinePackageItem> VaccinePackageItems { get; set; } = null!;
        public virtual DbSet<VaccineTemplate> VaccineTemplates { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer(getConnectionstring());
            }
        }

        private string getConnectionstring()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true).Build();
            return configuration["ConnectionStrings:swp"];
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("Appointment");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ChildrenId).HasColumnName("children_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateInjection).HasColumnType("date");

                entity.Property(e => e.DiseaseName)
                    .HasMaxLength(255)
                    .HasColumnName("diseaseName");

                entity.Property(e => e.DoctorId).HasColumnName("doctor_id");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Phone)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("phone");

                entity.Property(e => e.ProcessStep)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("processStep");

                entity.Property(e => e.Status)
                    .HasMaxLength(255)
                    .HasColumnName("status");

                entity.Property(e => e.Type)
                    .HasMaxLength(255)
                    .HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.VaccineId).HasColumnName("vaccine_id");

                entity.Property(e => e.VaccinePackageId).HasColumnName("vaccine_package_id");

                entity.HasOne(d => d.Children)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.ChildrenId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Appointme__child__04E4BC85");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("FK_Appointment_Room");

                entity.HasOne(d => d.Vaccine)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.VaccineId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Appointme__vacci__06CD04F7");

                entity.HasOne(d => d.VaccinePackage)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.VaccinePackageId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__Appointme__vacci__05D8E0BE");
            });

            modelBuilder.Entity<Child>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.ChildrenFullname)
                    .HasMaxLength(255)
                    .HasColumnName("children_fullname");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Dob)
                    .HasColumnType("date")
                    .HasColumnName("dob");

                entity.Property(e => e.FatherFullName).HasMaxLength(255);

                entity.Property(e => e.FatherPhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasMaxLength(10)
                    .HasColumnName("gender");

                entity.Property(e => e.MotherFullName).HasMaxLength(255);

                entity.Property(e => e.MotherPhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Children__user_i__628FA481");
            });

            modelBuilder.Entity<Disease>(entity =>
            {
                entity.ToTable("Disease");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.HasMany(d => d.Vaccines)
                    .WithMany(p => p.Diseases)
                    .UsingEntity<Dictionary<string, object>>(
                        "VaccineDisease",
                        l => l.HasOne<Vaccine>().WithMany().HasForeignKey("VaccineId").HasConstraintName("FK__VaccineDi__vacci__6C190EBB"),
                        r => r.HasOne<Disease>().WithMany().HasForeignKey("DiseaseId").HasConstraintName("FK__VaccineDi__disea__6B24EA82"),
                        j =>
                        {
                            j.HasKey("DiseaseId", "VaccineId").HasName("PK__VaccineD__3E3B4E9E9C97F7B9");

                            j.ToTable("VaccineDisease");

                            j.IndexerProperty<int>("DiseaseId").HasColumnName("disease_id");

                            j.IndexerProperty<int>("VaccineId").HasColumnName("vaccine_id");
                        });
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payment");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");

                entity.Property(e => e.InjectionProcessStatus)
                    .HasMaxLength(255)
                    .HasColumnName("injection_process_status");

                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(255)
                    .HasColumnName("payment_method");

                entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(255)
                    .HasColumnName("payment_status");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("total_price");

                entity.Property(e => e.TransactionId)
                    .HasMaxLength(500)
                    .HasColumnName("transactionID");

                entity.HasOne(d => d.Appointment)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Payment__appoint__09A971A2");
            });

            modelBuilder.Entity<PaymentDetail>(entity =>
            {
                entity.ToTable("PaymentDetail");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");

                entity.Property(e => e.DoseNumber).HasColumnName("dose_number");

                entity.Property(e => e.DoseRemaining).HasColumnName("dose_remaining");

                entity.Property(e => e.PaymentId).HasColumnName("payment_id");

                entity.Property(e => e.PricePerDose)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("price_per_dose");

                entity.Property(e => e.VaccineId).HasColumnName("vaccine_id");

                entity.HasOne(d => d.Appointment)
                    .WithMany(p => p.PaymentDetails)
                    .HasForeignKey(d => d.AppointmentId)
                    .HasConstraintName("FK_PaymentDetails_Appointment");

                entity.HasOne(d => d.Payment)
                    .WithMany(p => p.PaymentDetails)
                    .HasForeignKey(d => d.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__PaymentDe__payme__0C85DE4D");

                entity.HasOne(d => d.Vaccine)
                    .WithMany(p => p.PaymentDetails)
                    .HasForeignKey(d => d.VaccineId)
                    .HasConstraintName("FK__PaymentDe__vacci__0D7A0286");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Room");

                entity.Property(e => e.RoomNumber).HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .HasColumnName("email");

                entity.Property(e => e.Fullname)
                    .HasMaxLength(255)
                    .HasColumnName("fullname");

                entity.Property(e => e.LastLogin)
                    .HasColumnType("datetime")
                    .HasColumnName("last_login");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .HasColumnName("password");

                entity.Property(e => e.ResetToken)
                    .HasMaxLength(255)
                    .HasColumnName("reset_token");

                entity.Property(e => e.ResetTokenExpiry)
                    .HasColumnType("datetime")
                    .HasColumnName("reset_token_expiry");

                entity.Property(e => e.Role)
                    .HasMaxLength(10)
                    .HasColumnName("role");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Username)
                    .HasMaxLength(255)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<VaccinationDetail>(entity =>
            {
                entity.ToTable("VaccinationDetail");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ActualInjectionDate)
                    .HasColumnType("date")
                    .HasColumnName("actual_injection_date");

                entity.Property(e => e.DiseaseId).HasColumnName("disease_id");

                entity.Property(e => e.ExpectedInjectionDate)
                    .HasColumnType("date")
                    .HasColumnName("expected_injection_date");

                entity.Property(e => e.VaccinationProfileId).HasColumnName("vaccination_profile_id");

                entity.Property(e => e.VaccineId).HasColumnName("vaccine_id");

                entity.HasOne(d => d.Disease)
                    .WithMany(p => p.VaccinationDetails)
                    .HasForeignKey(d => d.DiseaseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Vaccinati__disea__778AC167");

                entity.HasOne(d => d.VaccinationProfile)
                    .WithMany(p => p.VaccinationDetails)
                    .HasForeignKey(d => d.VaccinationProfileId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Vaccinati__vacci__76969D2E");

                entity.HasOne(d => d.Vaccine)
                    .WithMany(p => p.VaccinationDetails)
                    .HasForeignKey(d => d.VaccineId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Vaccinati__vacci__787EE5A0");
            });

            modelBuilder.Entity<VaccinationProfile>(entity =>
            {
                entity.ToTable("VaccinationProfile");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ChildrenId).HasColumnName("children_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Children)
                    .WithMany(p => p.VaccinationProfiles)
                    .HasForeignKey(d => d.ChildrenId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Vaccinati__child__73BA3083");
            });

            modelBuilder.Entity<Vaccine>(entity =>
            {
                entity.ToTable("Vaccine");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.ImageUrl).HasColumnName("image_url");

                entity.Property(e => e.InStockNumber).HasColumnName("in_stock_number");

                entity.Property(e => e.Manufacture)
                    .HasMaxLength(255)
                    .HasColumnName("manufacture");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Notes).HasColumnName("notes");

                entity.Property(e => e.Price).HasMaxLength(255);

                entity.Property(e => e.RecAgeEnd).HasColumnName("rec_age_end");

                entity.Property(e => e.RecAgeStart).HasColumnName("rec_age_start");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<VaccinePackage>(entity =>
            {
                entity.ToTable("VaccinePackage");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("total_price");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<VaccinePackageItem>(entity =>
            {
                entity.ToTable("VaccinePackageItem");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DoseNumber).HasColumnName("dose_number");

                entity.Property(e => e.PricePerDose)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("price_per_dose");

                entity.Property(e => e.VaccineId).HasColumnName("vaccine_id");

                entity.Property(e => e.VaccinePackageId).HasColumnName("vaccine_package_id");

                entity.HasOne(d => d.Vaccine)
                    .WithMany(p => p.VaccinePackageItems)
                    .HasForeignKey(d => d.VaccineId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__VaccinePa__vacci__7F2BE32F");

                entity.HasOne(d => d.VaccinePackage)
                    .WithMany(p => p.VaccinePackageItems)
                    .HasForeignKey(d => d.VaccinePackageId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__VaccinePa__vacci__00200768");
            });

            modelBuilder.Entity<VaccineTemplate>(entity =>
            {
                entity.ToTable("VaccineTemplate");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AgeRange)
                    .HasMaxLength(255)
                    .HasColumnName("age_range");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.DiseaseId).HasColumnName("disease_id");

                entity.Property(e => e.DoseNumber).HasColumnName("dose_number");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Notes).HasColumnName("notes");

                entity.HasOne(d => d.Disease)
                    .WithMany(p => p.VaccineTemplates)
                    .HasForeignKey(d => d.DiseaseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__VaccineTe__disea__6EF57B66");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
