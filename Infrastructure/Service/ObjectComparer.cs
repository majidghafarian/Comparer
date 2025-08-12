
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public static class ObjectComparer
    {
        private static string GetDisplayName(PropertyInfo prop)
        {

            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name;
        }
        private static string GetDisplayNameForObject(object prop)
        {
            var displayAttr = prop.GetType().GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name;
        }



        private static string GetEnumDisplayName(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name ?? value.ToString();
        }




        public static string CompareObjects(object oldObj, object newObj)

        {
            var changes = new List<string>();

            if (oldObj == null || newObj == null)
            {
                changes.Add("یکی از آبجکت‌ها نال است.");
                return string.Join(Environment.NewLine, changes);
            }

            var displayNameObject = GetDisplayNameForObject((oldObj));


            var type = oldObj.GetType();
            if (type.IsEnum)
            {
                if (type.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    changes.AddRange(CompareFlagsManually((Enum)oldObj, (Enum)newObj));
                }
                else if (!Equals(oldObj, newObj))
                {
                    string oldText = GetEnumDisplayName((Enum)oldObj);
                    string newText = GetEnumDisplayName((Enum)newObj);
                    if (displayNameObject != null)
                    {
                        changes.Add($"در {displayNameObject}" + " " + $"مقدار تغییر کرده: از '{oldText}' به '{newText}'");
                    }
                    else
                    {
                        changes.Add($"مقدار تغییر کرده: از '{oldText}' به '{newText}'");
                    }
                }
                return string.Join(Environment.NewLine, changes);
            }

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                var displayName = GetDisplayName(prop);
                if (displayName == null && !prop.PropertyType.IsEnum)
                    continue;
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

                        // اگر Display برای خود پراپرتی نبود، از نام enum استفاده کن
                        string fieldName = displayName ?? prop.Name;
                        if (displayNameObject != null)
                        {
                            changes.Add($"در {displayNameObject}" + " " + $"{fieldName} تغییر کرده: از '{oldText}' به '{newText}'");
                        }
                        else
                        {
                            changes.Add($"{fieldName} تغییر کرده: از '{oldText}' به '{newText}'");
                        }
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
                        if (displayNameObject != null)
                        {
                            changes.Add($"در {displayNameObject}" + " " + $"{displayName} تغییر کرده: از '{oldStatus}' به '{newStatus}'");
                        }
                        else
                        {
                            changes.Add($"{displayName} تغییر کرده: از '{oldStatus}' به '{newStatus}'");
                        }
                    }
                    continue;
                }

                if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    var oldList = (oldValue as IEnumerable)?.Cast<object>().ToList();
                    var newList = (newValue as IEnumerable)?.Cast<object>().ToList();
                    for (int i = 0; i < oldList.Count; i++)
                    {
                        if (i < oldList.Count)
                        {
                            var CompareList = CompareObjects(oldList[i], newList[i]);
                            changes.Add(CompareList);
                        }
                    }
                    continue;
                }

                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    var nestedChanges = CompareObjects(oldValue, newValue);
                    changes.AddRange(nestedChanges.Split(Environment.NewLine)); // ✅
                    continue;
                }

                if ((oldValue == null && newValue != null) || (oldValue != null && newValue == null) || (oldValue != null && !oldValue.Equals(newValue)))
                {
                    Console.WriteLine($"🔍 مقایسه: {prop.Name}, old: '{oldValue}', new: '{newValue}'");
                    if (displayNameObject != null)
                    {
                        changes.Add($"در {displayNameObject}" + " " + $"{displayName} تغییر کرده: از '{oldValue ?? "null"}' به '{newValue ?? "null"}'");
                    }
                    else
                    {
                        changes.Add($"{displayName} تغییر کرده: از '{oldValue ?? "null"}' به '{newValue ?? "null"}'");
                    }
                }
                else
                {
                    Console.WriteLine($"✅ بدون تغییر: {prop.Name}, مقدار: '{oldValue}'");
                }
            }
            return string.Join(Environment.NewLine, changes);
        }

        private static List<string> CompareFlagsManually(Enum oldValue, Enum newValue)
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

