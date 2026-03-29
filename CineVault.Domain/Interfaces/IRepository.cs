using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineVault.Domain.Interfaces;
public interface IRepository<T> where T : class
{
    Task<T?> GetById(int id);
    Task<IEnumerable<T>> GetAll();
    Task Create(T entity);
    Task Update(T entity);
    Task Delete(int id);
}