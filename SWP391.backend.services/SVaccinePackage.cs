using Microsoft.Extensions.Configuration;
using SWP391.backend.repository.DTO.VaccinePackage;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SVaccinePackage
    {
        private readonly IConfiguration _configuration;

        private readonly swpContext _swpContext;

        public SVaccinePackage(IConfiguration configuration, swpContext swpContext)
        {
            _configuration = configuration;
            _swpContext = swpContext;
        }
    }
}
