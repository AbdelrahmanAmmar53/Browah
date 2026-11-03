using System;
using System.Collections.Generic;

namespace Berwah.Models.Models
{
    /// <summary>
    /// A choice inside a service (a vehicle type for "وصلني", a kind of work for
    /// "صيانة"). The service is a <see cref="SubCategory"/> of a Services category.
    /// </summary>
    public class ServiceOption
    {
        public int Id { get; set; }
        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
