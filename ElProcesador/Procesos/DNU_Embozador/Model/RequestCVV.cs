using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class RequestCVV
    {
        public string CVKA { get; set; }
        public string KCVA { get; set; }
        public string CVKB { get; set; }
        public string KCVB { get; set; }
        public string PAN { get; set; }
        public string ExpirationDate { get; set; }
        public string ServiceCode { get; set; }
    }
}
