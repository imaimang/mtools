using System.ComponentModel;
using System.Reflection;

namespace MTools.Enumlib
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }
    }

    public class EnumValue
    {
        public string Label { get; set; }

        public object Value { get; set; }
    }


    /// <summary>
    /// Enum数据操作类
    /// </summary>
    public class EnumHelper
    {

        public static List<EnumValue> ConvertEnumValues<T>()
        {
            List<EnumValue> enumValues = new List<EnumValue>();
            var array = Enum.GetValues(typeof(T));
            foreach (var type in array)
            {
                EnumValue enumValue = new EnumValue();
                enumValue.Value = (T)type;

                System.Reflection.FieldInfo fieldInfo = type.GetType().GetField(type.ToString());
                if (fieldInfo != null)
                {
                    object[] attribArray = fieldInfo.GetCustomAttributes(false);
                    if (attribArray.Length == 0)
                        enumValue.Label = type.ToString();
                    else
                        enumValue.Label = (attribArray[0] as DescriptionAttribute).Description;
                }
                enumValues.Add(enumValue);
            }
            return enumValues;
        }

        /// <summary>
        /// 获取枚举值上的Description特性的说明
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="obj">枚举值</param>
        /// <returns>特性的说明</returns>
        public static string GetEnumDescription<T>(T obj)
        {
            var type = obj.GetType();
            FieldInfo field = type.GetField(Enum.GetName(type, obj));
            DescriptionAttribute descAttr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (descAttr == null)
            {
                return string.Empty;
            }

            return descAttr.Description;
        }
        public static T GetEnumAttribute<T>(Enum source) where T : Attribute
        {
            Type type = source.GetType();
            var sourceName = Enum.GetName(type, source);
            FieldInfo field = type.GetField(sourceName);
            object[] attributes = field.GetCustomAttributes(typeof(T), false);
            foreach (var o in attributes)
            {
                if (o is T)
                    return (T)o;
            }
            return null;
        }

        public static string GetDescription(Enum source)
        {
            if (source == null) return "";
            var str = GetEnumAttribute<DescriptionAttribute>(source);
            if (str == null)
                return null;
            return str.Description;
        }

        public static List<string> GetDescriptions<TEnum>() where TEnum : struct
        {
            return EnumHelper.GetEnumList<TEnum>().Select(t => EnumHelper.GetDescription(t as Enum)).ToList();
        }

        public static List<TEnum> GetEnumList<TEnum>() where TEnum : struct
        {
            List<TEnum> list = new List<TEnum>();
            list.AddRange(typeof(TEnum).GetEnumValues() as TEnum[]);
            return list;
        }
    }
}