namespace LoggingSample.Entity {
	using System.ComponentModel.DataAnnotations;

	public class User : IEntity {

		[EmailAddress]
		[StringLength(255)]
		public string Email { get; set; }

		[StringLength(50)]
		public string FirstName { get; set; }

		[StringLength(50)]
		public string SessionToken { get; set; }

	}
}
