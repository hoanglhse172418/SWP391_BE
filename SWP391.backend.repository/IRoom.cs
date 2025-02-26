using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IRoom
    {
        Task<List<Room>> GetAll();
        Task<Room> Create(string roomNumber);
        Task<Room> Update(int id, string roomNumber);
        Task<Room> GetById(int id);
        Task<bool> Delete(int id);
    }
}
