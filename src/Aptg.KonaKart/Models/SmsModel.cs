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
    public class SmsModel
    {
        /// <summary>
        /// 簡訊主旨，主旨不會隨著簡訊內容發送出去。用以註記本次發送之用途。可傳入空字串
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 簡訊發送內容
        /// </summary>
        public string Content { get; set; }
    }
}
