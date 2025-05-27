using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ManagementPortal.Shared.Dtos.ValidationAttributes
{
    /// <summary>
    /// Metodo para poder validar que al menos uno de los campos seleccionados no sea null o vacio.
    /// Para más información veasé https://stackoverflow.com/a/26424791
    ///
    ///
    /// Utilización:
    /// [AtLeastOneProperty ( FildName1, FildName2 , ..., ErrorMessage="You must supply at least one value")]
    ///  public class myClass (){}.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AtLeastOnePropertyAttribute : ValidationAttribute
    {
        /// <summary>
        /// Lista de los campos que se deben validar.
        /// </summary>
        private readonly string[] _propertyList;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtLeastOnePropertyAttribute"/> class with the specified property list.
        /// </summary>
        /// <param name="propertyList">Nombre de los campos que se deben verificar.</param>
        public AtLeastOnePropertyAttribute(params string[] propertyList)
        {
            _propertyList = propertyList;
        }

        /// <summary>
        /// Se sobreescribe el porque si no no funcionaría la Propiedad. Veasé http://stackoverflow.com/a/1365669.
        /// </summary>
        public override object TypeId
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Método que se encarga de verificar si al menos uno de los campos seleccionados no es null o vacio. Se recorre la lista de campos y se obtiene el campo de la clase que se debe validar y se verifica la condición.
        /// </summary>
        /// <param name="value"> El objecto en este caso es la Clase y se van a recorrer las propiedades dentro de la lista de _propertyList.</param>
        /// <returns>Se retorna el estado de la validación.</returns>
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return false;
            }

            foreach (string propertyName in _propertyList)
            {
                PropertyInfo? propertyInfo = value.GetType().GetProperty(propertyName);

                if (propertyInfo != null && propertyInfo.GetValue(value, null) != null)
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        string? test = propertyInfo.GetValue(value, null) as string;
                        bool evaluation = string.IsNullOrEmpty(test);
                        return !evaluation;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}