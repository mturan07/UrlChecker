using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlChecker.Web.Models
{
    public class TargetApp
    {
        public int Id { get; set; }
        public string AppName { get; set; }
        public string AppUrl { get; set; }
        public string Interval { get; set; }
    }
}
