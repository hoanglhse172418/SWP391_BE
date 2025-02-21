using System;
using System.Collections.Generic;

namespace SWP391.backend.repository.Models
{
    public partial class User
    {
        public User()
        {
            Children = new HashSet<Child>();
        }

        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public DateTime? LastLogin { get; set; }

        public virtual ICollection<Child> Children { get; set; }
    }
}
