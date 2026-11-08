using Berwah.Models.DTO.SupervisorUsed;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface ISupervisorUsedRepository
    {
        Task<UsedAdStatusDto> GetStatsAsync();
        Task<IEnumerable<Advertisment>> GetUsedAdsByStatusAsync(AdStatus? status, int page, int pageSize);
        Task<Advertisment?> GetAdByIdAsync(int id);
        Task<bool> UpdateAdAsync(Advertisment ad);
    }
}
