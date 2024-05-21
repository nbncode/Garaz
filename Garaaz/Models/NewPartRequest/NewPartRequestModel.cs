using System.ComponentModel.DataAnnotations;

namespace Garaaz.Models
{
    public class NewPartRequestModel : BaseRequestModel
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets user id of the user on whose behalf or who is requesting for new part.
        /// </summary>
        [Required(ErrorMessage = "User Id is required.")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets user id of the user who is logged in app.
        /// </summary>
        public string RefUserId { get; set; }

        public string WorkshopCode { get; set; }
        public string WorkshopName { get; set; }
        public string Mobile { get; set; }
        public int? TempOrderId { get; set; }
        public string Base64Image { get; set; }
    }
}