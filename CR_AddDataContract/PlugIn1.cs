//using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Serialization;
using DevExpress.CodeRush.Core;
using DevExpress.CodeRush.PlugInCore;
using DevExpress.CodeRush.StructuralParser;
using System.Xml.Linq;
//using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using NuGet;
using Microsoft.VisualStudio.ComponentModelHost;
using NuGet.VisualStudio;

namespace CR_AddDataContract {
    public partial class PlugIn1 : StandardPlugIn {
        // DXCore-generated code...
        #region InitializePlugIn
        public override void InitializePlugIn() {
            base.InitializePlugIn();
            registerAddDataContract();
        }
        #endregion
        #region FinalizePlugIn
        public override void FinalizePlugIn() {
            //
            // TODO: Add your finalization code here.
            //

            base.FinalizePlugIn();
        }
        #endregion
        public void registerAddDataContract() {
            #region Add Data Contract
            CodeProvider AddDataContract = new CodeProvider(components);
            ((System.ComponentModel.ISupportInitialize)(AddDataContract)).BeginInit();
            AddDataContract.ProviderName = "AddDataContract"; // Should be Unique
            AddDataContract.DisplayName = "Add DataContract";
            AddDataContract.CheckAvailability += AddDataContract_CheckAvailability;
            AddDataContract.Apply += AddDataContract_Apply;
            ((System.ComponentModel.ISupportInitialize)(AddDataContract)).EndInit();
            #endregion

            #region Remove Data Contract
            CodeProvider RemoveDataContract = new CodeProvider(components);
            ((System.ComponentModel.ISupportInitialize)(RemoveDataContract)).BeginInit();
            RemoveDataContract.ProviderName = "RemoveDataContract "; // Should be Unique
            RemoveDataContract.DisplayName = "Remove DataContract";
            RemoveDataContract.CheckAvailability += RemoveDataContract_CheckAvailability;
            RemoveDataContract.Apply += RemoveDataContract_Apply;
            ((System.ComponentModel.ISupportInitialize)(RemoveDataContract)).EndInit();
            #endregion

            #region Add ProtoContract
            CodeProvider AddProtoContract = new CodeProvider(components);
            ((System.ComponentModel.ISupportInitialize)(AddProtoContract)).BeginInit();
            AddProtoContract.ProviderName = "AddProtoContract"; // Should be Unique
            AddProtoContract.DisplayName = "Add Protobuf Contract";
            AddProtoContract.CheckAvailability += AddDataContract_CheckAvailability;
            AddProtoContract.Apply += AddProtoContract_Apply;
            ((System.ComponentModel.ISupportInitialize)(AddProtoContract)).EndInit();
            #endregion
        }
        private void AddDataContract_CheckAvailability(object sender, CheckContentAvailabilityEventArgs ea) {
            // Limit availability to when the caret is within the name of the active class.
            if (CodeRush.Source.ActiveClass == null)
                return; // No active class
            if (!CodeRush.Source.ActiveClass.NameRange.Contains(CodeRush.Caret.SourcePoint))
                return;  // Caret not in class name
            ea.Available = true;
        }

        #region Language stuff
        private void ConfigureLanguageConstants(TextDocument activeDoc) {
            ActiveLanguage = activeDoc.Language;
            if (activeDoc.Language == "Basic") {
                TrueConstant = "True";
                AttributeValueAssignmentOperator = ":=";
                FalseConstant = "False";
            }
            else {
                TrueConstant = "true";
                AttributeValueAssignmentOperator = "=";
                FalseConstant = "false";
            }
        }
        private string AttributeValueAssignmentOperator { get; set; }
        private string TrueConstant { get; set; }
        private string FalseConstant { get; set; }
        private string ActiveLanguage { get; set; }
        #endregion

        #region Add Attribute

