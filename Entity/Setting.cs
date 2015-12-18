namespace LoggingSample.Entity {
	using System.ComponentModel.DataAnnotations;

	// Not IEntity because primary key isn't an int
	public class Setting : IEntityLite {
		[Key]
		[Required]
		[StringLength(50)]
		public string SettingName { get; set; }
		[Required]
		[StringLength(2000)]
		public string SettingValue { get; set; }
	}
}
