using System.Collections.Generic;
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Data;

namespace DBAdapter.Helper
{
	/// <summary>
	/// Repository provider for Entity Framework
	/// See base class for method comments
	/// </summary>
	public class BaseRepository<T> : IRepository<T> where T : class
	{
		private readonly IUnitOfWork _unitOfWork;
		private DbSet<T> _dbSet;
		public string[] Includes { get; set; }

		public BaseRepository(IUnitOfWork unitOfWork)
		{
			if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
			_unitOfWork = unitOfWork;
			_dbSet = _unitOfWork.DbContext.Set<T>();
		}

        public T GetById(Guid id) 
        {
            return _dbSet.Find(id);
        }

		public IQueryable<T> All(string[] includes = null) 
		{
			//HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
			if (includes != null && includes.Count() > 0)
			{
				var query = _dbSet.Include(includes.First());
				foreach (var include in includes.Skip(1))
					query = query.Include(include);
				return query.AsQueryable();
			}

			return _dbSet.AsQueryable();
		}

		public virtual T FindOne(Expression<Func<T, bool>> predicate, string[] includes = null) 
		{
            try
            {
                //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
                if (includes != null && includes.Count() > 0)
                {
                    var query = _dbSet.Include(includes.First());

                    foreach (var include in includes.Skip(1))
                        query = query.Include(include);

                    return query.FirstOrDefault<T>(predicate);
                }

				return _dbSet.FirstOrDefault<T>(predicate);
            }
            catch (Exception e){ 
                //TODO Write log
                return null;
            }
		}

		public virtual IQueryable<T> Filter(Expression<Func<T, bool>> predicate, string[] includes = null) 
		{
			try
            {
                //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
			    if (includes != null && includes.Count() > 0)
			    {
				    var query = _dbSet.Include(includes.First());
				    foreach (var include in includes.Skip(1))
					    query = query.Include(include);
				    return query.Where<T>(predicate).AsQueryable<T>();
			    }

			    return _dbSet.Where<T>(predicate).AsQueryable<T>();
            }
            catch (Exception e){ 
                //TODO Write log
                return null;
            }
		}

		public virtual IQueryable<T> Filter(Expression<Func<T, bool>> predicate, out int total, int index = 0, int size = 50, string[] includes = null) 
		{
			int skipCount = index * size;
			IQueryable<T> resetSet;

            try
            {
			    //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
			    if (includes != null && includes.Count() > 0)
			    {
				    var query = _dbSet.Include(includes.First());

				    foreach (var include in includes.Skip(1))
				    {
					    query = query.Include(include);
				    }

				    resetSet = predicate != null ? query.Where<T>(predicate).AsQueryable() : query.AsQueryable();
			    }
			    else
			    {
				    resetSet = predicate != null ? _dbSet.Where<T>(predicate).AsQueryable() : _dbSet.AsQueryable();
			    }

			    resetSet = skipCount == 0 ? resetSet.Take(size) : resetSet.Skip(skipCount).Take(size);
			    total = resetSet.Count();

			    return resetSet.AsQueryable();
            }
            catch (Exception e){ 
                //TODO Write log
                total = 0;
                return null;
            }
		}

		public virtual T Create(T TObject) 
		{
            try
            {
                /*//ADD CREATE DATE IF APPLICABLE
                if (TObject is ICreatedOn)
                {
                        (TObject as ICreatedOn).CreatedOn = DateTime.UtcNow;
                }

                //ADD LAST MODIFIED DATE IF APPLICABLE
                if (TObject is IModifiedOn)
                {
                        (TObject as IModifiedOn).ModifiedOn = DateTime.UtcNow;
                }*/

                var newEntry = _dbSet.Add(TObject);
				_unitOfWork.DbContext.SaveChanges();

                return newEntry;
            }
            catch
            {
                //TODO Write log
                return null;
            }
		}

	
		public virtual int Delete(T TObject) 
		{
            try
            {
				_dbSet.Remove(TObject);

				return _unitOfWork.DbContext.SaveChanges();
            }
            catch
            {
                //TODO Write log
                return 0;
            }
        }

		public virtual int Update(T TObject) 
		{
            try
            {
				_dbSet.Attach(TObject);
				_unitOfWork.DbContext.Entry(TObject).State = EntityState.Modified;
				return _unitOfWork.DbContext.SaveChanges();
            }
            catch(Exception e) {
                //TODO Write log
                return 0;
            }
		}

		public virtual int Delete(Expression<Func<T, bool>> predicate) 
		{
            try
            {
                var objects = Filter(predicate);

                foreach (var obj in objects)
                {
                    _dbSet.Remove(obj);
                }
				return _unitOfWork.DbContext.SaveChanges();
            }
            catch
            {
                //TODO Write log
                return 0;
            }
		}

		public bool Contains(Expression<Func<T, bool>> predicate) 
		{
			return _dbSet.Count(predicate) > 0;
		}

		public virtual void ExecuteProcedure(String procedureCommand, params SqlParameter[] sqlParams)
		{
			_unitOfWork.DbContext.Database.ExecuteSqlCommand(procedureCommand, sqlParams);
		}

		public virtual void SaveChanges()
		{
			_unitOfWork.DbContext.SaveChanges();
		}

		public void Dispose()
		{
			if (_unitOfWork.DbContext != null)
				_unitOfWork.DbContext.Dispose();
		}
	}
}