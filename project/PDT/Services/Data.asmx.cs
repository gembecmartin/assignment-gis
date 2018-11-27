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
        public string GetDepos()
        {
            var data = new SqlController().GetData();
            return data;
        }

        [WebMethod(EnableSession = true)]
        public string GetAeroStations()
        {
            var data = new SqlController().GetRangeFromFiit();
            return data;
        }

        [WebMethod(EnableSession = true)]
        public object GetSomething(string village)
        {
            var data = new SqlController().GetSomething(village);
            return data;
        }
    }
}
