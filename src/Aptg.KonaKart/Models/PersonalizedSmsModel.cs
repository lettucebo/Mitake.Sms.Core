using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aptg.KonaKart.Models
{
    /// <summary>
    /// 簡訊內容
    /// </summary>
    public class PersonalizedSmsModel
    {
        public string Name { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }

        public string Content { get; set; }

        public DateTime? SendTime { get; set; }
    }
}
