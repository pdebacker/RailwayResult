using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailwayResultTests.StubDomain
{
    public class SmtpService
    {
        public static bool SendMail(string recipient, string subject, string body)
        {
            switch (recipient)
            {
                case "valid@mail.com": return true;
                case "failed@mail.com": return false;
                case "exception@mail.com": throw new SmptException("SmptService Exception");
            }
            throw new SmptException("don't know what to do");
        }
    }

    public class SmptException : ApplicationException
    {
        public SmptException(string message) : base(message) { }
    }
}
