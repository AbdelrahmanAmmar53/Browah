using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Berwah.Localization
{
    /// <summary>
    /// Translates the server's fixed Arabic texts (messages, notification titles/bodies,
    /// default names) to English when a request sends <c>Accept-Language: en</c>.
    /// Texts written by users (product names, descriptions...) are left untouched.
    /// </summary>
    public static class ArabicToEnglish
    {
        private static readonly Regex ArabicLetters = new(@"[\u0600-\u06FF]", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> Exact = new()
        {
            ["بيانات الجهاز غير صحيحة"] = "Invalid device data",
            ["طلب سحب"] = "Withdrawal request",
            ["طلب سحب من تاجر 💸"] = "Merchant withdrawal request 💸",
            ["تقييم جديد ⭐"] = "New review ⭐",
            ["رد على تقييمك 💬"] = "Reply to your review 💬",
            ["تم إيقاف استقبال الطلبات مؤقتًا"] = "Orders are paused for now",
            ["عاد المتجر لاستقبال الطلبات"] = "The store is taking orders again",
            ["اسم القسم غير صالح"] = "Invalid section name",
            ["القسم غير موجود"] = "The category doesn't exist",
            ["تم تعديل اسم القسم"] = "Section renamed",
            ["تم حذف القسم"] = "Section deleted",
            ["لا يمكن حذف قسم يحتوي على أصناف. انقل الأصناف أو احذفها أولًا."] = "A section with items can't be deleted. Move or delete its items first.",
            ["التقييم من 1 إلى 5"] = "Rating must be from 1 to 5",
            ["يمكن التقييم بعد استلام الطلب"] = "You can rate after receiving the order",
            ["هذا المتجر ليس ضمن الطلب"] = "This store isn't part of the order",
            ["قيّمت هذا المتجر على هذا الطلب من قبل"] = "You already rated this store on this order",
            ["شكرًا لتقييمك"] = "Thanks for your rating",
            ["اكتب الرد"] = "Write a reply",
            ["التقييم غير موجود"] = "Review not found",
            ["تم إرسال الرد"] = "Reply sent",
            ["انتهت رخصة القيادة ⛔"] = "Driving license expired ⛔",
            ["تنبيه انتهاء الرخصة ⚠️"] = "License expiry notice ⚠️",
            ["انتهت رخصة القيادة. حدّث بياناتها وارفع نسخة جديدة."] = "Your driving license has expired. Update its details and upload a new copy.",
            ["وثيقة كابتن جديدة 📄"] = "New captain document 📄",
            ["بلاغ مشكلة من مندوب ⚠️"] = "Courier problem report ⚠️",
            ["المندوب يحاول الوصول إليك 📞"] = "The courier is trying to reach you 📞",
            ["العميل لا يرد"] = "Customer isn't answering",
            ["العنوان غير صحيح"] = "Wrong address",
            ["المتجر مغلق"] = "Store is closed",
            ["الطلب ناقص"] = "Order is incomplete",
            ["مشكلة أخرى"] = "Other problem",
            ["نوع الوثيقة غير صحيح"] = "Invalid document type",
            ["الصورة مطلوبة"] = "A photo is required",
            ["حجم الصورة كبير جدًا"] = "The image is too large",
            ["تعذر رفع الصورة، حاول مرة أخرى"] = "Couldn't upload the image, try again",
            ["تم رفع الوثيقة"] = "Document uploaded",
            ["الطلب غير موجود أو غير مسجل لك"] = "The order doesn't exist or isn't assigned to you",
            ["اكتب وصف المشكلة"] = "Describe the problem",
            ["تم إرسال البلاغ للإدارة"] = "Report sent to the admin",
            ["تم الاعتذار عن المهمة"] = "You stepped back from the job",
            ["لا يمكن الاعتذار عن هذه المهمة الآن"] = "You can't step back from this job now",
            ["لا يمكن تغيير مقدم الخدمة بعد بدء التنفيذ"] = "The provider can't be changed once work has started",
            ["تم إلغاء اختيار مقدم الخدمة، اختر عرضًا آخر"] = "The provider was unselected, pick another offer",
            ["المهمة غير موجودة"] = "The job doesn't exist",
            ["لا يمكن بدء هذه المهمة الآن"] = "This job can't be started now",
            ["تم بدء المهمة"] = "The job has started",
            ["لا يمكن إنهاء هذه المهمة الآن"] = "This job can't be finished now",
            ["تم إبلاغ العميل بإنهاء المهمة"] = "The customer was told the job is done",
            ["تم تأكيد هذا الطلب من قبل"] = "This request was already confirmed",
            ["لم ينه مقدم الخدمة المهمة بعد"] = "The provider hasn't finished the job yet",
            ["التقييم يجب أن يكون من 1 إلى 5"] = "The rating must be from 1 to 5",
            ["تم تأكيد إتمام الخدمة"] = "The service was confirmed as done",
            ["لا يمكن بدء محادثة مع نفسك"] = "You can't start a chat with yourself",
            ["المحادثة تُفتح بعد قبول العرض"] = "The chat opens once the offer is accepted",
            ["المحادثة غير متاحة"] = "This chat isn't available",
            ["المحادثة تُفتح بعد إتمام الشراء وقبول المتجر للطلب"] = "The chat opens after the purchase is made and the store accepts the order",
            ["نوع المحادثة غير صحيح"] = "Invalid chat type",
            ["المحادثة غير موجودة"] = "The chat doesn't exist",
            ["الرسالة فارغة"] = "The message is empty",
            ["قدمت عرضًا على هذا الطلب من قبل"] = "You already made an offer on this request",
            ["وقت التحضير غير صالح"] = "The preparation time isn't valid",
            ["تعذر حفظ وقت التحضير"] = "Couldn't save the preparation time",
            ["تم تحديث وقت التحضير بنجاح"] = "Preparation time updated",
            ["مقدم الخدمة غير موجود"] = "The service provider doesn't exist",
            ["الكمية غير صالحة"] = "Invalid quantity",
            ["المنتج غير متاح"] = "The product isn't available",
            ["الكمية المطلوبة أكبر من المتاح"] = "The requested quantity is more than what's available",
            ["اسم الخيار مطلوب"] = "The option name is required",
            ["الخدمة غير موجودة"] = "The service doesn't exist",
            ["الخيار غير موجود"] = "The option doesn't exist",
            ["تم حذف الخيار"] = "Option deleted",
            [" (ملغي)"] = " (cancelled)",
            ["11:00 ص - 02:00 ص"] = "11:00 AM - 02:00 AM",
            ["12 - 15 دقيقة"] = "12 - 15 min",
            ["15 ث"] = "15 s",
            ["18 دقيقة"] = "18 min",
            ["35 - 45 دقيقة"] = "35 - 45 min",
            ["4 ساعات"] = "4 hours",
            ["أكل بيتي"] = "Home food",
            ["اكل بيتي"] = "Home food",
            ["إعدادات الملاحة والتوجيه الصوتي"] = "Navigation and voice guidance settings",
            ["استلام من أكثر من محل"] = "Pickup from several stores",
            ["استلم الطلب من كل المحلات أولاً"] = "Pick the order up from all stores first",
            ["اسم المقر/المركز مطلوب"] = "The branch/center name is required",
            ["اعتذار عن جزء من الطلب ❌"] = "Part of your order was cancelled ❌",
            ["الإعلان غير موجود أو ليس لديك صلاحية لحذفه"] = "The ad doesn't exist or you aren't allowed to delete it",
            ["الإعلان غير موجود"] = "The ad doesn't exist",
            ["البائع غير موجود"] = "The seller doesn't exist",
            ["البريد الإلكتروني أو كلمة المرور غير صحيحة"] = "Incorrect email or password",
            ["البريد الإلكتروني مستخدم بالفعل"] = "This email is already in use",
            ["البقالة"] = "Grocery",
            ["بقالة"] = "Grocery",
            ["البيانات المدخلة غير صحيحة"] = "The data entered is invalid",
            ["التاجر غير موجود"] = "The merchant doesn't exist",
            ["التوصيل للعنوان المحدد"] = "Delivery to the selected address",
            ["الدفع نقداً عند الاستلام"] = "Cash on delivery",
            ["السعر مطلوب"] = "The price is required",
            ["السعر يجب أن يكون قيمة موجبة"] = "The price must be positive",
            ["السلة فارغة"] = "The cart is empty",
            ["السوق العام"] = "General market",
            ["سوق عام"] = "General market",
            ["سوق برواح"] = "Berwah market",
            ["الشروط وسياسة العمل الميداني"] = "Terms and field-work policy",
            ["الطلب أو المعاملة غير موجودة"] = "The order or transaction doesn't exist",
            ["الطلب غير موجود أو غير تابع لمتجرك"] = "The order doesn't exist or doesn't belong to your store",
            ["الطلب غير موجود"] = "The order doesn't exist",
            ["الطلب منتهي بالفعل"] = "The order is already finished",
            ["العربية"] = "Arabic",
            ["العرض غير موجود"] = "The offer doesn't exist",
            ["العنوان المحدد"] = "Selected address",
            ["العنوان المسجل للعميل"] = "The customer's registered address",
            ["الفرع الرئيسي - شارع مصر"] = "Main branch - Misr Street",
            ["الفرع الرئيسي"] = "Main branch",
            ["القسم الرئيسي غير موجود"] = "The main category doesn't exist",
            ["القسم الرئيسي مطلوب"] = "The main category is required",
            ["القسم الفرعي غير موجود"] = "The sub-category doesn't exist",
            ["القسم الفرعي مطلوب"] = "The sub-category is required",
            ["الكابتن غير موجود"] = "The captain doesn't exist",
            ["المبلغ المراد توريده غير صالح"] = "The amount to deposit is invalid",
            ["المتجر مغلق حاليًا ولا يستقبل طلبات"] = "The store is closed and not taking orders right now",
            ["المتجر"] = "Store",
            ["المحافظة مطلوبة"] = "The governorate is required",
            ["المحل غير موجود"] = "The store doesn't exist",
            ["المدينة مطلوبة"] = "The city is required",
            ["المستخدم غير موجود"] = "The user doesn't exist",
            ["المنتج غير موجود أو الكمية غير متوفرة"] = "The product doesn't exist or the quantity isn't available",
            ["الموقع غير صالح"] = "The location is invalid",
            ["الوصف يجب ألا يتجاوز 2000 حرف"] = "The description must not exceed 2000 characters",
            ["اليوم"] = "Today",
            ["بعض المحلات جهزت طلبك والباقي قيد التجهيز"] = "Some stores have prepared your order, the rest are still preparing",
            ["تحديث طلبك 🍔"] = "Order update 🍔",
            ["تحديث معاملة 📦"] = "Transaction update 📦",
            ["تفضيلات وساعات العمل ومناطق التوزيع"] = "Preferences, working hours and delivery zones",
            ["تم إتمام الطلب بنجاح"] = "Order placed successfully",
            ["تم إرسال رمز التحقق إلى بريدك الإلكتروني"] = "A verification code was sent to your email",
            ["تم إرسال رمز التحقق بنجاح"] = "Verification code sent",
            ["تم إرسال طلبك مع المرفق بنجاح"] = "Your request was sent with the attachment",
            ["تم إرسال عرضك للعميل بنجاح"] = "Your offer was sent to the customer",
            ["تم إسناد الطلب لمندوب التوصيل"] = "The order was assigned to a courier",
            ["تم إضافة الوجبة إلى المنيو بنجاح"] = "The dish was added to the menu",
            ["تم إلغاء الطلب من قبل المتجر"] = "The order was cancelled by the store",
            ["تم إلغاء الطلب وتطبيق الخصم المالي الموضح بنجاح"] = "The order was cancelled and the stated deduction applied",
            ["تم إنشاء الحساب بنجاح"] = "Account created successfully",
            ["تم إنشاء القسم بنجاح"] = "Category created successfully",
            ["تم إيقاف استقبال الطلبات مؤقتاً"] = "Order intake paused",
            ["تم اختيار مقدم آخر"] = "Another provider was chosen",
            ["تم اختيار مقدم الخدمة"] = "Provider selected",
            ["تم اختيار منفذ لهذا الطلب بالفعل أو أنه لم يعد متاحًا"] = "A provider was already chosen for this request, or it's no longer available",
            ["تم استلام إعلانك"] = "We received your ad",
            ["تم استلام الطلب من المحلات وهو في طريقه إليك الآن"] = "The order was picked up from the stores and is on its way to you",
            ["تم استلام طلبك ✅"] = "We received your order ✅",
            ["تم استلام طلبك"] = "We received your order",
            ["تم استلام كل الطلب، جاري التوجه للعميل"] = "All items picked up, heading to the customer",
            ["تم اعتماد إعلانك ✅"] = "Your ad was approved ✅",
            ["تم التاكيد بنجاح"] = "Confirmed",
            ["تم التحقق بنجاح"] = "Verified successfully",
            ["تم التسليم والاستلام"] = "Delivered and received",
            ["تم التسليم"] = "Delivered",
            ["تم تأكيد الطلب"] = "Order confirmed",
            ["تم تأكيد طلب السحب الفوري بنجاح وإرسال المبلغ لحسابك."] = "Instant withdrawal confirmed and the amount sent to your account.",
            ["تم تجديد الـ Token بنجاح"] = "Token refreshed",
            ["تم تجميد إعلانك"] = "Your ad was frozen",
            ["تم تجميد الحساب بنجاح"] = "Account frozen",
            ["تم تحديث أقسام المحل"] = "Store categories updated",
            ["تم تحديث البيانات بنجاح"] = "Data updated",
            ["تم تحديث التفضيلات بنجاح"] = "Preferences updated",
            ["تم تحديث حالة الطلب بنجاح وإرسال الإشعار"] = "Order status updated and notification sent",
            ["تم تحديث حالة المعاملة وإرسال الإشعار للطرف الآخر بنجاح"] = "Transaction status updated and the other party notified",
            ["تم تحديث صورة المحل"] = "Store photo updated",
            ["تم تحديث مواعيد العمل بنجاح"] = "Working hours updated",
            ["تم تحديث موقع المحل بنجاح"] = "Store location updated",
            ["تم تسجيل الخروج بنجاح"] = "Signed out",
            ["تم تسجيل الدخول بنجاح"] = "Signed in",
            ["تم تسجيل طلب توريد الكاش بنجاح وفي انتظار مراجعة الإدارة وتسويه الحساب."] = "Cash deposit request recorded; awaiting admin review and settlement.",
            ["تم تسليم الطلب"] = "Order delivered",
            ["تم تسليم طلبك، بالهنا والشفا"] = "Your order was delivered, enjoy!",
            ["تم تعديل القسم الفرعي بنجاح"] = "Sub-category updated",
            ["تم تعديل القسم بنجاح"] = "Category updated",
            ["تم تغيير كلمة المرور بنجاح"] = "Password changed",
            ["تم حذف الإعلان وجميع المواصفات المرتبطة به بنجاح"] = "The ad and all its specifications were deleted",
            ["تم حذف الحساب نهائياً"] = "Account permanently deleted",
            ["تم حذف الخاصية بنجاح"] = "Specification deleted",
            ["تم حذف القسم الفرعي بنجاح"] = "Sub-category deleted",
            ["تم حذف القسم بنجاح"] = "Category deleted",
            ["تم حفظ وتأكيد وسيلة الدفع بنجاح"] = "Payment method saved and confirmed",
            ["تم رفض إعلانك"] = "Your ad was rejected",
            ["تم فتح المتجر لاستقبال الطلبات بنجاح"] = "The store is open for orders",
            ["تم قبول الطلب بنجاح، توجه إلى الموقع لاستلام الشحنة."] = "Order accepted, head to the location to pick it up.",
            ["تم قبول الطلب وجاري التوجه للمطعم"] = "Order accepted, heading to the restaurant",
            ["تم قبول عرضك ✅"] = "Your offer was accepted ✅",
            ["تمت إضافة القسم الرئيسي بنجاح"] = "Main category added",
            ["تمت إضافة القسم الفرعي بنجاح"] = "Sub-category added",
            ["تمت إضافة المنتج ✅"] = "Product added ✅",
            ["تمت إضافة المواصفات بنجاح"] = "Specifications added",
            ["تمت الموافقة على الطلب بنجاح وتحويله إلى رادار المناديب."] = "Order approved and sent to the couriers' radar.",
            ["تمت الموافقة على طلبك"] = "Your order was approved",
            ["توصيل طلبات متجر"] = "Delivery of store orders",
            ["ج.م"] = "EGP",
            ["جاري البحث عن مندوب توصيل..."] = "Looking for a courier...",
            ["جاري التجهيز والخرج الآمن"] = "Preparing and packing safely",
            ["جاهز لتسليم الوجبات للمندوب"] = "Ready to hand the meals to the courier",
            ["جاهز للاستلام الفوري"] = "Ready for immediate pickup",
            ["جلسة العمل منتهية، يرجى تسجيل الدخول من جديد"] = "Your session has expired, please sign in again",
            ["جيد"] = "Good",
            ["حدث خطأ أثناء إتمام الطلب"] = "An error occurred while placing the order",
            ["حدث خطأ أثناء التحقق"] = "An error occurred during verification",
            ["حدث خطأ أثناء تجديد الـ Token"] = "An error occurred while refreshing the token",
            ["حدث خطأ أثناء تسجيل الخروج"] = "An error occurred while signing out",
            ["حدث خطأ أثناء تسجيل الدخول"] = "An error occurred while signing in",
            ["حدث خطأ أثناء تغيير كلمة المرور"] = "An error occurred while changing the password",
            ["حدث خطأ أثناء جلب الإعلانات"] = "An error occurred while loading ads",
            ["حدث خطأ أثناء جلب بيانات الخدمات"] = "An error occurred while loading services",
            ["حدث خطأ أثناء حذف الإعلان"] = "An error occurred while deleting the ad",
            ["حدث خطأ أثناء حفظ الإعلان"] = "An error occurred while saving the ad",
            ["حدث خطأ أثناء حفظ البطاقة"] = "An error occurred while saving the card",
            ["حدث خطأ أثناء حفظ العرض"] = "An error occurred while saving the offer",
            ["حدث خطأ أثناء قبول الطلب"] = "An error occurred while accepting the order",
            ["حدث خطأ أثناء معالجة الطلب"] = "An error occurred while processing the order",
            ["حدد منطقة عملك"] = "Choose your work area",
            ["حساب التاجر غير موجود"] = "The merchant account doesn't exist",
            ["خدمات"] = "Services",
            ["خرائط جوجل المدمجة • التنبيهات الصوتية"] = "Built-in Google Maps • voice alerts",
            ["دفع إلكتروني محفظة"] = "Wallet payment",
            ["دفع إلكتروني"] = "Online payment",
            ["رقم الهاتف مستخدم بالفعل"] = "This phone number is already in use",
            ["رمز إعادة تعيين كلمة المرور - برواح"] = "Password reset code - Berwah",
            ["رمز التحقق غير صحيح أو انتهت صلاحيته"] = "The verification code is wrong or expired",
            ["رمز تأكيد إنشاء الحساب"] = "Account creation code",
            ["رمز تأكيد إنشاء حساب تاجر"] = "Merchant account creation code",
            ["رمز تأكيد الحساب - برواح"] = "Account confirmation code - Berwah",
            ["زمن الاستجابة فوري"] = "Instant response",
            ["سارية حتى نهاية العام"] = "Valid until the end of the year",
            ["سريع جداً"] = "Very fast",
            ["سكوتر توصيل"] = "Delivery scooter",
            ["شحن موجّه داخل العبوة وتحت الرقابة"] = "Shipping packed and supervised",
            ["شريك كابتن معتمد"] = "Certified captain partner",
            ["شكراً لاختيارك برواح"] = "Thank you for choosing Berwah",
            ["صيغة البريد الإلكتروني غير صحيحة"] = "The email format is invalid",
            ["طلب توصيل جديد 🛵"] = "New delivery order 🛵",
            ["طلب جديد 🛒"] = "New order 🛒",
            ["طلب جديد"] = "New order",
            ["طلب خدمة"] = "Service request",
            ["طلب منتج مستعمل جديد"] = "New used-product order",
            ["عذراً، هذا الطلب تم قبوله مسبقاً بواسطة مندوب آخر أو لم يعد متاحاً."] = "Sorry, this order was already accepted by another courier or is no longer available.",
            ["عميل برواح"] = "Berwah customer",
            ["عنوان الإعلان مطلوب"] = "The ad title is required",
            ["عنوان الإعلان يجب ألا يتجاوز 200 حرف"] = "The ad title must not exceed 200 characters",
            ["عنوان العميل"] = "Customer address",
            ["غير متوفر"] = "Not available",
            ["غير مصرح لك بالوصول"] = "You are not authorized",
            ["فريق التدخل السريع لخدمة العمليات"] = "Rapid operations response team",
            ["فشل إلغاء الطلب، تأكد من صحة رقم الطلب"] = "Couldn't cancel the order, check the order number",
            ["فشل إنشاء الحساب"] = "Couldn't create the account",
            ["فشل إنشاء القسم الجديد"] = "Couldn't create the category",
            ["فشل تغيير كلمة المرور"] = "Couldn't change the password",
            ["فشل حذف الحساب"] = "Couldn't delete the account",
            ["فشل رفع الصورة"] = "Couldn't upload the photo",
            ["فشل في إرسال الطلب"] = "Couldn't send the request",
            ["في الطريق إليك"] = "On its way to you",
            ["في انتظار قبول المندوب"] = "Waiting for a courier to accept",
            ["قبول الطلب والتوجه للاستلام"] = "Accept the order and head to pickup",
            ["قيد التجهيز"] = "Preparing",
            ["قيد الطهي والتجهيز"] = "Cooking and preparing",
            ["قيد المعالجة"] = "Processing",
            ["كابتن سحابي معتمد"] = "Certified captain",
            ["كلمات المرور غير متطابقة"] = "Passwords don't match",
            ["كلمة المرور الحالية غير صحيحة أو الجديدة لا تستوفي الشروط"] = "The current password is wrong or the new one doesn't meet the rules",
            ["كلمة المرور وتأكيد كلمة المرور غير متطابقين"] = "Password and confirmation don't match",
            ["كلمة المرور يجب أن تحتوي على أحرف كبيرة وصغيرة وأرقام ورموز، وطولها 9 أحرف على الأقل"] = "The password must include upper and lower case letters, numbers and symbols, and be at least 9 characters",
            ["لا توجد أصناف من هذا المحل في الطلب"] = "There are no items from this store in the order",
            ["لا يمكن حذف قسم فرعي توجد منتجات مرتبطة به"] = "Can't delete a sub-category that has products",
            ["لا يمكن حذف قسم يحتوي على أقسام فرعية مرتبطة به"] = "Can't delete a category that has sub-categories",
            ["لم تتم إضافة العنوان بعد"] = "No address added yet",
            ["لم يتم التغيير مؤخراً"] = "Not changed recently",
            ["مبلغ السحب غير صالح"] = "The withdrawal amount is invalid",
            ["مبلغ السحب يجب أن يكون أكبر من رسوم السحب المقرر"] = "The withdrawal amount must exceed the withdrawal fee",
            ["متجر الشريك"] = "Partner store",
            ["مثالي"] = "Perfect",
            ["مدفوع إلكترونياً"] = "Paid online",
            ["مركز دعم ومساعدة الكابتن 24/7"] = "Captain support center 24/7",
            ["مسبق الدفع"] = "Prepaid",
            ["مستخدم"] = "User",
            ["مستودع برواح / المطعم"] = "Berwah warehouse / restaurant",
            ["مطاعم"] = "Restaurants",
            ["مطبخ بيتي"] = "Home kitchen",
            ["مطعم"] = "Restaurant",
            ["مطبح المشويات الأصيلة والأكلات الشرقية"] = "Authentic grills and Eastern dishes",
            ["معايير الجودة والسمعة المهنية"] = "Quality standards and professional reputation",
            ["معتمدة ومفحوصة بالكامل • تأمين شامل ساري"] = "Fully certified and inspected • full insurance valid",
            ["ممتاز"] = "Excellent",
            ["منتج"] = "Product",
            ["مندوب برواح"] = "Berwah courier",
            ["منذ دقيقة"] = "1 minute ago",
            ["منذ دقيقتين"] = "2 minutes ago",
            ["منذ ساعة"] = "1 hour ago",
            ["منذ ساعتين"] = "2 hours ago",
            ["منذ سنة"] = "1 year ago",
            ["منذ سنتين"] = "2 years ago",
            ["منذ شهر"] = "1 month ago",
            ["منذ شهرين"] = "2 months ago",
            ["منذ لحظات"] = "moments ago",
            ["منذ يوم"] = "1 day ago",
            ["منذ يومين"] = "2 days ago",
            ["منسقة وموثقة رسمياً ✓"] = "Officially documented ✓",
            ["نشط"] = "Active",
            ["نقداً (كاش)"] = "Cash",
            ["نوع البيانات غير مطابق"] = "Data type mismatch",
            ["نوع النشاط مطلوب"] = "The business type is required",
            ["هذا القسم موجود بالفعل"] = "This category already exists",
            ["وثائق الحساب والفيش الجنائي"] = "Account documents and background check",
            ["وصف النشاط مطلوب"] = "The business description is required",
            ["وصلك عرض جديد 💼"] = "You got a new offer 💼",
            ["يجب تسجيل الدخول أولاً أو أن المعرف غير صالح"] = "You need to sign in first, or the id is invalid",
            ["يحتوي طلبك على منتجات مستعملة، سيتم مراجعة الطلب من قبل الإدارة قريباً."] = "Your order contains used products and will be reviewed by the admin soon.",
            ["يرجى إرفاق صورة واحدة على الأقل"] = "Please attach at least one photo",
            ["تحديث طلبك"] = "Order update",
            ["تم قبول طلبك 🛵"] = "Your order was accepted 🛵",
            ["تم تسليم طلبك ✅"] = "Your order was delivered ✅",
            ["تم رد مبلغ إلى حسابك"] = "A refund was issued",
            ["تم الاسترداد"] = "Refunded",
            ["المنتج غير موجود"] = "The product doesn't exist",
            ["تم تحديث المنتج"] = "Product updated",
            ["تم حذف المنتج"] = "Product deleted",
            ["لا يمكن حذف منتج مرتبط بطلبات سابقة. اجعل المخزون صفرًا لإخفائه."] = "Can't delete a product linked to past orders. Set its stock to 0 to hide it.",
            ["بدأ تنفيذ طلبك 🛠️"] = "Your request has started 🛠️",
            ["تم تنفيذ طلبك ✅"] = "Your request was completed ✅",
            ["أكد العميل استلام الخدمة 🎉"] = "The customer confirmed the service 🎉",
            ["تم تغيير مقدم الخدمة"] = "The provider was changed",
            ["اعتذر مقدم الخدمة عن طلبك"] = "The provider stepped back from your request",
            ["الطلب متاح من جديد"] = "The request is available again",
            ["تم تأكيد طلبك تلقائيًا"] = "Your request was confirmed automatically",
            ["تم تأكيد المهمة تلقائيًا"] = "The job was confirmed automatically",
            ["عميل"] = "Customer",
            ["نفدت كمية هذا المنتج"] = "This product is out of stock",
            ["تم إنشاء التذكرة بنجاح"] = "Ticket created successfully",
            ["تم إنشاء السؤال الشائع بنجاح"] = "FAQ created successfully",
            ["تم تحديث رسوم وعمولات النظام بنجاح"] = "System fees and commissions updated",
            ["فشل تحديث حالة الإعلان أو الإعلان غير موجود"] = "Couldn't update the ad status, or the ad doesn't exist",
            ["تم تحديث حالة الإعلان بنجاح"] = "Ad status updated",
            ["معتمد"] = "Approved",
            ["مشوار المطار"] = "Airport ride",
            ["طلب توكتوك"] = "Tuktuk ride",
            ["سيارة ملاكي"] = "Private car",
            ["طلب سوزوكي"] = "Pickup truck",
            ["عربية نقل"] = "Transport truck",
            ["سباكة"] = "Plumbing",
            ["كهرباء"] = "Electrical",
            ["نجارة"] = "Carpentry",
            ["تكييف وتبريد"] = "AC & cooling",
            ["دهانات"] = "Painting",
            ["أجهزة منزلية"] = "Home appliances",
            ["وصلني"] = "Ride",
            ["صيانة"] = "Maintenance",
            ["البيانات الشخصية"] = "Personal data",
            ["المعرف (ID)"] = "ID",
            ["الاسم"] = "Name",
            ["البريد الإلكتروني"] = "Email",
            ["رقم الهاتف"] = "Phone number",
            ["تاريخ الإنضمام"] = "Join date",
            ["الإعلانات"] = "Ads",
            ["العنوان"] = "Address",
            ["السعر"] = "Price",
            ["التاريخ"] = "Date",
            ["طلبات الخدمات"] = "Service requests",
            ["تفاصيل الطلب"] = "Order details",
            ["السبت"] = "Saturday",
            ["الأحد"] = "Sunday",
            ["الاثنين"] = "Monday",
            ["الثلاثاء"] = "Tuesday",
            ["الأربعاء"] = "Wednesday",
            ["الخميس"] = "Thursday",
            ["الجمعة"] = "Friday",
            ["طوال أيام الأسبوع"] = "All week",
            ["برواح"] = "Browah",
            ["كابتن معتمد"] = "Verified captain",
            ["كابتن قيد المراجعة"] = "Captain under review",
            ["لا توجد بيانات كافية"] = "Not enough data yet",
            ["الوثائق موثقة من الإدارة"] = "Documents verified by the admin",
            ["الوثائق قيد المراجعة"] = "Documents under review",
            ["حدث خطأ غير متوقع، حاول مرة أخرى"] = "Something went wrong, please try again",
            ["تم تحديث بيانات المركبة"] = "Vehicle details updated",
            ["تاريخ انتهاء الرخصة غير صالح"] = "The license expiry date isn't valid",
            ["لا يمكنك الاعتذار عن هذا الطلب بعد بدء الاستلام"] = "You can't step back from this order once pickup has started",
            ["تم الاعتذار عن الطلب"] = "You stepped back from the order",
            ["تعذر على المندوب إكمال طلبك 🛵"] = "The courier couldn't complete your order 🛵",
            ["نبحث لك عن مندوب آخر"] = "We're finding you another courier",
            ["لا يمكن تقييم هذا الطلب"] = "This order can't be rated",
            ["تم تقييم هذا الطلب من قبل"] = "This order was already rated",
            ["تم تسجيل تقييمك، شكرًا لك"] = "Your rating was saved, thank you",
            ["قيّمك العميل ⭐"] = "The customer rated you ⭐",
            ["المحادثة تُفتح بعد قبول مندوب التوصيل"] = "The chat opens once a courier accepts the order",
            ["سحب أرباح"] = "Earnings withdrawal",
            ["توريد كاش"] = "Cash deposit",
            ["بطيء"] = "Slow",
            ["الرصيد المتاح لا يكفي لهذا المبلغ"] = "Your available balance doesn't cover this amount",
            ["مبلغ السحب أقل من الحد الأدنى"] = "The withdrawal amount is below the minimum",
            ["المبلغ أكبر من الكاش الموجود معك"] = "The amount is more than the cash you hold",
            ["تم تسجيل طلب السحب، وسيتم التحويل بعد مراجعة الإدارة."] = "Your withdrawal request was recorded; it will be paid after the admin reviews it.",
            ["تم تسجيل توريد الكاش، وفي انتظار مراجعة الإدارة."] = "Your cash deposit was recorded and is waiting for the admin's review.",
        };

        private static readonly (Regex Pattern, string Template)[] Rules = new (Regex, string)[]
        {
            (new Regex("^طلب #(\\d+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Order #{1}"),
            (new Regex("^طلب تاجر سحب ([\\d.]+) ج\\.م\\. راجعه\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "A merchant requested to withdraw {1} EGP. Please review."),
            (new Regex("^قيّمك عميل (\\d+) من 5 على الطلب #(\\d+)\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "A customer rated you {1} out of 5 on order #{2}."),
            (new Regex("^ردّ المتجر على تقييمك للطلب #(\\d+)\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The store replied to your review of order #{1}."),
            (new Regex("^سحب (.+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Withdrawal {1}"),
            (new Regex("^تنتهي رخصة القيادة خلال (\\d+) يوم\\. حدّث بياناتها من حسابك\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Your driving license expires in {1} days. Update its details from your account."),
            (new Regex("^حدّث الكابتن (.+) إحدى وثائقه\\. راجعها\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Captain {1} updated one of their documents. Please review it."),
            (new Regex("^بلاغ على الطلب #(\\d+): (.+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Report on order #{1}: {2}"),
            (new Regex("^المندوب لم يتمكن من الوصول إليك بخصوص طلبك #(\\d+)\\. تأكد من العنوان وارد على الهاتف\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The courier couldn't reach you about your order #{1}. Check your address and answer the phone."),
            (new Regex("^(\\d+) محلات: (.+)$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} stores: {2}"),
            (new Regex("^المندوب (.*) قبل طلبك #(\\d+) وفي طريقه للمحل\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Courier {1} accepted your order #{2} and is heading to the store."),
            (new Regex("^تم تسليم طلبك #(\\d+)، بالهنا والشفا\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Your order #{1} was delivered, enjoy!"),
            (new Regex("^تم رد (.+?) ج\\.م عن الطلب #(\\d+) إلى وسيلة الدفع\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} EGP was refunded for order #{2} to your payment method."),
            (new Regex("^استرداد عن الطلب #(\\d+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Refund for order #{1}"),
            (new Regex("^(.+?) كم رحلة التوصيل$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} km delivery trip"),
            (new Regex("^(\\d+) محلات: (.*)$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} stores: {2}"),
            (new Regex("^آخر تحديث (.+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Last updated {1}"),
            (new Regex("^إعلانك «(.+?)» أصبح منشورًا الآن\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Your ad “{1}” is now live."),
            (new Regex("^إعلانك «(.+?)» قيد المراجعة وسيصلك إشعار عند اعتماده\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Your ad “{1}” is under review; you'll be notified once approved."),
            (new Regex("^اختار العميل عرضك على الطلب #(\\d+)\\. تواصل معه لتنفيذ المهمة\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The customer chose your offer on request #{1}. Contact them to carry out the task."),
            (new Regex("^اختار العميل عرضًا آخر على الطلب #(\\d+)\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The customer chose another offer on request #{1}."),
            (new Regex("^تم إتمام طلبك رقم #(\\d+) وجاري تجهيزه\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Your order #{1} was placed and is being prepared."),
            (new Regex("^تم إلغاء أصناف (.+?) والباقي مستمر$", RegexOptions.Compiled | RegexOptions.Singleline), "{1}'s items were cancelled; the rest continues"),
            (new Regex("^تم إلغاء الطلب من قبل التاجر \\(السبب: (.*)\\)$", RegexOptions.Compiled | RegexOptions.Singleline), "The order was cancelled by the merchant (reason: {1})"),
            (new Regex("^تم الاستلام، متبقي (\\d+) محل$", RegexOptions.Compiled | RegexOptions.Singleline), "Picked up, {1} store(s) left"),
            (new Regex("^تم تجميد إعلانك «(.+?)» مؤقتًا\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Your ad “{1}” was frozen temporarily."),
            (new Regex("^تم تغيير حالة المعاملة الخاصة بك إلى: (.*)$", RegexOptions.Compiled | RegexOptions.Singleline), "Your transaction status changed to: {1}"),
            (new Regex("^تم نشر المنتج «(.+?)» في متجرك\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Your product “{1}” is now published in your store."),
            (new Regex("^تمت مراجعة طلبك المستعمل رقم #(\\d+) واعتماده بنجاح، وجاري إسناده لمندوب التوصيل\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Your used-product order #{1} was reviewed and approved; assigning a courier."),
            (new Regex("^حدث خطأ أثناء إنشاء القسم: (.*)$", RegexOptions.Compiled | RegexOptions.Singleline), "An error occurred while creating the category: {1}"),
            (new Regex("^شريك كابتن منذ (.+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Captain partner since {1}"),
            (new Regex("^طلب #(\\d+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Order #{1}"),
            (new Regex("^طلب خدمة #(\\d+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Service request #{1}"),
            (new Regex("^طلب توصيل #(\\d+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Delivery order #{1}"),
            (new Regex("^طلب من (.+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Order from {1}"),
            (new Regex("^طلب منتجات أو خدمات #(\\d+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Products or services order #{1}"),
            (new Regex("^طلبك رقم #(\\d+) قيد مراجعة الإدارة لاحتوائه على منتجات مستعملة\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Your order #{1} is being reviewed by the admin because it contains used products."),
            (new Regex("^عضو منذ (.+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Member since {1}"),
            (new Regex("^غرامة إلغاء الطلب #(\\d+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Cancellation penalty for order #{1}"),
            (new Regex("^قبل (\\d+) أشهر$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} months ago"),
            (new Regex("^قبل (\\d+) أيام$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} days ago"),
            (new Regex("^قدّم مقدم خدمة عرضًا بسعر (.+?) ج\\.م على طلبك #(\\d+)\\. افتح طلباتي لمراجعته\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "A provider offered {1} EGP on your request #{2}. Open My orders to review it."),
            (new Regex("^كاش مستلم (.+?) ج\\.م$", RegexOptions.Compiled | RegexOptions.Singleline), "Cash received {1} EGP"),
            (new Regex("^للأسف تم رفض إعلانك «(.+?)»\\. يمكنك تعديله وإعادة نشره\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Unfortunately your ad “{1}” was rejected. You can edit and repost it."),
            (new Regex("^متجر (.+?) قام بتحديث حالة طلبك إلى: (.*)$", RegexOptions.Compiled | RegexOptions.Singleline), "Store {1} updated your order status to: {2}"),
            (new Regex("^منذ (\\d+) (أيام|يوم)$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} days ago"),
            (new Regex("^منذ (\\d+) (ساعات|ساعة)$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} hours ago"),
            (new Regex("^منذ (\\d+) (دقائق|دقيقة)$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} minutes ago"),
            (new Regex("^منذ (\\d+) (شهور|شهر)$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} months ago"),
            (new Regex("^منذ (\\d+) (سنوات|سنة)$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} years ago"),
            (new Regex("^نعتذر لك، (.+?) ألغى أصنافه من طلبك #(\\d+) وباقي الطلب مستمر\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Sorry, {1} cancelled its items from your order #{2}; the rest of the order continues."),
            (new Regex("^نعتذر لك، تم إلغاء طلبك #(\\d+) من قبل المتجر\\. يمكنك اختيار متجر آخر\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Sorry, your order #{1} was cancelled by the store. You can order from another store."),
            (new Regex("^نقداً عند الاستلام • تحصيل: (.+?) ج\\.م$", RegexOptions.Compiled | RegexOptions.Singleline), "Cash on delivery • collect: {1} EGP"),
            (new Regex("^وصلك طلب جديد رقم #(\\d+)\\. راجعه واقبله من قائمة الطلبات\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "You received a new order #{1}. Review and accept it from the orders list."),
            (new Regex("^يبعد (.+?) كم عن موقعك$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} km from your location"),
            (new Regex("^يوجد طلب جديد رقم #(\\d+) قريب منك\\. افتح الرادار لقبوله\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "There's a new order #{1} near you. Open the radar to accept it."),
            (new Regex("^يوجد طلب جديد رقم #(\\d+) يحتوي على منتجات مستعملة ويتطلب موافقتك\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "New order #{1} contains used products and needs your approval."),
            (new Regex("^طلب من (.+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Order from {1}"),
            (new Regex("^المتاح من هذا المنتج (\\d+) فقط$", RegexOptions.Compiled | RegexOptions.Singleline), "Only {1} of this product available"),
            (new Regex("^بدأ مقدم الخدمة العمل على الطلب #(\\d+)\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The provider started working on request #{1}."),
            (new Regex("^أنهى مقدم الخدمة الطلب #(\\d+)\\. أكد الاستلام وقيّمه\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The provider finished request #{1}. Confirm receipt and rate it."),
            (new Regex("^أكد العميل إتمام الطلب #(\\d+)\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The customer confirmed request #{1} as done."),
            (new Regex("^أكد العميل إتمام الطلب #(\\d+) وقيّمك (.+?) من 5\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The customer confirmed request #{1} as done and rated you {2} out of 5."),
            (new Regex("^اختار العميل مقدم خدمة آخر للطلب #(\\d+)\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The customer chose another provider for request #{1}."),
            (new Regex("^اعتذر مقدم الخدمة عن الطلب #(\\d+)\\. اختر عرضًا آخر من العروض المتاحة\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The provider stepped back from request #{1}. Pick another of the available offers."),
            (new Regex("^عاد الطلب #(\\d+) متاحًا للاختيار، وعرضك لا يزال قائمًا\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Request #{1} is open for selection again, and your offer still stands."),
            (new Regex("^مرّت 48 ساعة على إنهاء الطلب #(\\d+)، فتم اعتباره مكتملًا\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "48 hours passed since request #{1} was finished, so it was marked complete."),
            (new Regex("^اكتمل الطلب #(\\d+) بعد مرور 48 ساعة على إنهائك له\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "Request #{1} was completed 48 hours after you finished it."),
            (new Regex("^(\\d+) دقيقة$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} min"),
            (new Regex("^(\\d+) ساعات?$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} hours"),
            (new Regex("^سارية حتى (.+)$", RegexOptions.Compiled | RegexOptions.Singleline), "Valid until {1}"),
            (new Regex("^تعذر على المندوب إكمال طلبك #(\\d+)\\. نبحث لك عن مندوب آخر\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The courier couldn't complete your order #{1}. We're finding you another courier."),
            (new Regex("^قيّمك العميل (\\d+) من 5 على الطلب #(\\d+)\\.$", RegexOptions.Compiled | RegexOptions.Singleline), "The customer rated you {1} out of 5 on order #{2}."),
            (new Regex("^(\\d+) ث$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} s"),
            (new Regex("^([\\d.]+) ساعة$", RegexOptions.Compiled | RegexOptions.Singleline), "{1} h"),
        };

        public static string Translate(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || !ArabicLetters.IsMatch(text)) return text;
            var trimmed = text.Trim();
            if (Exact.TryGetValue(trimmed, out var hit)) return hit;
            foreach (var (pattern, template) in Rules)
            {
                var m = pattern.Match(trimmed);
                if (!m.Success) continue;
                var result = template;
                for (int i = 1; i < m.Groups.Count; i++)
                {
                    var part = m.Groups[i].Value;
                    // A captured piece can itself be a known Arabic text (e.g. a status).
                    result = result.Replace("{" + i + "}", Translate(part));
                }
                return result;
            }
            // "السبت، الأحد • 10:00 - 23:30": a day list, then the hours.
            var dot = trimmed.IndexOf('•');
            if (dot > 0)
            {
                var days = trimmed[..dot].Trim().Split('،', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var names = days.Select(d => Exact.TryGetValue(d, out var e) ? e : null).ToList();
                if (names.Count > 0 && names.All(x => x != null))
                    return string.Join(", ", names) + " •" + trimmed[(dot + 1)..];
            }
            return text;
        }

        /// <summary>Walks a JSON document and translates every string value in place.</summary>
        public static void TranslateTree(JsonNode? node)
        {
            switch (node)
            {
                case JsonObject obj:
                    foreach (var key in obj.Select(p => p.Key).ToList())
                    {
                        var child = obj[key];
                        if (child is JsonValue v && v.TryGetValue<string>(out var s)) obj[key] = JsonValue.Create(Translate(s));
                        else TranslateTree(child);
                    }
                    break;
                case JsonArray arr:
                    for (int i = 0; i < arr.Count; i++)
                    {
                        var child = arr[i];
                        if (child is JsonValue v && v.TryGetValue<string>(out var s)) arr[i] = JsonValue.Create(Translate(s));
                        else TranslateTree(child);
                    }
                    break;
            }
        }
    }

    /// <summary>Buffers JSON responses and translates them for English clients.</summary>
    public class EnglishResponseMiddleware
    {
        private readonly RequestDelegate _next;
        public EnglishResponseMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var lang = context.Request.Headers.AcceptLanguage.ToString();
            var path = context.Request.Path;
            if (!lang.TrimStart().StartsWith("en", StringComparison.OrdinalIgnoreCase)
                || context.WebSockets.IsWebSocketRequest
                || path.StartsWithSegments("/orderHub") || path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            var original = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;
            try
            {
                await _next(context);
                buffer.Position = 0;
                var isJson = context.Response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true;
                if (isJson && buffer.Length > 0)
                {
                    try
                    {
                        var node = JsonNode.Parse(buffer);
                        ArabicToEnglish.TranslateTree(node);
                        var translated = node?.ToJsonString(
                            new System.Text.Json.JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                        var bytes = System.Text.Encoding.UTF8.GetBytes(translated ?? "null");
                        context.Response.ContentLength = bytes.Length;
                        await original.WriteAsync(bytes);
                        return;
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        buffer.Position = 0;
                    }
                }
                buffer.Position = 0;
                await buffer.CopyToAsync(original);
            }
            finally
            {
                context.Response.Body = original;
            }
        }
    }
}
