using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO.Account
{
    public class CreateStaffDTO
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
    }
}
