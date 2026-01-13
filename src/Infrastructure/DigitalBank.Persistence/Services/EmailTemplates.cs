using System.Net;

namespace DigitalBank.Persistence.Services.Email
{
    public static class EmailTemplates
    {
        public static string ConfirmEmail(string displayName, string confirmLink)
        {
            var name = WebUtility.HtmlEncode(displayName);

            return $@"
<!doctype html>
<html lang=""az"">
<head>
  <meta charset=""utf-8""/>
  <meta name=""viewport"" content=""width=device-width,initial-scale=1""/>
  <title>DigitalBank — Email təsdiqi</title>
</head>
<body style=""margin:0;background:#f5f7fb;padding:24px;font-family:Inter,Arial,sans-serif;color:#0f172a;"">
  <div style=""max-width:680px;margin:0 auto;"">
    <div style=""background:#0b57d0;border-radius:16px;padding:18px 22px;color:#fff;"">
      <div style=""font-size:14px;opacity:.9;"">DigitalBank</div>
      <div style=""font-size:18px;font-weight:700;margin-top:6px;"">Email təsdiqi</div>
    </div>

    <div style=""background:#ffffff;border:1px solid #e6eaf0;border-radius:16px;margin-top:14px;padding:22px;"">
      <p style=""margin:0 0 10px;"">Salam, <b>{name}</b>!</p>
      <p style=""margin:0 0 14px;line-height:1.55;color:#334155;"">
        DigitalBank hesabının təhlükəsizliyini təmin etmək üçün email ünvanını təsdiqləməyini xahiş edirik.
      </p>

      <div style=""margin:18px 0 18px;"">
        <a href=""{confirmLink}"" 
           style=""display:inline-block;background:#0b57d0;color:#fff;text-decoration:none;
                  padding:12px 18px;border-radius:12px;font-weight:600;"">
          Email ünvanını təsdiqlə
        </a>
      </div>

      <p style=""margin:0 0 10px;color:#64748b;font-size:13px;line-height:1.5;"">
        Düymə işləmirsə, aşağıdakı linki brauzerə yapışdır:
      </p>
      <p style=""margin:0 0 18px;font-size:12.5px;word-break:break-all;color:#0b57d0;"">{confirmLink}</p>

      <hr style=""border:none;border-top:1px solid #eef2f7;margin:18px 0;""/>

      <p style=""margin:0;color:#64748b;font-size:12.5px;line-height:1.55;"">
        Bu sorğunu sən etməmisənsə, bu emaili nəzərə alma və hesabının təhlükəsizliyi üçün şifrəni dəyiş.
      </p>
    </div>

    <div style=""text-align:center;color:#94a3b8;font-size:12px;margin-top:14px;"">
      © {DateTime.UtcNow.Year} DigitalBank • Təhlükəsizlik bölməsi
    </div>
  </div>
</body>
</html>";
        }
    }
}
