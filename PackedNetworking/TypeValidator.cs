using System;
using System.Text;

namespace PackedNetworking
{
    public class TypeValidator
    {
        private readonly Type[] _validTypes;

        public TypeValidator(params Type[] validTypes)
        {
            _validTypes = validTypes ?? new Type[0];
        }

        public bool IsValid(Type type, bool allowSubclass = true)
        {
            for (var i = 0; i < _validTypes.Length; i++)
                if (type == _validTypes[i] || type.IsSubclassOf(_validTypes[i]) && allowSubclass)
                    return true;

            return false;
        }

        public string GetValidTypesAsString()
        {
            var builder = new StringBuilder();

            for (var i = 0; i < _validTypes.Length; i++) 
                builder.Append($"'{_validTypes[i].Name}', ");

            builder.Remove(builder.Length - 2, 2);
            
            return builder.ToString();
        }
    }
}