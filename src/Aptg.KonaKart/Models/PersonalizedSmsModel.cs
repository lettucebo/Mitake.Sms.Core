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
        /// <summary>
        /// 接收人姓名(可為空字串)。若有帶入姓名，將於發送紀錄中出現，以方便識別
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 收訊人電話(必須傳入)。若傳送電話為國內手機時，如 0933123456。請務必轉為 +886933123456，避免點數計算錯誤
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 收訊人之EMail(可為空字串)。EVERY8D 會將簡訊內容一併傳送一份 Email 給收訊人，此部份不收費
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 簡訊內容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 如需預約不同傳送時間請在此設定；否，則保留空值
        /// </summary>
        public DateTime? SendTime { get; set; }
    }
}
