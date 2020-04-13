using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum OrderDirection
{
    Ascending,
    Descending
}

public enum FieldNamingType
{
    ///// <summary>
    ///// Use Name of PropertyInfo as is
    ///// </summary>
    //Exact,
    /// <summary>
    /// Use Name of PropertyInfo in lower case
    /// </summary>
    LowerCase,
    /// <summary>
    /// Use Name of PropertyInfo in lower case, but capital letters are replaced with an underscore, except for the first character if it's a capital letter
    /// </summary>
    LowerSnakeCamelCase,
    /// <summary>
    /// Use Name of PropertyInfo in lower case with type prefix
    /// </summary>
    TypeLowerCase
}