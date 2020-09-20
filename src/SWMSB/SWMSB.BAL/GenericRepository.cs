
using SWMSB.COMMON;
using SWMSB.PROVIDERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SWMSB.BAL
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(string id);
        Task<T> AddOrUpdateAsync(T obj);
        Task<bool> DeleteAsync(string id);
        Task<DocumentResult<T>> GetDocumentsWithContinuationAsync(int pageSize,
              string continuationToken,
              Expression<Func<T, bool>> filterOptions,
              Expression<Func<T, string>> sortOptions);
    }

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public DocumentProvider<T> Dbprovider { get; }

        public GenericRepository(Config config)
        {
            Dbprovider = new DocumentProvider<T>(config.DocumentSecreteKeys);
        }

        public async Task<T> GetByIdAsync(string id)
        {
            return await Dbprovider.GetDocumentByIdAsync(id);
        }

        public async Task<T> AddOrUpdateAsync(T obj)
        {
            return await Dbprovider.AddOrUpdateAsync(obj);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await Dbprovider.RemoveAsync(id);
        }

        public async Task<DocumentResult<T>> GetDocumentsWithContinuationAsync(int pageSize, string continuationToken, Expression<Func<T, bool>> filterOptions, Expression<Func<T, string>> sortOptions)
        {
            return await Dbprovider.GetDocumentsWithContinuationAsync(pageSize, continuationToken, filterOptions, sortOptions);
        }


    }
}
