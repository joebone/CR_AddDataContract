//using System.ComponentModel;
using DevExpress.CodeRush.Core;
using DevExpress.CodeRush.PlugInCore;
using DevExpress.CodeRush.StructuralParser;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.ComponentModelHost;
using NuGet.VisualStudio;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
                    if (!isContractable(prop)) continue;
                    AddAttribute(prop, "DataMember", ++dataOrder);
                }

                foreach (LanguageElement prop in CodeRush.Source.ActiveClass.AllFields) {
                    // Add DataMember Attribute
                    if (!isContractable(prop)) continue;
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
                        if (AttributeName == att.Name)
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
        private void AddAttribute(LanguageElement element, string AttributeName, List<object> ConstructorParams = null, Dictionary<string, object> AttributeArguments = null) {
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

            if (AttributeArguments != null) {
                foreach (var kvp in AttributeArguments) {
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
                        if (AttributeName == att.Name)
                            continue; //if (new[] { AttributeName }.Contains(att.Name))

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


        private bool isContractable(LanguageElement prop) {
            //throw new System.NotImplementedException();
            var X = prop as Property;
            if (X != null) {
                if (X.IsStatic || X.IsReadOnly || X.IsConst) return false;
                if (!IsSimpleProperty(X)) return false;
            }

            var Y = prop as Member;
            if (Y != null) {
                if (Y.IsStatic || Y.IsReadOnly || Y.IsConst) return false;
            }

            return true;
        }
        private bool IsSimpleProperty(Property activeProperty) {
            if (activeProperty == null)
                return false;

            if (activeProperty.IsAutoImplemented)
                return true;

            Set setter = activeProperty.Setter;
            Get getter = activeProperty.Getter;

            if (setter == null || setter.NodeCount != 1)
                return false;
            if (getter == null || getter.NodeCount != 1)
                return false;

            LanguageElement setterNode = (LanguageElement)setter.Nodes[0];
            LanguageElement getterNode = (LanguageElement)getter.Nodes[0];

            return setterNode is Assignment && getterNode is Return;
        }


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

                Class ActiveClass = CodeRush.Source.ActiveClass;
                AddAttribute(ActiveClass, "ProtoContract", -1);
                int dataOrder = 0;

                if (!ActiveClass.HasDefaultConstructor) {

                    AddDefaultConstructor(ActiveClass);
                }
                foreach (Property prop in ActiveClass.AllProperties) {
                    if (!isContractable(prop)) continue;

                    var Ctrs = new List<object> { ++dataOrder }; // tag = 1,2,3 etc ( not 0 based )
                    var atts = new Dictionary<string, object>(); // { {"tag", dataOrder} };

                    // Add DataMember Attribute
                    AddAttribute(prop, "ProtoMember", Ctrs, atts);

                }

                foreach (Member Field in ActiveClass.AllFields) {
                    if (!isContractable(Field)) continue;

                    var Ctrs = new List<object> { ++dataOrder }; // tag = 1,2,3 etc ( not 0 based )
                    var atts = new Dictionary<string, object>(); // { {"tag", dataOrder} };
                    AddAttribute((LanguageElement)Field, "ProtoMember", Ctrs, atts);
                }
                CodeRush.Documents.ActiveTextDocument.ApplyQueuedEdits();
                CodeRush.Documents.ActiveTextDocument.ParseIfNeeded();
                CodeRush.Actions.Get("FormatFile").DoExecute();
                CodeRush.Actions.Get("FormatFile").DoExecute();
            }
        }
        private void AddDefaultConstructor(Class activeClass) {
            var Builder = CodeRush.Language.GetElementBuilder(ActiveLanguage); //DevExpress.CodeRush.Common.Constants.Str.Language.CSharp
            //var comment = Builder.BuildComment("Empty constructor added por serialization", CommentType.SingleLine);
            //var comment = Builder.BuildXmlDocComment("");
            //var sub = Builder.AddXmlDocComment(comment, "<summary>Empty constructor added por serialization</summary>");

            var comment = Builder.BuildXmlDocComment("<summary>Empty constructor added for serialization</summary>");
            
            var ccCon = Builder.BuildConstructor("public " + activeClass.Name);
            var acces = Builder.BuildAccessSpecifiers(false, false, false, false);
            
            //ccCon.AddDetailNode();
            ccCon.AddCommentNode(comment);


            var Code = CodeRush.CodeMod.GenerateCode(ccCon, false);

            var line = activeClass.Range.Start.Line + 2;
            var col = 1;

            foreach (Method allMethod in activeClass.AllMethods)
            {
            	if(allMethod.MethodType == MethodTypeEnum.Constructor)
                {
                     line = allMethod.Range.Start.Line;
                     col = allMethod.Range.Start.Offset;
                }
            }

            SourcePoint InsertionPoint = new SourcePoint(line, col);
            CodeRush.Documents.ActiveTextDocument.QueueInsert(InsertionPoint, Code);
            //activeClass.buil
        }
        private void LoadProtobufNuget(TextDocument ActiveDoc) {

            //var pbNode = xd.Descendants("package").FirstOrDefault(ndd=>ndd.Name.LocalName == "package" && ndd.Attributes["id"]?.Value == "protobuf-bet");
            //<package id="protobuf-net" version="2.0.0.668" targetFramework="net40" />

            var prj = ActiveDoc.Project;


            XDocument xd = null;
            XElement pbNode = null;

            var PackConfig = prj.EnumerateProjectItems().FirstOrDefault(itm => itm.Name.Contains("packages.config"));
            if (PackConfig != null) {
                //prj.AddFile("packages.config");
                //PackConfig = prj.EnumerateProjectItems().FirstOrDefault(itm => itm.Name.Contains("packages.config"));
                xd = XDocument.Load(PackConfig.FullPath);
                pbNode = (from nd in xd.Descendants("package")
                          where
                            nd.Name.LocalName == "package" &&
                            nd.HasAttributes && nd.Attributes("id") != null && nd.Attribute("id").Value == "protobuf-net"
                          select nd).FirstOrDefault();

                if (pbNode != null) {
                    //Protobuf already isntalled as a nuget package;
                    return;
                }
            }



            var pBar = CodeRush.Progress.GetProgressVisualizer(VisualizerType.WithProgressBar);
            pBar.ProcessingStarted(6);
            pBar.Description = "Loading Protobuf-net Nuget Package";

            pBar.Visible = true;
            pBar.Activate();



            string packageID = "protobuf-net";
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            pBar.ItemProcessed("ComponentModel Loaded", 1, 5);

            IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>(); //var installedPackages = installerServices.GetInstalledPackages();
            pBar.ItemProcessed("IVsPackageInstallerServices initialized", 2, 4);
            if (!installerServices.IsPackageInstalled(prj.ProjectObject, packageID)) {

                pBar.ItemProcessed("Package Not installed", 3, 3);
                var Inst = componentModel.GetService<NuGet.VisualStudio.IVsPackageInstaller>();
                pBar.ItemProcessed("IVsPackageInstaller initialized", 4, 2);
                if (Inst != null) {
                    string NugetSource = null;
                    Inst.InstallPackage(NugetSource, prj.ProjectObject, "protobuf-net", "2.0.0.668", false);
                    pBar.ItemProcessed("protobuf-net package 2.0.0.668 installed", 5, 1);
                    xd = XDocument.Load(PackConfig.FullPath);
                    pbNode = (from nd in xd.Descendants("package")
                              where
                                nd.Name.LocalName == "package" &&
                                nd.HasAttributes && nd.Attributes("id") != null && nd.Attribute("id").Value == "protobuf-net"
                              select nd).FirstOrDefault();
                    pBar.ItemProcessed("packagesConfig checked", 6, 1);
                }
            }
            pBar.ProcessingFinished();
            return;

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


            //if (pbNode == null) {
            //    var NewLine = new XElement("package",
            //        new XAttribute("id", "protobuf-net"),
            //        new XAttribute("version", "2.0.0.668"),
            //        new XAttribute("targetFramework", "net40"));
            //    xd.Element("packages").Add(NewLine);
            //    var atts = System.IO.File.GetAttributes(PackConfig.FullPath);
            //    if (atts.HasFlag(System.IO.FileAttributes.ReadOnly)) {
            //        // Not checked out in TFS
            //        tryCheckOut(PackConfig.FullPath);

            //    }
            //    xd.Save(PackConfig.FullPath);
            //}

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