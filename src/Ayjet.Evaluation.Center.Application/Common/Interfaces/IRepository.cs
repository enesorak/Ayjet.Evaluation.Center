using System.Linq.Expressions;

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

// Bu genel arayüz, tüm varlıklar için temel CRUD işlemlerini tanımlar.
public interface IRepository<T> where T : class
{
    
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default); // <-- BU SATIRI EKLEYİN
    
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default); // <-- BU SATIRI EKLEYİN

    void Delete(T entity); // Delete için
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default); 

    void DeleteRange(IEnumerable<T> entities); // <-- YENİ METOT


    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    void Update(T entity); // <-- YENİ METOT
    
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);


    // Gelecekte eklenecek diğer metotlar:
    // Task<T> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    // Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    // void Update(T entity);
    // void Delete(T entity);
}