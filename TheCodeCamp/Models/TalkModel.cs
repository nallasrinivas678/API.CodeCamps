using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheCodeCamp.Models
{
    public class TalkModel
    {
        [Required]
        public int TalkId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [StringLength(4096,MinimumLength =100)]
        public string Abstract { get; set; }
        [Required]
        [Range(100,500)]
        public int Level { get; set; }
        public SpeakerModel Speaker { get; set; }
    }
}