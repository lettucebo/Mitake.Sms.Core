using System.ComponentModel;

namespace Mitake.Sms.Core.Models;

public enum StatusFlag
{
    [Description("預約傳送中")] Zero,

    [Description("已送達業者")] One,

    [Description("已送達業者")] Two,

    [Description("已送達手機")] Four,

    [Description("內容有錯誤")] Five,

    [Description("門號有錯誤")] Six,

    [Description("簡訊已停用")] Seven,

    [Description("逾時無送達")] Eight,

    [Description("預約已取消")] Nine,

    [Description("簡訊發送功能暫時停止服務，請稍候再試")] A,

    [Description("簡訊發送功能暫時停止服務，請稍候再試")] B,

    [Description("請輸入帳號")] C,

    [Description("請輸入密碼")] D,

    [Description("帳號、密碼錯誤")] E,

    [Description("帳號已過期")] F,

    [Description("帳號已被停用")] H,

    [Description("無效的連線位址")] K,

    [Description("帳號已達到同時連線數上限")] L,

    [Description("必須變更密碼，在變更密碼前，無法使用簡訊發送服務")]
    M,

    [Description("密碼已逾期，在變更密碼前，將無法使用簡訊發送服務")]
    N,

    [Description("沒有權限使用外部Http程式")] P,

    [Description("系統暫停服務，請稍後再試")] R,

    [Description("帳務處理失敗，無法發送簡訊")] S,

    [Description("簡訊已過期")] T,

    [Description("簡訊內容不得為空白")] U,

    [Description("無效的手機號碼")] V,

    [Description("查詢筆數超過上限")] W,

    [Description("發送檔案過大，無法發送簡訊")] X,

    [Description("參數錯誤")] Y,

    [Description("查無資料")] Z,

    [Description("系統發生錯誤，請聯絡三竹資訊窗口人員")] Star
}