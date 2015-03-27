using System;
using System.Collections.Generic;
using System.Linq;

//using Toolfactory.Core.BaseTypes;
using System.Runtime.Serialization;
using ProtoBuf;
using Toolfactory.Home.Web.Models.Offers;

// namespaces...
namespace Toolfactory.Home.Web.Models.Offers {
    #region Enums
    public enum Application {
        Logitravel,
        LogitravelUK,
        LogitravelIT,
        LogitravelDE,
        LogitravelFR,
        LogitravelPT
    }
    #endregion

    #region Public Classes
    [ProtoContract]
    public class TopBannerOffersModel {
        #region Constructors
        public TopBannerOffersModel() {
        }
        #endregion

        // public properties...
        [ProtoMember(1)]
        public string AltImage { get; set; }
        [ProtoMember(2)]
        public Application Application { get; set; }
        [ProtoMember(3)]
        public string CdnDomain { get; set; }
        [ProtoMember(4)]
        public string ImageBanner { get; set; }
        public string ImageComplete {
            get {
                return string.Format("{0}/{1}/{2}", CdnDomain, Application.StringValue(), ImageBanner);
            }
        }
    }
    #endregion

    #region Static
    public static class XT {
        #region Static Methods & Properties
        public static string StringValue(this Application app) {
            return "Logitravel";
        }
        #endregion
    }
    #endregion
}