        private void AddDataContract_Apply(object sender, ApplyContentEventArgs ea) {
            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;

            ConfigureLanguageConstants(ActiveDoc);

            using (ActiveDoc.NewCompoundAction("Add DataContract")) {
                // Add Namespace Reference
                AddNamespaceReference("System.Runtime.Serialization");
                CodeRush.Project.AddReference(ActiveDoc.ProjectElement, "System.Runtime.Serialization");

                AddAttribute(CodeRush.Source.ActiveClass, "DataContract", -1);
                int dataOrder = 0;
                foreach (Property prop in CodeRush.Source.ActiveClass.AllProperties) {
                    // Add DataMember Attribute
                    AddAttribute(prop, "DataMember", ++dataOrder);
                }

                foreach (LanguageElement prop in CodeRush.Source.ActiveClass.AllFields) {
                    // Add DataMember Attribute
                    AddAttribute(prop, "DataMember", ++dataOrder);
                }
                CodeRush.Documents.ActiveTextDocument.ApplyQueuedEdits();
                CodeRush.Documents.ActiveTextDocument.ParseIfNeeded();
                CodeRush.Actions.Get("FormatFile").DoExecute();


            }
        }
        private static void AddNamespaceReference(string NamespaceName) {
            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
            var finder = new ElementEnumerable(ActiveDoc.FileNode, LanguageElementType.NamespaceReference, true);
            var NamespaceReferences = finder.OfType<NamespaceReference>();
            if (NamespaceReferences.Any(ns => ns.Name == NamespaceName))
                return;

            // Calculate Insert Location
            SourcePoint InsertionPoint;
            if (ActiveDoc.NamespaceReferences.Count <= 0) {
                InsertionPoint = ActiveDoc.Range.Start;
            }
            else {
                InsertionPoint = NamespaceReferences.Last().Range.Start;
            }

            // Generate new NamespaceReference
            var Code = CodeRush.CodeMod.GenerateCode(new NamespaceReference(NamespaceName));
            ActiveDoc.QueueInsert(InsertionPoint, Code);
        }

