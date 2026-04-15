using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string username);
        Task DeleteCookie(string key);
        Task SetCookie(string key, string value, TimeSpan duration);
    }
}
