using System.Reflection;

namespace test
{
    /// <summary>
    /// Получение значения привтного поля с помощью рефлексии
    /// </summary>
    public static class ObjectAssert
    {
        public static T? GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found");

            return (T)field.GetValue(obj);
        }

        public static T? GetPrivateProperty<T>(object obj, string propertyName)
        {
            var property = obj.GetType()
                .GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (property == null)
                throw new ArgumentException($"Property '{propertyName}' not found");

            return (T)property.GetValue(obj);
        }
    }
}
