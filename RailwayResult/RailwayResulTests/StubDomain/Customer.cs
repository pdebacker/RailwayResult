using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailwayResultTests.StubDomain
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal OrderLimit { get; set; }

        public string EmailAddress { get; set; }
    }
}