        private static void AddFullReference(string NamespaceName, string dllName, string Version, string CultureNCrap, string hintpath) {
            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
            var finder = new ElementEnumerable(ActiveDoc.FileNode, LanguageElementType.NamespaceReference, true);
            var NamespaceReferences = finder.OfType<NamespaceReference>();
            if (NamespaceReferences.Any(ns => ns.Name == NamespaceName))
                return;

            ActiveDoc.Project.AddReference(dllName);

            // Calculate Insert Location
            SourcePoint InsertionPoint;
            if (ActiveDoc.NamespaceReferences.Count <= 0) {
                InsertionPoint = ActiveDoc.Range.Start;
            }
            else {
                InsertionPoint = NamespaceReferences.Last().Range.Start;
            }

            // Generate new NamespaceReference
            var Code = CodeRush.CodeMod.GenerateCode(new NamespaceReference(NamespaceName));
            ActiveDoc.QueueInsert(InsertionPoint, Code);
        }
        private void AddAttribute(LanguageElement element, string AttributeName, int? DataOrder = null) {
            //var Builder = new ElementBuilder();
            var Builder = CodeRush.Language.GetElementBuilder(ActiveLanguage); //DevExpress.CodeRush.Common.Constants.Str.Language.CSharp

            var Attribute = Builder.BuildAttribute(AttributeName);

            if (AttributeName == "DataMember") {

                if (DataOrder.HasValue) {
                    Attribute.Arguments.Add(Builder.BuildAssignmentExpression("Order" + AttributeValueAssignmentOperator, DataOrder, AssignmentOperatorType.None));
                }
                Attribute.Arguments.Add(Builder.BuildAssignmentExpression("Name" + AttributeValueAssignmentOperator, "\"" + element.Name + "\"", AssignmentOperatorType.None));
                Attribute.Arguments.Add(Builder.BuildAssignmentExpression("EmitDefaultValue" + AttributeValueAssignmentOperator, FalseConstant, AssignmentOperatorType.None));
                Attribute.Arguments.Add(Builder.BuildAssignmentExpression("IsRequired" + AttributeValueAssignmentOperator, FalseConstant, AssignmentOperatorType.None));



                //foreach (var arg in Attribute.Arguments.OfType<AssignmentExpression>().ToList()) {
                //    // Devexpress too stupid to code attribute assignments correctly in both languages.
                //    var argg = new AssignmentExpression(arg.LeftSide, AttributeValueAssignmentOperator, arg.RightSide);
                //    Attribute.Arguments.Remove(arg);
                //    Attribute.Arguments.Add(argg);
                //    //new ElementReferenceExpression("IsRequired"), AttributeValueAssignmentOperator, new PrimitiveExpression(FalseConstant)));
                //    //arg.OperatorText = AttributeValueAssignmentOperator;
                //    //arg.NewTokens;
                //}


                //AssignmentExpression asign = ;
                //    new AssignmentExpression(
                //        new ElementReferenceExpression("EmitDefaultValue"), AttributeAssignmentOperator, new PrimitiveExpression(FalseConstant)));
                //Attribute.Arguments.Add(
                //   new AssignmentExpression(
                //       new ElementReferenceExpression("IsRequired"), AttributeAssignmentOperator, new PrimitiveExpression(FalseConstant)));

            }

            var currentElement = element as IHasAttributes;
            if (currentElement != null) {
                if (currentElement.Attributes.Count > 0) {
                    var VSec = currentElement.Attributes[0].Parent as AttributeSection;
                    var NewVSec = Builder.BuildAttributeSection();
                    foreach (Attribute att in currentElement.Attributes) {
                        if (new[] { "DataContract", "DataMember" }.Contains(att.Name))
                            continue;

                        NewVSec.AddAttribute(att);
                    }
                    NewVSec.AddAttribute(Attribute);
                    var Code = CodeRush.CodeMod.GenerateCode(NewVSec); //var Code = CodeRush.CodeMod.GenerateCode(NewVSec, true);
                    CodeRush.Documents.ActiveTextDocument.QueueReplace(VSec, Code);
                }
                else {
                    BuildNInsert(element, Builder, Attribute);
                }
            }
            else {
                BuildNInsert(element, Builder, Attribute);
            }
        }
        private void AddAttribute(LanguageElement element, string AttributeName, List<object> ConstructorParams = null, Dictionary<string, object> Values = null) {
            //var Builder = new ElementBuilder();
            var Builder = CodeRush.Language.GetElementBuilder(ActiveLanguage); //DevExpress.CodeRush.Common.Constants.Str.Language.CSharp

            var Attribute = Builder.BuildAttribute(AttributeName);

            if (ConstructorParams != null && ConstructorParams.Count > 0) {

                //Builder.BuildArgumentDirectionExpression( )
                //Builder.AddArgument(Attribute, new obj);
                foreach (var obj in ConstructorParams) {
                    var xx = new PrimitiveExpression(obj.ToString());
                    Attribute.AddArgument(xx);
                }
            }

            if (Values != null) {
                foreach (var kvp in Values) {
                    object value;
                    switch (System.Type.GetTypeCode(kvp.Value.GetType())) {
                        case System.TypeCode.String: value = string.Format("\"{0}\"", (string)kvp.Value); break;
                        case System.TypeCode.Boolean: value = ((bool)kvp.Value ? TrueConstant : FalseConstant); break;
                        default: value = kvp.Value; break;
                    }
                    Attribute.Arguments.Add(
                        Builder.BuildAssignmentExpression(
                            kvp.Key + AttributeValueAssignmentOperator, value, AssignmentOperatorType.None));
                }
            }


            var currentElement = element as IHasAttributes;
            if (currentElement != null) {
                if (currentElement.Attributes.Count > 0) {
                    var VSec = currentElement.Attributes[0].Parent as AttributeSection;
                    var NewVSec = Builder.BuildAttributeSection();
                    foreach (Attribute att in currentElement.Attributes) {
                        if (new[] { "DataContract", "DataMember" }.Contains(att.Name))
                            continue;

                        NewVSec.AddAttribute(att);
                    }
                    NewVSec.AddAttribute(Attribute);
                    var Code = CodeRush.CodeMod.GenerateCode(NewVSec); //var Code = CodeRush.CodeMod.GenerateCode(NewVSec, true);
                    CodeRush.Documents.ActiveTextDocument.QueueReplace(VSec, Code);
                }
                else {
                    BuildNInsert(element, Builder, Attribute);
                }
            }
            else {
                BuildNInsert(element, Builder, Attribute);
            }
        }

