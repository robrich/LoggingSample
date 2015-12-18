namespace LoggingSample.Repository {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using LoggingSample.DataAccess;
	using LoggingSample.Entity;
	using LoggingSample.Library;
	using LoggingSample.Library.Cache;

	public interface IRepositoryCache<TEntity> : IRepository<TEntity> where TEntity : IEntity {
		void ClearCache();
	}

	public class RepositoryCache<TEntity> : Repository<TEntity>, IRepositoryCache<TEntity> where TEntity : IEntity {
		protected readonly IAppCache<List<TEntity>> getAllCache;

		public RepositoryCache(ILoggingSampleDbContextFactory Factory, IRetryinator Retryinator, IDbExceptionHandler DbExceptionHandler, IAppCache<List<TEntity>> GetAllCache)
			: base(Factory, Retryinator, DbExceptionHandler) {
			if (GetAllCache == null) {
				throw new ArgumentNullException("GetAllCache");
			}
			this.getAllCache = GetAllCache;
		}

		protected override void OnSaved() {
			this.getAllCache.Clear();
		}

		public virtual void ClearCache() {
			this.getAllCache.Clear();
		}

		public override TEntity GetById(int Id) {
			if (Id < 1) {
				return null; // No need to get nothing
			}
			return (
				from l in this.GetAll()
				where l.Id == Id
				select l
			).FirstOrDefault();
		}

		public override bool Exists(int Id) {
			if (Id < 1) {
				return false; // No need to get nothing
			}
			return (
				from l in this.GetAll()
				where l.Id == Id
				select l
			).Any();
		}

		public override List<TEntity> GetAll() {
			return this.getAllCache.Retrieve(base.GetAll);
		}

		public override List<TEntity> GetActive() {
			return (
				from l in this.GetAll()
				where l.IsActive
				select l
			).ToList();
		}

	}
}
