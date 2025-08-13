
using Domain;
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
        private static string GetKey(PropertyInfo prop)
        {
            var attribute = Attribute.GetCustomAttribute(prop, typeof(KeyAttribute))
               as KeyAttribute;

            return attribute?.ToString();
        }
        private static string GetValueOrdinal(PropertyInfo prop)
        {
            var attribute = Attribute.GetCustomAttribute(prop, typeof(GetValueordinal))
             as GetValueordinal;

            if (attribute != null)
            {
                return attribute.Value;
            }

            else
                return string.Empty;

        }
        private static string GetOrdinal(object value)
        {
            var type = value.GetType();
            var Ordinal = new List<string>();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {

                string ordinal = GetValueOrdinal(prop);
                if (!string.IsNullOrEmpty(ordinal))
                {

                    var res = prop.GetValue(value);
                    if (res != null)
                    {
                        return ordinal + " " + res.ToString();
                    }
                }
            }
            return string.Empty;
        }
        private static string GetDisplayName(object prop)
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
            string ordinalAttribiute = GetOrdinal(oldObj);
            var displayNameObject = GetDisplayName((oldObj));
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
                    if (!string.IsNullOrEmpty(ordinalAttribiute))
                    {
                        if (displayNameObject != null)
                        {
                            changes.Add($" در {displayNameObject}:" + $"{ordinalAttribiute}" + " " + $"مقدار تغییر کرده: از '{oldText}' به '{newText}'");
                        }
                        else
                        {
                            changes.Add($"در {ordinalAttribiute}" + $"مقدار تغییر کرده: از '{oldText}' به '{newText}'");
                        }
                    }
                    else
                    {

                        if (displayNameObject != null)
                        {
                            changes.Add($" در :  {displayNameObject}" + " " + $"مقدار تغییر کرده: از '{oldText}' به '{newText}'");
                        }
                        else
                        {
                            changes.Add($"مقدار تغییر کرده: از '{oldText}' به '{newText}'");
                        }
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
                        if (!string.IsNullOrEmpty(ordinalAttribiute))
                        {
                            if (displayNameObject != null)
                            {
                                changes.Add($"در  {displayNameObject}" + " " + $"{ordinalAttribiute} " + $"{displayName} تغییر کرده: از '{oldValue ?? " null "}' به '{newValue ?? " null "}'");
                            }
                            else
                            {
                                changes.Add($"در {ordinalAttribiute} :  {displayName} تغییر کرده: از '{oldValue ?? " null "}' به '{newValue ?? " null "}'");
                            }
                        }
                        else
                        {

                            if (displayNameObject != null)
                            {
                                changes.Add($"در {displayNameObject}" + " " + $"{displayName} تغییر کرده: از '{oldValue ?? " null "}' به '{newValue ?? " null "}'");
                            }
                            else
                            {
                                changes.Add($"{displayName} تغییر کرده: از '{oldValue ?? " null "}' به '{newValue ?? " null "}'");
                            }
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
                        if (!string.IsNullOrEmpty(ordinalAttribiute))
                        {
                            if (displayNameObject != null)
                            {
                                changes.Add($"در  {displayNameObject}  "+" "+$"{ordinalAttribiute}" + " " + $"{displayName} تغییر کرده: از '{oldStatus}' به '{newStatus}'");
                            }
                            else
                            {
                                changes.Add($"{ordinalAttribiute} تغییر کرده: از '{oldStatus}' به '{newStatus}'");
                            }
                        }
                        else
                        {

                            if (displayNameObject != null)
                            {
                                changes.Add($"در {displayNameObject}" + " " + $"{displayName} تغییر کرده: از '{oldStatus}' به '{newStatus}'");
                            }
                            else
                            {
                                changes.Add($"{displayName} تغییر کرده: از '{oldStatus}' به '{newStatus}'");
                            }
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
                    changes.AddRange(nestedChanges.Split(Environment.NewLine.ToCharArray())); // ✅
                    continue;
                }

                if ((oldValue == null && newValue != null) || (oldValue != null && newValue == null) || (oldValue != null && !oldValue.Equals(newValue)))
                {
                    if (!string.IsNullOrEmpty(ordinalAttribiute))
                    {
                        if (displayNameObject != null)
                        {
                            changes.Add($"در  {displayNameObject}" + $"{ordinalAttribiute}" + " " + $"{displayName} تغییر کرده: از '{oldValue ?? " null "}' به '{newValue ?? " null "}'");
                        }
                        else
                        {
                            changes.Add($"در {ordinalAttribiute} :  {displayName} تغییر کرده: از '{oldValue ?? " null "}' به '{newValue ?? " null "}'");
                        }
                    }
                    else
                    {

                        if (displayNameObject != null)
                        {
                            changes.Add($"در {displayNameObject}" + " " + $"{displayName} تغییر کرده: از '{oldValue ?? " null "}' به '{newValue ?? " null "}'");
                        }
                        else
                        {
                            changes.Add($"{displayName} تغییر کرده: از '{oldValue ?? " null "}' به '{newValue ?? " null "}'");
                        }
                    }
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