        private static void BuildNInsert(LanguageElement element, ElementBuilder Builder, Attribute Attribute) {
            var Section = Builder.BuildAttributeSection();
            Section.AddAttribute(Attribute);
            var Code = CodeRush.CodeMod.GenerateCode(Section, false);
            SourcePoint InsertionPoint = new SourcePoint(element.Range.Start.Line, 1);
            CodeRush.Documents.ActiveTextDocument.QueueInsert(InsertionPoint, Code);
        }
        #endregion

        #region Remove Attribute

        private void RemoveDataContract_CheckAvailability(object sender, CheckContentAvailabilityEventArgs ea) {
            // Limit availability to when the caret is within the name of the active class.
            if (CodeRush.Source.ActiveClass == null)
                return; // No active class
            if (!CodeRush.Source.ActiveClass.NameRange.Contains(CodeRush.Caret.SourcePoint))
                return;  // Caret not in class name
            if (!CodeRush.Source.ActiveClass.Attributes.OfType<DevExpress.CodeRush.StructuralParser.Attribute>().Any(att => att.Name == "DataContract"))
                return;  // Class doesn't implement DataContract

            ea.Available = true;
        }
        private void RemoveDataContract_Apply(object sender, ApplyContentEventArgs ea) {
            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
            ConfigureLanguageConstants(ActiveDoc);

            using (ActiveDoc.NewCompoundAction("Remove DataContract")) {

                RemoveAttribute(CodeRush.Source.ActiveClass, "DataContract");
                foreach (Property prop in CodeRush.Source.ActiveClass.AllProperties) {
                    // Add DataMember Attribute
                    RemoveAttribute(prop, "DataMember");
                }
                foreach (IHasAttributes prop in CodeRush.Source.ActiveClass.AllFields) {
                    // Add DataMember Attribute
                    RemoveAttribute(prop, "DataMember");
                }

                CodeRush.Documents.ActiveTextDocument.ApplyQueuedEdits();
                CodeRush.Documents.ActiveTextDocument.ParseIfNeeded();
                CodeRush.Actions.Get("FormatFile").DoExecute();


            }
        }
        private void RemoveAttribute(IHasAttributes element, string param1) {

            var att = element.Attributes.Cast<Attribute>().FirstOrDefault(g => g.Name == param1);
            if (att != null) {
                if (element.Attributes.Count == 1) {
                    var ac = att.Parent as AttributeSection;
                    if (ac != null)
                        CodeRush.Documents.ActiveTextDocument.QueueDelete(ac);
                }
                else {
                    var ac = att.Parent as AttributeSection;
                    if (ac != null) {
                        //ac.RemoveAttribute(att);
                        var Builder = CodeRush.Language.GetElementBuilder(ActiveLanguage);
                        var NCC = ac.AttributeCollection.Cast<Attribute>().Except(new[] { att });

                        var Section = Builder.BuildAttributeSection();
                        foreach (var attL in NCC) Section.AddAttribute(attL);
                        var Code = CodeRush.CodeMod.GenerateCode(Section, true);

                        CodeRush.Documents.ActiveTextDocument.QueueReplace(ac, Code);
                    }


                    //CodeRush.Documents.ActiveTextDocument.QueueDelete(att);
                }



                //att.RemoveAllNodes();
                //att.RemoveFromParent();
            }

            //ActiveDoc
            //activeClass.FindAttribute(param1).RemoveFromParent();
        }

