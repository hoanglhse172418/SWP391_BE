using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Account;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.repository
{
    public interface IUser
    {
        Task<List<User>> GetAll(GetAllDTO request);
        Task<User> CreateUser(CreateUserDTO request);
        Task<User> CreateStaff(CreateStaffDTO request);
        Task<User> CreateDoctor(CreateStaffDTO request);
        Task<User> Update(int id, UpdateUserDTO user);
        Task<User> GetByID(int id);
        Task<bool> Delete(int id);
        Task ForgotPassword(string email);
        Task ResetPassword(string token, string newPassword, string confirmPassword);
        Task<string> Login(LoginDTO request);
        Task<bool> Logout(string email);
    }
}
