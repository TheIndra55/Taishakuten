using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Taishakuten.Entities
{
    [Index(nameof(User), nameof(At))]
    class Reminder
    {
        public int Id { get; set; }

        [Required]
        public ulong Channel { get; set; }

        [Required]
        public ulong Guild { get; set; }

        [Required]
        public ulong User { get; set; }

        [Required]
        public DateTime At { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; }

        public bool Fired { get; set; } = false;

        [MaxLength(500)]
        public string LastError { get; set; } = null;
    }
}
