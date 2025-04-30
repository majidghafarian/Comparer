using Application.IService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class ObjectComparer
    {



        private string GetDisplayName(PropertyInfo prop)
        {
            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name ?? prop.Name;
        }

        private string GetEnumDisplayName(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name ?? value.ToString();
        }

        private PropertyInfo GetKeyProperty(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        }

        public List<string> CompareObjects<T>(T oldObj, T newObj)
        {
            var changes = new List<string>();

            if (oldObj == null || newObj == null)
            {
                changes.Add("یکی از آبجکت‌ها نال است.");
                return changes;
            }

            var type = typeof(T);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                var displayName = GetDisplayName(prop);
                var oldValue = prop.GetValue(oldObj);
                var newValue = prop.GetValue(newObj);

                if (oldValue == null && newValue == null)
                    continue;

                if (prop.PropertyType.IsEnum)
                {
                    if (prop.PropertyType.GetCustomAttribute<FlagsAttribute>() != null)
                    {
                        changes.AddRange(CompareFlagsManually((Enum)oldValue, (Enum)newValue));
                    }
                    else if (!Equals(oldValue, newValue))
                    {
                        string oldText = GetEnumDisplayName((Enum)oldValue);
                        string newText = GetEnumDisplayName((Enum)newValue);
                        changes.Add($"{displayName} تغییر کرده: از '{oldText}' به '{newText}'");
                    }
                    continue;
                }

                if (prop.PropertyType == typeof(bool))
                {
                    var oldBool = (bool?)oldValue;
                    var newBool = (bool?)newValue;

                    if (oldBool != newBool)
                    {
                        string oldStatus = oldBool.HasValue && oldBool.Value ? "کاربر فعال" : "کاربر غیرفعال";
                        string newStatus = newBool.HasValue && newBool.Value ? "کاربر فعال" : "کاربر غیرفعال";
                        changes.Add($"{displayName} تغییر کرده: از '{oldStatus}' به '{newStatus}'");
                    }
                    continue;
                }

                if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    var oldList = (oldValue as IEnumerable)?.Cast<object>().ToList() ?? new();
                    var newList = (newValue as IEnumerable)?.Cast<object>().ToList() ?? new();

                    var itemType = prop.PropertyType.IsGenericType ? prop.PropertyType.GetGenericArguments().FirstOrDefault() : null;
                    var keyProp = itemType != null ? GetKeyProperty(itemType) : null;

                    if (keyProp == null)
                    {
                        changes.Add($"🔴 کلید برای {displayName} یافت نشد (نوع: {itemType?.Name})");
                        continue;
                    }

                    var oldDict = oldList.ToDictionary(x => keyProp.GetValue(x)?.ToString());
                    var newDict = newList.ToDictionary(x => keyProp.GetValue(x)?.ToString());

                    foreach (var key in oldDict.Keys)
                    {
                        if (newDict.TryGetValue(key, out var newItem))
                        {
                            var nestedChanges = CompareObjects(oldDict[key], newItem);
                            changes.AddRange(nestedChanges);
                        }
                        else
                        {
                            changes.Add($"{displayName} آیتمی با کلید '{key}' در لیست جدید وجود ندارد.");
                        }
                    }

                    foreach (var key in newDict.Keys.Except(oldDict.Keys))
                    {
                        changes.Add($"{displayName} آیتم جدیدی با کلید '{key}' اضافه شده.");
                    }
                    continue;
                }

                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    var nestedChanges = CompareObjects(oldValue, newValue);
                    changes.AddRange(nestedChanges);
                    continue;
                }

                if ((oldValue == null && newValue != null) || (oldValue != null && !oldValue.Equals(newValue)))
                {
                    changes.Add($"{displayName} تغییر کرده: از '{oldValue ?? "null"}' به '{newValue ?? "null"}'");
                }
            }

            return changes;
        }

        private List<string> CompareFlagsManually(Enum oldValue, Enum newValue)
        {
            var changes = new List<string>();

            var oldVal = Convert.ToInt32(oldValue);
            var newVal = Convert.ToInt32(newValue);

            var oldFlags = Enum.GetValues(oldValue.GetType()).Cast<Enum>()
                .Where(f => (Convert.ToInt32(f) & oldVal) == Convert.ToInt32(f) && Convert.ToInt32(f) != 0);

            var newFlags = Enum.GetValues(newValue.GetType()).Cast<Enum>()
                .Where(f => (Convert.ToInt32(f) & newVal) == Convert.ToInt32(f) && Convert.ToInt32(f) != 0);

            var removed = oldFlags.Except(newFlags).ToList();
            var added = newFlags.Except(oldFlags).ToList();

            if (removed.Count == 1 && added.Count == 1)
            {
                changes.Add($"{removed[0]} به {added[0]} تغییر کرده.");
            }
            else
            {
                foreach (var r in removed)
                {
                    changes.Add($"{r} حذف شده.");
                }
                foreach (var a in added)
                {
                    changes.Add($"{a} اضافه شده.");
                }
            }

            return changes;
        }
    }
}

