using Framework.Attributes;
using System;

namespace Framework.Interfaces
{

    /// <summary>
    /// A marker for any DTO
    /// </summary>
    public interface IDTO
    {
        [PrimaryKey]
        Guid Id { get; set; }
    }
}
