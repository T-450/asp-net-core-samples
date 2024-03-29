using System.ComponentModel.DataAnnotations;

namespace JWTAPI.Controllers.Resources
{
    public class RevokeTokenResource
    {
        [Required]
        public string Token { get; set; }

		[Required]
		public string Email { get; set; }
	}
}