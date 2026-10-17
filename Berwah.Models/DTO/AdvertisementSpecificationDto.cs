using System;
using System.Collections.Generic;

namespace Berwah.Models.DTO
{
    public class AdvertisementSpecificationDto
    {
        public int AdvertisementId { get; set; }
        public List<SpecificationItemDto> Specifications { get; set; } = new();
    }

    public class SpecificationItemDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}