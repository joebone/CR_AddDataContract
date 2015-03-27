using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Collections;
namespace Toolfactory.Home.Web.Models.Offers {
    public class OffersModel {
        //private Session _session;
        private string _cdnDomain;
        //private string _airport;
        //private MVCWeb.BaseTypes.Components.ComponentIncludes _includes;


        public TopBannerOffersModel TopBannerModel { get; set; }
        public MenuOffersModel MenuModel { get; set; }

        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Claim { get; set; }
        public string Meta { get; set; }
        public string Product { get; set; }


        public OffersModel(object toolfactorySession, Hashtable parameters)
            //: base(toolfactorySession, parameters["Mscdn"], parameters["PageName"], parameters["UrlCompleta"]) {
        {

        
            this._cdnDomain = (string)parameters["Mscdn"];
#if DEBUG
            _cdnDomain = "http://cdn.logitravel.com";
#endif
            //this._includes = new MVCWeb.BaseTypes.Components.ComponentIncludes() {
            //    CssManager = this.CssManager,
            //    JsInitializations = this.JsInitializations,
            //    JsManager = this.JsManager
            //};
        }

        public void LoadComponents() {
            return;
        }

        //public OffersModel SerDeser() {
        //    var ms = new MemoryStream();
        //    ProtoBuf.Serializer.Serialize(ms, this);
        //    var len = ms.Length;
        //    ms.Position = 0;

        //    return ProtoBuf.Serializer.Deserialize<OffersModel>(ms);



        //}
    }
}