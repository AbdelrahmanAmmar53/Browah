using System.Linq;
using System.Text.RegularExpressions;

namespace Berwah.BL.Service
{
    /// <summary>
    /// Where tapping a notification should lead: a kind ("order", "service", "job", "chat", "other")
    /// and the id of the thing it is about. Worked out from the notification text, so every place that
    /// sends a notification gets a destination without changing; a sender that knows better says it itself.
    /// </summary>
    public static class NotificationTarget
    {
        private static readonly Regex Hash = new(@"#(\d+)", RegexOptions.Compiled);

        // Sent to the service provider about a request he offered on.
        private static readonly string[] JobTitles =
        {
            "تم قبول عرضك", "تم اختيار مقدم آخر", "الطلب متاح من جديد", "أكد العميل استلام",
            "تم تأكيد المهمة تلقائيًا", "تم تغيير مقدم الخدمة"
        };

        // Sent to the customer about his service request.
        private static readonly string[] ServiceHints =
        {
            "مقدم الخدمة", "وصلك عرض", "تنفيذ طلبك", "تم تنفيذ طلبك", "تم تأكيد طلبك تلقائيًا"
        };

        // Order notifications that carry no number.
        private static readonly string[] OrderTitles = { "تحديث طلبك", "اعتذار عن جزء من الطلب", "تم رد مبلغ" };

        // Everything else the system sends that has no page of its own to open.
        private static readonly string[] OtherTitles =
        {
            "طلب سحب", "تحديث معاملة", "وثيقة كابتن", "الرخصة", "القيادة", "الباقة", "إعلان", "الإعلان",
            "تمت إضافة المنتج", "ملف", "حسابك"
        };

        public static (string Type, int? RefId) Resolve(string? title, string? message, string? type = null, int? refId = null)
        {
            if (!string.IsNullOrEmpty(type)) return (type, refId);

            title ??= string.Empty;
            message ??= string.Empty;

            var m = Hash.Match(message);
            if (!m.Success) m = Hash.Match(title);
            int? id = m.Success && int.TryParse(m.Groups[1].Value, out var n) ? n : null;

            if (id != null)
            {
                if (JobTitles.Any(title.Contains)) return ("job", id);
                if (ServiceHints.Any(h => title.Contains(h) || message.Contains(h))) return ("service", id);
                return ("order", id);
            }

            if (OrderTitles.Any(title.Contains)) return ("order", null);
            if (OtherTitles.Any(title.Contains)) return ("other", null);
            // What is left is a chat message: its title is the sender's name.
            return ("chat", null);
        }
    }
}
