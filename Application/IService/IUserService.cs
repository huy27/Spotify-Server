using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IUserService
    {
        Task<string> Login(string username, string password);
        Task<bool> Register(string username, string password, string email);
        Task<bool> RegisterAdmin(string username, string password, string email);
    }
}
