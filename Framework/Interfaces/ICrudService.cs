using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    public interface ICrudService<T, D> : IService where T : IDTO where D : IDAO<T>
    {
        D ServiceDAO { get; set; }

        Task<T> GetAsync(Guid id);

        Task<IEnumerable<T>> GetAllAsync(int pageIndex);

        Task<IStatus<int>> UpdateAsync(T record);

        Task<IStatus<int>> DeleteAsync(T record);

        Task<IStatus<int>> DeleteAsync(Guid recordId);

        Task<IStatus<int>> AddAsync(T record);

        Task<IStatus<int>> AddAllAsync(IList<T> records);

        Task<IStatus<int>> UpdateAllAsync(IList<T> records);

        Task<IStatus<int>> DeleteAllAsync(IList<T> records);

        Task<IStatus<int>> DeleteAllAsync(IList<Guid> recordIds);

        Task<IStatus<int>> ArchiveAllAsync(IList<T> records);

        Task<IStatus<int>> ArchiveAsync(T record);
    }
}
