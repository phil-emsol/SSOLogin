using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;

namespace SSOLogin
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;

            string strClaims = "";
            foreach (System.Security.Claims.Claim claim in claimsIdentity.Claims)
            {
                strClaims += "&" + claim.Type + "=" + claim.Value;
            }

            string strCookie = Encrypt("Encrypt Me", "90033E3984CEF5A659C44BBB47299B4208374FB5DC495C96", "E6B9AFA7A282A0CA");

            //create a cookie
            HttpCookie myCookie = new HttpCookie("EMSWMCookie");

            //Add key-values in the cookie
            myCookie.Values.Add("SSOValidation", strCookie);
            myCookie.Values.Add("SSOClaims", strClaims);


            //set cookie expiry date-time. Made it to last for next 12 hours.
            myCookie.Expires = DateTime.Now.AddMinutes(5);

            //Most important, write the cookie to client.
            Response.Cookies.Add(myCookie);

            string realm = ConfigurationManager.AppSettings["redirectURL"];
            Response.Redirect(realm);
        }

        public static string Encrypt(string data, string key, string iv)
        {
            byte[] bdata = Encoding.ASCII.GetBytes(data);
            byte[] bkey = HexToBytes(key);
            byte[] biv = HexToBytes(iv);

            TripleDESCryptoServiceProvider des3 = new TripleDESCryptoServiceProvider();

            var stream = new MemoryStream();
            var encStream = new CryptoStream(stream,
                des3.CreateEncryptor(bkey, biv), CryptoStreamMode.Write);

            encStream.Write(bdata, 0, bdata.Length);
            encStream.FlushFinalBlock();
            encStream.Close();

            return BytesToHex(stream.ToArray());
        }

        public static byte[] HexToBytes(string text)
        {
            return Enumerable
                        .Range(0, text.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(text.Substring(x, 2), 16))
                        .ToArray();
        }

        public static string BytesToHex(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}