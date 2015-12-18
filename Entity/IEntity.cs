namespace LoggingSample.Entity {
	using System;
	using System.ComponentModel.DataAnnotations;

	//For weird entities that don't have a singular int primary key or date/id properties
	/// <summary>
	/// It is an interface, but to avoid &quot;where T : IEntity, new()&quot; we make it abstract instead
	/// </summary>
	public abstract class IEntityLite {
	}

	/// <summary>
	/// It is an interface, but to avoid &quot;where T : IEntity, new()&quot; we make it abstract instead
	/// </summary>
	public abstract class IEntity : IEntityLite {
		protected IEntity() {
			this.CreateDate = DateTime.Now;
			this.ModifyDate = DateTime.Now;
			this.IsActive = true;
		}

		[Key]
		public int Id { get; set; }

		public DateTime CreateDate { get; set; }
		public DateTime ModifyDate { get; set; }

		public bool IsActive { get; set; }

		public bool IsNew() {
			return this.Id < 1;
		}

	}
}
