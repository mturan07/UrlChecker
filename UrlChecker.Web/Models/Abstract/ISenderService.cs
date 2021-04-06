using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlChecker.Web.Models.Abstract
{
    public interface ISenderService
    {
        void Send(string from, string to, string subject, string message);
    }
}
