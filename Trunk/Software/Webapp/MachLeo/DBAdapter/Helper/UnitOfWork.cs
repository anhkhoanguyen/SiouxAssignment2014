using System.Data.Entity;
using System.Transactions;
using DBAdapter.DataContext;

namespace DBAdapter.Helper
{
	public class UnitOfWork : IUnitOfWork
	{
		private TransactionScope _transaction;
		private DbContext _dbContext;

		public UnitOfWork()
		{
			_dbContext = new ApplicationDBContext();
			_dbContext.Configuration.ProxyCreationEnabled = false;
			_dbContext.Configuration.LazyLoadingEnabled = false;
		}

		public void Dispose()
		{
		}

		public void StartTransaction()
		{
			_transaction = new TransactionScope();
		}

		public void Commit()
		{
			_dbContext.SaveChanges();
			_transaction.Complete();
		}

		public DbContext DbContext
		{
			get { return _dbContext; }
			set { _dbContext = value ;}
		}
	}
}
