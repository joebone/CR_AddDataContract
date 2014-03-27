using System.ComponentModel.Composition;
using DevExpress.CodeRush.Common;

namespace CR_AddDataContract
{
    [Export(typeof(IVsixPluginExtension))]
    public class CR_AddDataContractExtension : IVsixPluginExtension { }
}