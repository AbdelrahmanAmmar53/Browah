using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Enum
{
    public enum OrderStatus
    {
        PendingAdminApproval,
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        // Appended last on purpose: the numbers stored in the database stay valid.
        // The courier has picked everything up and is on the way to the customer.
        OutForDelivery
    }
}
