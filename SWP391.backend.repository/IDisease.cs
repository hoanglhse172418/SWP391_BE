using SWP391.backend.repository.DTO;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IDisease
    {
        Task<List<Disease>> GetAll(GetAllDTO request);
        Task<Disease> Create(string name);
        Task<Disease> Update(int id, string name);
        Task<Disease> GetById(int id);
        Task<bool> Delete(int id);
    }
}
