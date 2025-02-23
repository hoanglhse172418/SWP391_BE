using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class Room
    {
        public Room()
        {
            Appointments = new HashSet<Appointment>();
        }

        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!;

        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}
