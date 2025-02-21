using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository.DTO
{
    public class GetAllDTO
    {
        public string? FilterOn { get; set; }// Field to filter on row
        public string? FilterQuery { get; set; }// The value to filter by
        public string? SortBy { get; set; }// Field to sort by
        public bool? IsAscending { get; set; }// Whether to sort ascending (true) or descending (false)
        public int? PageNumber { get; set; } = 1; // Current page number (default is 1)
        public int? PageSize { get; set; } = 10; // Number of items per page (default is 10)
    }
}
