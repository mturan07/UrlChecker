using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UrlChecker.Web.Models.Abstract;

namespace UrlChecker.Web.Models.Concrete
{
    public class SmsService : ISenderService
    {
        public void Send(string from, string to, string subject, string message)
        {
            throw new NotImplementedException();
        }
    }
}
