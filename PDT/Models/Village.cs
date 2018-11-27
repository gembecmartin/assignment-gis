using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDT.Models
{
    public class Village
    {
        public string Data { get; set; }
        public double Area { get; set; }

        public Village(string data, double area)
        {
            Data = data;
            Area = area;
        }
    }
}