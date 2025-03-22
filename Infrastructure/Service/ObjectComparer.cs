using Application.IService;
using Domain.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class ObjectComparer : IObjectComparer
    {

        private readonly ILogger<ObjectComparer> _logger;

        public ObjectComparer(ILogger<ObjectComparer> logger)
        {
            _logger = logger;
        }

        public string CompareAndLogChanges<T>(T oldObject, T newObject)
        {

            if (oldObject == null || newObject == null)
            {
                _logger.LogWarning("Objects cannot be null.");
                return "Objects cannot be null.";
            }

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            StringBuilder log = new StringBuilder();

            foreach (var prop in properties)
            {
                if (!prop.CanRead) continue; // بررسی می‌کنیم که خاصیت قابل خواندن باشد

                object oldValue = prop.GetValue(oldObject);
                object newValue = prop.GetValue(newObject);
                string oldValueString = oldValue?.ToString()?.ToLower();
                string newValueString = newValue?.ToString()?.ToLower();

                if ((oldValueString == null && newValueString != null) || (oldValueString != null && !oldValueString.Equals(newValueString)))
                {
                    var trackChangesAttribute = prop.GetCustomAttribute<TrackChangesAttribute>();
                    string changeDescription = trackChangesAttribute?.Description ?? prop.Name;
                    string logEntry = $"{changeDescription} تغییر کرد از '{oldValue?.ToString() ?? "null"}' به '{newValue?.ToString() ?? "null"}'";
                    log.AppendLine(logEntry);
                    _logger.LogInformation(logEntry);
                }
            }

            string logResult = log.Length == 0 ? "چیزی تغییر نکرد." : log.ToString();

            if (log.Length == 0)
                _logger.LogInformation("چیزی تغییر نکرد.");

            return logResult;
        }
    }
}
