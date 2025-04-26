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

      

        public List<string> CompareByKey<T>(List<T> oldList, List<T> newList, string keyName, string prefix = "")
        {
            var changes = new List<string>();

            if (oldList == null || newList == null)
            {
                changes.Add($"{prefix} یکی از لیست‌ها نال است.");
                return changes;
            }

            var type = typeof(T);
            var keyProp = type.GetProperty(keyName, BindingFlags.Public | BindingFlags.Instance);
            if (keyProp == null)
            {
                throw new ArgumentException($"پراپرتی کلید '{keyName}' در نوع '{type.Name}' پیدا نشد.");
            }

            // تبدیل لیست‌ها به دیکشنری براساس کلید
            var oldDict = oldList
                .Where(x => keyProp.GetValue(x) != null)
                .ToDictionary(x => keyProp.GetValue(x)!.ToString()!, x => x);

            var newDict = newList
                .Where(x => keyProp.GetValue(x) != null)
                .ToDictionary(x => keyProp.GetValue(x)!.ToString()!, x => x);

            // بررسی آبجکت‌های موجود در old
            foreach (var oldItem in oldDict)
            {
                if (!newDict.ContainsKey(oldItem.Key))
                {
                    changes.Add($"{prefix} آیتم با کلید {oldItem.Key} حذف شده.");
                }
                else
                {
                    // اگر کلید مشترک باشد، بریم فیلدها را چک کنیم
                    changes.AddRange(CompareObjects(oldItem.Value, newDict[oldItem.Key], $"{prefix}({keyName}={oldItem.Key}) "));
                }
            }

            // بررسی آیتم‌های جدیدی که قبلاً وجود نداشتند
            foreach (var newItem in newDict)
            {
                if (!oldDict.ContainsKey(newItem.Key))
                {
                    changes.Add($"{prefix} آیتم جدید با کلید {newItem.Key} اضافه شده.");
                }
            }

            return changes;
        }

        public List<string> CompareObjects<T>(T oldObj, T newObj, string prefix = "")
        {
            var changes = new List<string>();

            if (oldObj == null || newObj == null)
            {
                changes.Add($"{prefix} یکی از آبجکت‌ها نال است.");
                return changes;
            }

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldObj);
                var newValue = prop.GetValue(newObj);

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    continue; // لیست‌ها را اینجا مدیریت نمی‌کنیم (جلوتر میتونیم سفارشی کنیم)
                }

                if ((oldValue == null && newValue != null) ||
                    (oldValue != null && newValue == null) ||
                    (oldValue != null && !oldValue.Equals(newValue)))
                {
                    changes.Add($"{prefix}{prop.Name} تغییر کرده: از '{oldValue ?? "null"}' به '{newValue ?? "null"}'");
                }
            }

            return changes;
        }
    }
}
