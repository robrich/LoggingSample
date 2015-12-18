namespace LoggingSample.Repository {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Configuration;
	using System.Linq;
	using LoggingSample.DataAccess;
	using LoggingSample.Entity;
	using LoggingSample.Library;
	using LoggingSample.Library.Cache;

	public interface ISettingRepository {
		void Save(string Key, string Value);
		/// <summary>
		/// For values that change frequently, doesn't clear cach
		/// </summary>
		void SaveNoCache(string Key, string Value);
		List<Setting> GetAll();
		string EnvorionmentName { get; }
		string EmailFrom { get; }
		string ErrorEmail { get; }
	}

	public class SettingRepository : ISettingRepository {
		private readonly ILoggingSampleDbContextFactory LoggingSampleDbContextFactory;
		private readonly IAppCache<List<Setting>> settingCache;

		public SettingRepository(ILoggingSampleDbContextFactory LoggingSampleDbContextFactory, IAppCache<List<Setting>> SettingCache) {
			if (LoggingSampleDbContextFactory == null) {
				throw new ArgumentNullException("LoggingSampleDbContextFactory");
			}
			if (SettingCache == null) {
				throw new ArgumentNullException("SettingCache");
			}
			this.LoggingSampleDbContextFactory = LoggingSampleDbContextFactory;
			this.settingCache = SettingCache;

			this.settingCache.KeyPrefix = "Setting";
		}

		public void Save(string Key, string Value) {
			using (ILoggingSampleDbContext db = this.LoggingSampleDbContextFactory.GetContext()) {
				Setting s = (
					from ss in db.Settings
					where ss.SettingName == Key
					select ss
				).FirstOrDefault();
				if (s == null) {
					s = new Setting {
						SettingName = Key
					};
					db.Settings.Add(s);
				}
				s.SettingValue = Value;
				db.SaveChanges();
			}
			// TODO: log? notify?
			this.settingCache.Clear();
		}

		/// <summary>
		/// For values that change frequently, doesn't clear cache
		/// </summary>
		public void SaveNoCache(string Key, string Value) {
			using (ILoggingSampleDbContext db = this.LoggingSampleDbContextFactory.GetContext()) {
				Setting s = (
					from ss in db.Settings
					where ss.SettingName == Key
					select ss
				).FirstOrDefault();
				if (s == null) {
					s = new Setting {
						SettingName = Key
					};
					db.Settings.Add(s);
				}
				s.SettingValue = Value;
				db.SaveChanges();
			}
		}

		public List<Setting> GetAll() {
			return this.settingCache.Retrieve(() => {
				using (ILoggingSampleDbContext db = this.LoggingSampleDbContextFactory.GetContext()) {
					return (
						from s in db.Settings
						orderby s.SettingName
						select s
					).ToList();
				}
			});
		}

		public string GetByKey(string Key) {
			string value = ConfigurationManager.AppSettings[Key]; // appSettings takes precidence
			if (!string.IsNullOrEmpty(value)) {
				return value;
			}
			return (
				from s in this.GetAll()
				where s.SettingName == Key
				select s.SettingValue
			).FirstOrDefault();
		}

		public string GetByKeyNoCache(string Key) {
			string value = ConfigurationManager.AppSettings[Key]; // appSettings takes precidence
			if (!string.IsNullOrEmpty(value)) {
				return value;
			}
			using (ILoggingSampleDbContext db = this.LoggingSampleDbContextFactory.GetContext()) {
				return (
					from s in db.Settings
					where s.SettingName == Key
					select s.SettingValue
				).FirstOrDefault();
			}
		}

		[NotMapped]
		public string EnvorionmentName {
			get {
				string indexName = ConfigurationManager.AppSettings["EnvorionmentName"]; // appSettings takes precidence
				if (!string.IsNullOrEmpty(indexName)) {
					return indexName;
				}
				return "local"; // TODO: impliment this
			}
		}

		public string EmailFrom {
			get { return this.GetByKey("EmailFrom"); }
		}

		public string ErrorEmail {
			get { return this.GetByKey("ErrorEmail"); }
		}

	}
}
