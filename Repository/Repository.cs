namespace LoggingSample.Repository {
	using System;
	using System.Collections.Generic;
	using System.Data.Entity;
	using System.Linq;
	using LoggingSample.DataAccess;
	using LoggingSample.Entity;
	using LoggingSample.Library;

	public interface IRepository<TEntity> where TEntity : IEntity {
		TEntity GetById(int Id);
		bool Exists(int Id);
		List<TEntity> GetAll();
		List<TEntity> GetActive();
		void Save(TEntity Entity);
		void Save(List<TEntity> Entities);
		void Delete(int Id);
		void DeleteForever(int Id);
	}

	public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : IEntity {
		protected ILoggingSampleDbContextFactory Factory { get; private set; }
		protected IRetryinator Retryinator { get; private set; }
		protected IDbExceptionHandler DbExceptionHandler { get; private set; }

		protected Repository(ILoggingSampleDbContextFactory Factory, IRetryinator Retryinator, IDbExceptionHandler DbExceptionHandler) {
			if (Factory == null) {
				throw new ArgumentNullException("Factory");
			}
			if (Retryinator == null) {
				throw new ArgumentNullException("Retryinator");
			}
			if (DbExceptionHandler == null) {
				throw new ArgumentNullException("DbExceptionHandler");
			}
			this.Factory = Factory;
			this.Retryinator = Retryinator;
			this.DbExceptionHandler = DbExceptionHandler;
		}

		protected TResult Retry<TResult>(Func<TResult> Func) {
			return this.Retryinator.Retry(Func, this.DbExceptionHandler.RetryException);
		}

		public virtual TEntity GetById(int Id) {
			if (Id < 1) {
				return null; // No need to get nothing from no one
			}
			return this.Retry(() => {
				using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
					return db.GetTable<TEntity>().Find(Id); // This produces ugly SQL
				}
			});
		}

		public virtual bool Exists(int Id) {
			if (Id < 1) {
				return false; // No need to get nothing from no one
			}
			return this.Retry(() => {
				using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
					return db.GetTable<TEntity>().Find(Id) != null; // This produces ugly SQL
				}
			});
		}

		/// <summary>
		/// This is generally a bad idea
		/// </summary>
		public virtual List<TEntity> GetAll() {
			return this.Retry(() => {
				using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
					return (
						from r in db.GetTable<TEntity>()
						select r
					).ToList();
				}
			});
		}

		/// <summary>
		/// This is generally a bad idea
		/// </summary>
		public virtual List<TEntity> GetActive() {
			return this.Retry(() => {
				using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
					List<TEntity> result = (
						from r in db.GetTable<TEntity>()
						where r.IsActive
						select r
					).ToList();
					return result;
				}
			});
		}

		protected virtual void OnSaved() {
		}

		// FRAGILE: ASSUME: all navigation properties are null
		public virtual void Save(TEntity Entity) {
			Entity.ModifyDate = DateTime.Now;
			this.Retry(() => {
				using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
					IDbSet<TEntity> table = db.GetTable<TEntity>();
					if (Entity.IsNew()) {
						// Add
						table.Add(Entity);
					} else {
						// Update
						table.Attach(Entity);
						db.DefeatChangeTracking(Entity);
					}
					db.SaveChanges();
					// TODO: Catch concurrency errors
				}
				return 0;
			});
			this.OnSaved();
		}

		// FRAGILE: ASSUME: all navigation properties are null
		public virtual void Save(List<TEntity> Entities) {
			if (Entities.IsNullOrEmpty()) {
				return; // Successfully did nothing
			}
			this.Retry(() => {
				using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
					List<Exception> exs = new List<Exception>();
					foreach (TEntity entity in Entities) {
						entity.ModifyDate = DateTime.Now;
						IDbSet<TEntity> table = db.GetTable<TEntity>();
						try {
							if (entity.IsNew()) {
								// Add
								table.Add(entity);
							} else {
								// Update
								table.Attach(entity);
								db.DefeatChangeTracking(entity);
							}
						} catch (Exception ex) {
							exs.Add(ex);
						}
					}
					if (exs.IsNullOrEmpty()) {
						try {
							db.SaveChanges(); // Save at once so it'll all succeed or fail
						} catch (Exception ex) {
							// TODO: Catch concurrency errors separately
							exs.Add(ex);
						}
					}
					if (!exs.IsNullOrEmpty()) {
						throw new AggregateException(exs);
					}
				}
				return 0;
			});
			this.OnSaved();
		}

		public virtual void Delete(int Id) {
			if (Id < 1) {
				throw new ArgumentOutOfRangeException("Id", "Can't delete " + Id);
			}
			this.Retry(() => {
				using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
					TEntity entity = db.GetTable<TEntity>().Find(Id);
					if (entity != null) {
						entity.IsActive = false;
						entity.ModifyDate = DateTime.Now;
						db.SaveChanges();
					//} else {
						 // You tried to make it gone and it is -- success!
					}
				}
				return 0;
			});
			this.OnSaved();
		}

		public virtual void DeleteForever(int Id) {
			if (Id < 1) {
				// TODO: throw?
				return; // You tried to make it gone, it never existed -- success!
			}
			this.Retry(() => {
				using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
					IDbSet<TEntity> table = db.GetTable<TEntity>();
					TEntity entity = table.Find(Id);
					if (entity == null) {
						//throw new ArgumentNullException("Can't delete id " + Id + " because it doesn't exist", (Exception)null);
						return 0; // You tried to make it gone and it is -- success!
					}
					table.Remove(entity);
					db.SaveChanges();
				}
				return 0;
			});
			this.OnSaved();
		}

	}
}
