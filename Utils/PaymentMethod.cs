using Microsoft.AspNetCore.Mvc.Rendering;

namespace OpenExpenseApp.Utils
{
    public static class PaymentMethod
    {
        public static List<SelectListItem> GetPaymentMethodOptions(string? selectedValue = null)
        {
            return
            [
                new SelectListItem
                {
                    Text = "Cash",
                    Value = "Cash",
                    Selected = selectedValue == "Cash",
                },
                new SelectListItem
                {
                    Text = "Card",
                    Value = "Card",
                    Selected = selectedValue == "Card",
                },
                new SelectListItem
                {
                    Text = "Bank",
                    Value = "Bank",
                    Selected = selectedValue == "Bank",
                },
                new SelectListItem
                {
                    Text = "E-Wallet",
                    Value = "E-Wallet",
                    Selected = selectedValue == "E-Wallet",
                },
                new SelectListItem
                {
                    Text = "Other",
                    Value = "Other",
                    Selected = selectedValue == "Other",
                },
            ];
        }

        public static List<(string Value, string Text)> GetPaymentMethodList()
        {
            return
            [
                ("Cash", "Cash"),
                ("Card", "Card"),
                ("Bank", "Bank"),
                ("E-Wallet", "E-Wallet"),
                ("Other", "Other"),
            ];
        }
    }
}