        //private void RemoveAttribute(IHasAttributes prop, string param1) {
        //    prop.FindAttribute(param1).RemoveFromParent();
        //}



        #endregion


        private void AddProtoContract_Apply(object sender, ApplyContentEventArgs ea) {

            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;

            ConfigureLanguageConstants(ActiveDoc);
            LoadProtobufNuget(ActiveDoc);
            AddNamespaceReference("ProtoBuf");


            //AddFullReference("ProtoBuf", "protobuf-net",
            //    "2.0.0.668", "Culture=neutral, PublicKeyToken=257b51d87d2e4d67, processorArchitecture=MSIL",
            //    "..\\packages\\protobuf-net.2.0.0.668\\lib\\net40\\protobuf-net.dll");

            //AddNamespaceReference("ProtoBuf");


            //        <Reference Include="protobuf-net, Version=2.0.0.668, Culture=neutral, PublicKeyToken=257b51d87d2e4d67, processorArchitecture=MSIL">
            //  <SpecificVersion>False</SpecificVersion>
            //  <HintPath>..\packages\protobuf-net.2.0.0.668\lib\net40\protobuf-net.dll</HintPath>
            //</Reference>

            using (ActiveDoc.NewCompoundAction("Add ProtoContract")) {
                // Add Namespace Reference
                AddNamespaceReference("System.Runtime.Serialization");
                CodeRush.Project.AddReference(ActiveDoc.ProjectElement, "protobuf-net");

                AddAttribute(CodeRush.Source.ActiveClass, "ProtoContract", -1);
                int dataOrder = 0;


                foreach (Property prop in CodeRush.Source.ActiveClass.AllProperties) {

                    var Ctrs = new List<object> { ++dataOrder }; // tag = 1,2,3 etc ( not 0 based )
                    var atts = new Dictionary<string, object>(); // { {"tag", dataOrder} };

                    // Add DataMember Attribute
                    AddAttribute(prop, "ProtoMember", Ctrs, atts);

                }

                foreach (IHasAttributes prop in CodeRush.Source.ActiveClass.AllFields) {
                    var Ctrs = new List<object> { ++dataOrder }; // tag = 1,2,3 etc ( not 0 based )
                    var atts = new Dictionary<string, object>(); // { {"tag", dataOrder} };
                    AddAttribute((LanguageElement)prop, "ProtoMember", Ctrs, atts);
                }
                CodeRush.Documents.ActiveTextDocument.ApplyQueuedEdits();
                CodeRush.Documents.ActiveTextDocument.ParseIfNeeded();
                CodeRush.Actions.Get("FormatFile").DoExecute();
                CodeRush.Actions.Get("FormatFile").DoExecute();
            }
        }
        private void LoadProtobufNuget(TextDocument ActiveDoc) {

            //var pbNode = xd.Descendants("package").FirstOrDefault(ndd=>ndd.Name.LocalName == "package" && ndd.Attributes["id"]?.Value == "protobuf-bet");
            //<package id="protobuf-net" version="2.0.0.668" targetFramework="net40" />

            var prj = ActiveDoc.Project;





            var PackConfig = prj.EnumerateProjectItems().FirstOrDefault(itm => itm.Name.Contains("packages.config"));
            if (PackConfig == null) {
                prj.AddFile("packages.config");
                PackConfig = prj.EnumerateProjectItems().FirstOrDefault(itm => itm.Name.Contains("packages.config"));
            }



            XDocument xd = XDocument.Load(PackConfig.FullPath);
            var pbNode = (from nd in xd.Descendants("package")
                          where
                            nd.Name.LocalName == "package" &&
                            nd.HasAttributes && nd.Attributes("id") != null && nd.Attribute("id").Value == "protobuf-net"
                          select nd).FirstOrDefault();

            if (pbNode != null) {
                //Protobuf already isntalled as a nuget package;
                return;
            }



            var fi = new System.IO.FileInfo(prj.FileName);

            string packageID = "protobuf-net";
            //IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
            //var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));

            IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            var installedPackages = installerServices.GetInstalledPackages();

            if (!installerServices.IsPackageInstalled(prj.ProjectObject, packageID)) {

                var Inst = componentModel.GetService<NuGet.VisualStudio.IVsPackageInstaller>();
                if (Inst != null) {
                    string NugetSource = null;
                    Inst.InstallPackage(NugetSource, prj.ProjectObject, "protobuf-net", "2.0.0.668", false);

                    xd = XDocument.Load(PackConfig.FullPath);
                    pbNode = (from nd in xd.Descendants("package")
                              where
                                nd.Name.LocalName == "package" &&
                                nd.HasAttributes && nd.Attributes("id") != null && nd.Attribute("id").Value == "protobuf-net"
                              select nd).FirstOrDefault();
                }

            }

            //List<IPackage> packages = repo.FindPackagesById(packageID).
            //    Where(pk => pk.Version.Version.Revision == 668 && pk.Version.Version.Major == 2).ToList();

            //Initialize the package manager
            //string path = fi.Directory.ToString() + "..\\packages\\"; //..;// <PATH_TO_WHERE_THE_PACKAGES_SHOULD_BE_INSTALLED>
            //PackageManager packageManager = new PackageManager(repo, path);
            ////Download and unzip the package
            //packageManager.InstallPackage(packageID, SemanticVersion.Parse("2.0.0.668"));

            //prj.AddReference("..\\packages\\" + packageID.Replace(" ", ".") + "\\lib\\net40\\protobuf-net.dll");



            //VSProject  theVSProject = (VSProject)(DTE.Solution.Projects[1].Object);
            //VSLangProj110.References


            if (pbNode == null) {
                var NewLine = new XElement("package",
                    new XAttribute("id", "protobuf-net"),
                    new XAttribute("version", "2.0.0.668"),
                    new XAttribute("targetFramework", "net40"));
                xd.Element("packages").Add(NewLine);
                var atts = System.IO.File.GetAttributes(PackConfig.FullPath);
                if (atts.HasFlag(System.IO.FileAttributes.ReadOnly)) {
                    // Not checked out in TFS
                    tryCheckOut(PackConfig.FullPath);

                }
                xd.Save(PackConfig.FullPath);
            }

            //return PackConfig;
        }
        private static void tryCheckOut(string fullPath) {

            try {
                System.IO.FileInfo fi = new System.IO.FileInfo(fullPath);
                WorkspaceInfo wi = Workstation.Current.GetLocalWorkspaceInfo(fi.Directory.ToString());//Environment.CurrentDirectory);
                if (wi == null) {
                    return; // bad luck, chummer.
                }
                Microsoft.TeamFoundation.Client.TfsTeamProjectCollection tpc = new Microsoft.TeamFoundation.Client.TfsTeamProjectCollection(wi.ServerUri);
                var WK = wi.GetWorkspace(tpc);
                WK.PendEdit(fullPath);
            }
            catch (System.Exception) { return; }


            //// Get a reference to Version Control.
            //VersionControlServer versionControl = tpc.GetService<VersionControlServer>();

            //versionControl.pend

            //// Listen for the Source Control events.
            //versionControl.NonFatalError += Example.OnNonFatalError;
            //versionControl.Getting += Example.OnGetting;
            //versionControl.BeforeCheckinPendingChange += Example.OnBeforeCheckinPendingChange;
            //versionControl.NewPendingChange += Example.OnNewPendingChange;
        }
    }
}