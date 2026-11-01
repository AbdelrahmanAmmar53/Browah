using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface IUserService
    {
        string GenerateAndSaveOtp(string key, object registerModel, string userType);
        bool VerifyOtp(string key, string code, out object? registerModel, out string? userType);
        Task<byte[]> ExportUserDataAsExcelAsync(int userId);
    }
}
