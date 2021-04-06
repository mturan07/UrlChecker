using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UrlChecker.Web.Models
{
    public class TargetApp
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "App name is required")]
        public string AppName { get; set; }
        [Required(ErrorMessage = "App url is required")]
        public string AppUrl { get; set; }
        [Required(ErrorMessage = "App interval (cron) is required")]
        public string Interval { get; set; }
    }
}
