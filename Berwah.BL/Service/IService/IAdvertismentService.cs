using Berwah.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface IAdvertismentService
    {
        Task<AdvertisementResponseDto> CreateAdvertisementAsync(CreateAdvertisementDto dto, int userId);
        Task<AdvertisementResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<AdvertisementResponseDto>> GetUserAdvertisementsAsync(int userId);
    }
}
