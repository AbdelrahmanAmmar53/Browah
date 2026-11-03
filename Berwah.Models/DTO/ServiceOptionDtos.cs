using System;
using System.Collections.Generic;

namespace Berwah.Models.DTO
{
    public class ServiceOptionResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ServiceOptionUpsertDto
    {
        /// <summary>The service (sub-category) the option belongs to. Ignored on update.</summary>
        public int ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
