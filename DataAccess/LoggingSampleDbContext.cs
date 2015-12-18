namespace LoggingSample.DataAccess {
	using System;
	using System.Data.Entity;
	using System.Data.Entity.ModelConfiguration.Conventions;
	using LoggingSample.Entity;
	using LoggingSample.Infrastructure;

	public interface ILoggingSampleDbContext : IDisposable {
		IDbSet<TEntity> GetTable<TEntity>() where TEntity : IEntity;
		void DefeatChangeTracking(object Entity);
		int SaveChanges();
		int? CommandTimeout { get; set; }
		Action<string> Logger { set; }

		IDbSet<ErrorLog> ErrorLogs { get; }
		IDbSet<Setting> Settings { get; }
		IDbSet<User> Users { get; }
	}

	public class LoggingSampleDbContext : DbContext, ILoggingSampleDbContext {

		// http://stackoverflow.com/questions/5035323/mocking-or-faking-dbentityentry-or-creating-a-new-dbentityentry
		public IDbSet<TEntity> GetTable<TEntity>() where TEntity : IEntity {
			return this.Set<TEntity>();
		}

		public void DefeatChangeTracking(object Entity) {
			this.Entry(Entity).State = EntityState.Modified;
		}

		public int? CommandTimeout {
			get { return this.Database.CommandTimeout; }
			set { this.Database.CommandTimeout = value; }
		}

		public Action<string> Logger {
			set { this.Database.Log = value; }
		}

		static LoggingSampleDbContext() {
			// Don't try to create the database if it doesn't exist, just fail instead
			Database.SetInitializer<LoggingSampleDbContext>(null);
		}

		public LoggingSampleDbContext()
			: base(Constant.CONNECTION_STRING_NAME) {
			this.Configuration.LazyLoadingEnabled = false;
			this.CommandTimeout = Constant.COMMAND_TIMEOUT;
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
			modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

			base.OnModelCreating(modelBuilder);
		}

		public IDbSet<ErrorLog> ErrorLogs { get; set; }
		public IDbSet<Setting> Settings { get; set; }
		public IDbSet<User> Users { get; set; }
	}
}
