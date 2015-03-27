using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Mvc;
//using Toolfactory.Core.BaseTypes;
using System.Runtime.Serialization;
//using ProtoBuf;
//using Toolfactory.Home.Web.Helpers;
using System.Collections;

namespace Toolfactory.Home.Web.Models.Offers {

    public class MenuOffersModel {

        public List<Menu> MenuList { get; set; }
        public Application Application { get; set; }

        //        public Language Language { get; set; }

        //        //public CommonRequestParams RqParams { get; private set; }
        //        //[ProtoMember(5)]
        //        //public FormCollection Parameters { get; private set; }
        //        public MenuOffersModel(Hashtable parameters) {
        //            this.Parameters = parameters;

        //            this.RqParams = new CommonRequestParams() {
        //                Airport = parameters["Airport"],
        //                ApplicationGroup = parameters["ApplicationGroup"],
        //                Brand = parameters["Brand"],
        //                CdnDomain = parameters["Mscdn"],
        //                FlightsShowcase = parameters["flightsShowcaseUrl"],
        //                Language = (Language)Enum.Parse(typeof(Language), parameters["LanguageCode"], true),
        //                SessionCode = parameters["SessionCode"],
        //                UrlHttp = parameters["UrlCompleta"],
        //                WebUserCode = long.Parse(parameters["UserCode"]),
        //                Application = Toolfactory.Core.BaseTypes.ApplicationExtensions.GetApplicationValue(parameters["Application"])
        //            };
        //            MenuList = new List<Menu>();
        //        }
        //        public MenuOffersModel(CommonRequestParams rqparams) {
        //            //this.Parameters = new FormCollection();
        //            this.RqParams = rqparams;
        //            if (rqparams != null) {
        //                this.Language = rqparams.Language;
        //                this.Application = rqparams.Application;
        //            }
        //            MenuList = new List<Menu>();
        //        }
        //    }
    }
}
