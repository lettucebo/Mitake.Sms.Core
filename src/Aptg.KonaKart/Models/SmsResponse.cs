using System;

namespace Aptg.KonaKart.Models
{
    public class SmsResponse
    {
        /// <summary>
        /// 發送後剩餘點數
        /// 負值代表發送失敗，系統無法處理該命令
        /// </summary>
        public double Credit { get; set; }

        /// <summary>
        /// 發送通數
        /// </summary>
        public int Sent { get; set; }

        /// <summary>
        /// 本次發送扣除點數
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// 無額度時發送的通數
        /// 當該值大於 0 而剩餘點數等於 0 時表示有部份的簡訊因無額度而無法被發送
        /// </summary>
        public int UnSend { get; set; }

        /// <summary>
        /// 批次識別代碼
        /// 唯一識別碼，可藉由本識別碼查詢發送狀態
        /// </summary>
        public Guid BatchId { get; set; }
    }
}
