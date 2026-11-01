using Berwah.Models.Enum;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface ISupervisorForAdRepository
    {
        Task<IEnumerable<Advertisment>> GetAdsByStatusAsync(AdStatus status, int page, int pageSize);
        Task<int> GetAdsCountByStatusAsync(AdStatus status);
        Task<Advertisment?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(Advertisment ad);
    }
}
