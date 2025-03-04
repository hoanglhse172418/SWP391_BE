using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.NewFolder
{
    public static class AppointmentStatus
    {
        public const string Pending = "Pending";          // Đang chờ xác nhận
        public const string Processing = "Processing";    // Đang xử lý      
        public const string Completed = "Completed";      // Hoàn thành
        public const string Canceled = "Canceled";        // Đã hủy
    }
}
