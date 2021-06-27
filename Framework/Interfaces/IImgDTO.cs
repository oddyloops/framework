using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    /// <summary>
    /// A marker for DTOs with images
    /// </summary>
    public interface IImgDTO : IDTO
    {
        int MaxImageCount { get; set; }

        bool HasIcon { get; set; }
    }
}
