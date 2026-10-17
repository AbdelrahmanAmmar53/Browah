using Berwah.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface IAdvertisementDetailService
    {
        Task<AdvertisementDetailDto?> GetAdvertisementDetailAsync(int advertisementId);
        Task<List<SpecificationItemDto>> GetSpecificationsAsync(int advertisementId);
        Task<SellerProfileDto?> GetSellerProfileAsync(int sellerId);
        Task AddSpecificationsAsync(AdvertisementSpecificationDto dto);
        Task DeleteSpecificationAsync(int specificationId);
    }
}