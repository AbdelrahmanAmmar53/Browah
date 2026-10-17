using Berwah.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface IHomeService
    { 
        Task<HomeDto> GetHomeDataAsync(string userId);
    }
}
