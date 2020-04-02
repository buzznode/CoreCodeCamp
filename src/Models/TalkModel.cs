using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Models
{
    public class TalkModel
    {
        public int TalkId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MinLength(20), MaxLength(4000)]
        public string Abstract { get; set; }
        [Range(100, 300)]
        public int Level { get; set; }
        public SpeakerModel Speaker { get; set; }
    }
}
