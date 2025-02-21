using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Child;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IChild
    {
        Task<List<Child>> GetAll(GetAllDTO request);
        Task<Child> Create(CreateChildDTO request);
        Task<Child> Update(int Id, UpdateChildDTO request);
        Task<Child> GetById(int Id);
        Task<bool> Delete(int Id);
    }
}
