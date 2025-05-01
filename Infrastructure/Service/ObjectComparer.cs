
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public static  class ObjectComparer
    {



        private static string GetDisplayName(PropertyInfo prop)
        {
            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name;
        }


        private static string GetEnumDisplayName(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.Name ?? value.ToString();
        }

        private static PropertyInfo GetKeyProperty(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        }




        public static string CompareByKey(object oldInput, object newInput)
        {
            var changes = new List<string>();

            // === اگر لیست بودن ===
            if (oldInput is IEnumerable oldList && newInput is IEnumerable newList)
            {
                var itemType = oldList.Cast<object>().FirstOrDefault()?.GetType();
                if (itemType == null)
                {
                    changes.Add("❌ لیست قدیمی خالی است یا نوع آیتم مشخص نیست.");
                    return string.Join(Environment.NewLine,changes);
                }

                var keyProp = GetKeyProperty(itemType);
                if (keyProp == null)
                {
                    changes.Add($"❌ کلید [Key] برای کلاس {itemType.Name} یافت نشد.");
                    return string.Join(Environment.NewLine,changes);
                }

                var oldDict = oldList.Cast<object>()
                                     .Where(x => keyProp.GetValue(x) != null)
                                     .ToDictionary(x => keyProp.GetValue(x)!.ToString());

                var newDict = newList.Cast<object>()
                                     .Where(x => keyProp.GetValue(x) != null)
                                     .ToDictionary(x => keyProp.GetValue(x)!.ToString());

                foreach (var addedKey in oldDict.Keys)
                {
                    if (newDict.TryGetValue(addedKey, out var newItem))
                    {

                        var nestedChanges = CompareObjects(oldDict[addedKey], newItem);
                        changes.AddRange(nestedChanges.Split(Environment.NewLine));
                    }
                    else
                    {
                        changes.Add($"❌ آیتم با کلید '{addedKey}' در لیست جدید وجود ندارد.");
                    }
                }

                foreach (var addedKey in newDict.Keys.Except(oldDict.Keys))
                {
                    changes.Add($"➕ آیتم جدیدی با کلید '{addedKey}' اضافه شده.");
                }

                return string.Join(Environment.NewLine, changes);
            }

            // === اگر کلاس تکی بودن ===
            if (oldInput == null || newInput == null)
            {
                changes.Add("❌ یکی از آبجکت‌ها نال است.");
                return string.Join(Environment.NewLine, changes);
            }

            var type = oldInput.GetType();
            if (type != newInput.GetType())
            {
                changes.Add("❌ نوع دو آبجکت یکسان نیست.");
                return string.Join(Environment.NewLine, changes);
            }

            var key = GetKeyProperty(type);
            if (key == null)
            {
                changes.Add($"❌ کلید [Key] در کلاس {type.Name} یافت نشد.");
                return string.Join(Environment.NewLine, changes);
            }

            var oldKey = key.GetValue(oldInput)?.ToString();
            var newKey = key.GetValue(newInput)?.ToString();

            if (oldKey != newKey)
            {
                changes.Add($"❌ مقایسه انجام نشد چون کلیدها متفاوت هستند (قدیم: {oldKey}، جدید: {newKey})");
                return string.Join(Environment.NewLine, changes);
            }

            var finalChanges = CompareObjects(oldInput, newInput);
            changes.AddRange(finalChanges.Split(Environment.NewLine));
            return string.Join(Environment.NewLine, changes);
        }


        public static string CompareObjects(object oldObj, object newObj)

        {
            var changes = new List<string>();

            if (oldObj == null || newObj == null)
            {
                changes.Add("یکی از آبجکت‌ها نال است.");
                return string.Join(Environment.NewLine, changes);
            }

            var type = oldObj?.GetType() ?? newObj?.GetType();
            // ✅ اگر مقدار enum هست، مستقیماً مقایسه کن
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
                    changes.Add($"مقدار تغییر کرده: از '{oldText}' به '{newText}'");
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

                        changes.Add($"{fieldName} تغییر کرده: از '{oldText}' به '{newText}'");
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
                            changes.AddRange(nestedChanges.Split(Environment.NewLine)); // ✅ درستی

                       
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
                    changes.AddRange(nestedChanges.Split(Environment.NewLine)); // ✅

                    continue;
                }

                if ((oldValue == null && newValue != null) || (oldValue != null && newValue == null) || (oldValue != null && !oldValue.Equals(newValue)))
                {
                    Console.WriteLine($"🔍 مقایسه: {prop.Name}, old: '{oldValue}', new: '{newValue}'");
                    changes.Add($"{displayName} تغییر کرده: از '{oldValue ?? "null"}' به '{newValue ?? "null"}'");
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

