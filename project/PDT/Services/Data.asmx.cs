using PDT.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

namespace PDT.Services
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "")]
    [ScriptService]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        [WebMethod(EnableSession = true)]
        public string GetMotorway()
        {
            var data = new SqlController().GetMotorway();
            return data;
        }

        [WebMethod(EnableSession = true)]
        public string GetRangeFromFiit(int range)
        {
            var data = new SqlController().GetRangeFromFiit(range);
            return data;
        }

        [WebMethod(EnableSession = true)]
        public object GetVillage(string village)
        {
            var data = new SqlController().GetVillage(village);
            return data;
        }
    }
}
