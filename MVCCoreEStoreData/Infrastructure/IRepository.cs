using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCCoreEStoreData.Infrastructure
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        void Create(T model);
        void Edit(T model);
        void Remove(int id);

    }

    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext context;

        public Repository(AppDbContext context)
        {
            this.context = context;
        }

        public void Create(T model)
        {
            context.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            context.SaveChanges();
        }

        public void Edit(T model)
        {
            context.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
        }

        public IEnumerable<T> GetAll()
        {
            return context.Set<T>().ToList();
        }

        public void Remove(int id)
        {
            var model = context.Set<T>().Find(id);
            context.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            context.SaveChanges();
        }


    }
}
