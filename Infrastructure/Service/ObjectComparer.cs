using Application.IService;
using Domain.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class ObjectComparer : IObjectComparer
    {

        private readonly ILogger<ObjectComparer> _logger;
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new();

        public ObjectComparer(ILogger<ObjectComparer> logger)
        {
            _logger = logger;
        }



        public List<string> CompareByKey(IEnumerable<object> oldList, IEnumerable<object> newList, string keyName, string prefix = "")
        {
            var changes = new List<string>();

            var oldDict = oldList
                .Where(x => GetKeyValue(x, keyName) != null)
                .ToDictionary(x => GetKeyValue(x, keyName)!.ToString()!, x => x);

            var newDict = newList
                .Where(x => GetKeyValue(x, keyName) != null)
                .ToDictionary(x => GetKeyValue(x, keyName)!.ToString()!, x => x);

            foreach (var oldItem in oldDict)
            {
                if (!newDict.ContainsKey(oldItem.Key))
                {
                    changes.Add($"{prefix} آیتم با کلید {oldItem.Key} حذف شده.");
                }
                else
                {
                    changes.AddRange(CompareObjects(oldItem.Value, newDict[oldItem.Key], $"{prefix}({keyName}={oldItem.Key}) ", keyName));
                }
            }

            foreach (var newItem in newDict)
            {
                if (!oldDict.ContainsKey(newItem.Key))
                {
                    changes.Add($"{prefix} آیتم جدید با کلید {newItem.Key} اضافه شده.");
                }
            }

            return changes;
        }

      
        private object GetKeyValue(object obj, string keyName)
        {
            if (obj == null || string.IsNullOrEmpty(keyName))
                return null;

            var type = obj.GetType();

            // چک کردن کش: آیا قبلاً پراپرتی های این تایپ ذخیره شده؟
            if (!_propertyCache.TryGetValue(type, out var properties))
            {
                // اگر نبوده، پراپرتی ها را بخوان و بریز داخل کش
                properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

                _propertyCache[type] = properties;
            }

            // حالا سعی کن پراپرتی مورد نظر رو از کش پیدا کنی
            if (properties.TryGetValue(keyName, out var propertyInfo))
            {
                return propertyInfo.GetValue(obj);
            }

            // اگر پراپرتی نبود (مثلا keyName اشتباه فرستاده شده بود)
            return null;
        }
        private string GetDisplayName(PropertyInfo prop)
        {
            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name ?? prop.Name;
        }


        public List<string> CompareObjects<T>(T oldObj, T newObj, string prefix = "", string keyName = "Id")
        {
            var changes = new List<string>();

            if (oldObj == null || newObj == null)
            {
                changes.Add($"{prefix} یکی از آبجکت‌ها نال است.");
                return changes;
            }

            var type = oldObj.GetType(); // 👈 اینجا باید نوع واقعی آبجکت رو بگیری
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldObj);
                var newValue = prop.GetValue(newObj);

                if (oldValue == null && newValue == null)
                    continue;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    var oldList = (oldValue as System.Collections.IEnumerable)?.Cast<object>().ToList() ?? new List<object>();
                    var newList = (newValue as System.Collections.IEnumerable)?.Cast<object>().ToList() ?? new List<object>();

                    if (oldList.Any() || newList.Any())
                    {
                        changes.AddRange(CompareByKey(oldList, newList, keyName, $"{prefix}{prop.Name}->"));
                    }
                    continue;
                }

                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    changes.AddRange(CompareObjects(oldValue, newValue, $"{prefix}{prop.Name}->", keyName));
                    continue;
                }

                if ((oldValue == null && newValue != null) ||
                    (oldValue != null && newValue == null) ||
                    (oldValue != null && !oldValue.Equals(newValue)))
                {
                    var displayName = GetDisplayName(prop);

                    changes.Add($"{prefix}{displayName} تغییر کرده: از '{oldValue ?? "null"}' به '{newValue ?? "null"}'");

                }
            }

            return changes;
        }


    }
}
