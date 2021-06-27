using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    /// <summary>
    /// A marker for singleton dependency injection
    /// </summary>
    public interface IDAO
    {

    }

    /// <summary>
    /// A marker fot DAOs
    /// </summary>
    /// <typeparam name="T">DTO type being managed by DAO</typeparam>
    public interface IDAO<T> : IDAO where T : IDTO
    {
        IDataContext DataContext { get; set; }

        int PageSize { get; set; }

        Task<T> GetAsync(Guid id);

        Task<IEnumerable<T>> GetAllAsync();
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
