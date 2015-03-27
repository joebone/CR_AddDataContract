using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using System.Web;

namespace Toolfactory.Home.Web.Models.Offers {
    public class Menu {
        public string Name { get; set; }
        public string Id { get; set; }
        public string CssIcon { get; set; }
        public object LineOfBusiness { get; set; }
    }
}