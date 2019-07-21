using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepoPractice
{
    public class Course
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public Author Author { get; set; }
    }

    public class Author
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }
        public List<Course> Courses { get; set; }
    }

    public class UniversityContext : DbContext
    {
        public UniversityContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Author> Authors { get; set; }
    }
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Get(object Id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        void Add(TEntity entity);
        void Addrenge(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext Context;

        public Repository(DbContext context)
        {
            Context = context;
        }

        public TEntity Get(object Id)
        {
            return Context.Set<TEntity>().Find(Id);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return Context.Set<TEntity>();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where(predicate);
            //context.course.Find(c=>c.Name=="Math")
        }

        //Func<int, bool> equalsFive = x => x == 5;
        //bool result = equalsFive(4);
        Func<TEntity, bool> predicate = T => T.ToString() == "a";

        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public void Addrenge(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().AddRange(entities);
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
        }
    }
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public interface ICoursesRepository : IRepository<Course>
    {
        IEnumerable<Course> GetTopSellingCourses(int count);
        IEnumerable<Course> GetAuthors(int pageIndex, int pageSize);
    }

    public class CourseRepository : Repository<Course>, ICoursesRepository
    {
        private readonly UniversityContext _context;

        public CourseRepository(UniversityContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<Course> GetTopSellingCourses(int count)
        {
            return _context.Courses.OrderByDescending(c => c.Price).Take(count);
        }

        public IEnumerable<Course> GetAuthors(int pageIndex, int pageSize)
        {
            return _context.Courses
                .Include(c => c.Author)
                .OrderBy(c => c.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);
        }
    }
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public interface IUnitOfWork:IDisposable
    {
        ICoursesRepository Courses { get; }
        //IAuthor
        int Save();
    }

    public class UnitOfWork : IUnitOfWork
    {
        public ICoursesRepository Courses { get; private set; }
        private readonly UniversityContext _universityContext;

        public UnitOfWork(UniversityContext universityContext)
        {
            _universityContext = universityContext;
            Courses = new CourseRepository(_universityContext);
        }
        

        public int Save()
        {
            return _universityContext.SaveChanges();
        }

        public void Dispose()
        {
            _universityContext.Dispose();
        }
    }
}
