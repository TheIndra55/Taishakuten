using System.ComponentModel.DataAnnotations;

namespace Taishakuten.Entities
{
    class Welcome
    {
        [Key]
        public ulong Guild { get; set; }

        [Required]
        public ulong Channel { get; set; }

        [MaxLength(256)]
        public string Title { get; set; }

        [MaxLength(4096)]
        public string Body { get; set; }

        public int Color { get; set; }

        public bool Mention { get; set; }
    }
}
