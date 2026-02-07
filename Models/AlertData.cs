using AccountingApp.Enums;

namespace AccountingApp.Models
{
    public class AlertData
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public AlertType Type { get; set; } = AlertType.Info;
        public bool ShowConfirmButton { get; set; } = true;
        public string ConfirmButtonText { get; set; } = "OK";
        public bool ShowCancelButton { get; set; } = false;
        public string CancelButtonText { get; set; } = "Cancel";
        public int? Timer { get; set; } // in milliseconds, null = no timer
        public bool AllowOutsideClick { get; set; } = true;
        public bool AllowEscapeKey { get; set; } = true;

        // Property untuk serialisasi JSON
        public string Icon => Type.ToString().ToLower();
    }
}
