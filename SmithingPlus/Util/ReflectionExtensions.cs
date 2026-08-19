using System;
using System.Reflection;

namespace SmithingPlus.Util;

#nullable enable

public static class ReflectionExtensions
{
    public static T GetField<T>(this object obj, string fieldName)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        FieldInfo? fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (fieldInfo == null)
        {
            throw new MissingFieldException(obj.GetType().FullName, fieldName);
        }

        object? fieldValue = fieldInfo.GetValue(obj);
        if (fieldValue is T typedValue)
        {
            return typedValue;
        }

        throw new InvalidCastException("The reflected field is null or has an unexpected type.");
    }

    public static bool TryGetField<T>(this object obj, string fieldName, out T? fieldValue)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        FieldInfo? fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (fieldInfo != null && fieldInfo.GetValue(obj) is T typedValue)
        {
            fieldValue = typedValue;
            return true;
        }

        fieldValue = default;
        return false;
    }
    
    public static void SetField<T>(this object obj, string fieldName, T newValue)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        FieldInfo? fi = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (fi == null) throw new MissingFieldException(obj.GetType().FullName, fieldName);

        fi.SetValue(obj, newValue);
    }

    public static T GetInternalField<T>(this object obj, string fieldName)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        FieldInfo? fi = obj.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (fi == null) throw new MissingFieldException(obj.GetType().FullName, fieldName);

        object? fieldValue = fi.GetValue(obj);
        if (fieldValue is T typedValue)
        {
            return typedValue;
        }

        throw new InvalidCastException("The reflected field is null or has an unexpected type.");
    }

    public static void SetInternalField<T>(this object obj, string fieldName, T newValue)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        FieldInfo? fi = obj.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (fi == null) throw new MissingFieldException(obj.GetType().FullName, fieldName);

        fi.SetValue(obj, newValue);
    }
}
