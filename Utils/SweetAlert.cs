using System.Text.Json;
using AccountingApp.Enums;
using AccountingApp.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AccountingApp.Helpers
{
    public static class SweetAlertHelper
    {
        // Constructor-like method for quick alert setup
        public static void SetAlert(
            ITempDataDictionary tempData,
            string title,
            AlertType type,
            string message
        )
        {
            var alertData = new AlertData
            {
                Title = title ?? "Info",
                Type = type,
                Message = message ?? "",
            };
            SetAlert(tempData, alertData);
        }

        // Overload for backward compatibility with old string-based icons
        public static void SetAlert(
            ITempDataDictionary tempData,
            string title,
            string icon,
            string message
        )
        {
            var alertType = icon?.ToLower() switch
            {
                "success" => AlertType.Success,
                "error" => AlertType.Error,
                "warning" => AlertType.Warning,
                "info" => AlertType.Info,
                "question" => AlertType.Question,
                _ => AlertType.Info,
            };

            SetAlert(tempData, title, alertType, message);
        }

        // Main method with full AlertData object
        public static void SetAlert(ITempDataDictionary tempData, AlertData alertData)
        {
            // https://sweetalert2.github.io/
            var json = JsonSerializer.Serialize(
                alertData,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );
            tempData["AlertData"] = json;
            tempData["ShowAlert"] = true;
        }

        // Helper methods for common scenarios
        public static void Success(ITempDataDictionary tempData, string title, string message)
        {
            SetAlert(tempData, title, AlertType.Success, message);
        }

        public static void Error(ITempDataDictionary tempData, string title, string message)
        {
            SetAlert(tempData, title, AlertType.Error, message);
        }

        public static void Warning(ITempDataDictionary tempData, string title, string message)
        {
            SetAlert(tempData, title, AlertType.Warning, message);
        }

        public static void Info(ITempDataDictionary tempData, string title, string message)
        {
            SetAlert(tempData, title, AlertType.Info, message);
        }

        public static void Question(ITempDataDictionary tempData, string title, string message)
        {
            SetAlert(tempData, title, AlertType.Question, message);
        }
    }
}
