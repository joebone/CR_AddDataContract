//using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Serialization;
using DevExpress.CodeRush.Core;
using DevExpress.CodeRush.PlugInCore;
using DevExpress.CodeRush.StructuralParser;

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
            DevExpress.CodeRush.Core.CodeProvider AddDataContract = new DevExpress.CodeRush.Core.CodeProvider(components);
            ((System.ComponentModel.ISupportInitialize)(AddDataContract)).BeginInit();
            AddDataContract.ProviderName = "AddDataContract"; // Should be Unique
            AddDataContract.DisplayName = "Add DataContract";
            AddDataContract.CheckAvailability += AddDataContract_CheckAvailability;
            AddDataContract.Apply += AddDataContract_Apply;
            ((System.ComponentModel.ISupportInitialize)(AddDataContract)).EndInit();

            DevExpress.CodeRush.Core.CodeProvider RemoveDataContract = new DevExpress.CodeRush.Core.CodeProvider(components);
            ((System.ComponentModel.ISupportInitialize)(RemoveDataContract)).BeginInit();
            RemoveDataContract.ProviderName = "RemoveDataContract "; // Should be Unique
            RemoveDataContract.DisplayName = "Remove DataContract";
            RemoveDataContract.CheckAvailability += RemoveDataContract_CheckAvailability;
            RemoveDataContract.Apply += RemoveDataContract_Apply;
            ((System.ComponentModel.ISupportInitialize)(RemoveDataContract)).EndInit();
        }
        private void AddDataContract_CheckAvailability(System.Object sender, CheckContentAvailabilityEventArgs ea) {
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

        private void AddDataContract_Apply(System.Object sender, ApplyContentEventArgs ea) {
            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;

            ConfigureLanguageConstants(ActiveDoc);

            using (ActiveDoc.NewCompoundAction("Add DataContract")) {
                // Add Namespace Reference
                AddNamespaceReference("System.Runtime.Serialization");
                CodeRush.Project.AddReference(ActiveDoc.ProjectElement, "System.Runtime.Serialization");

                AddAttribute(CodeRush.Source.ActiveClass, "DataContract");
                int dataOrder = 0;
                foreach (Property prop in CodeRush.Source.ActiveClass.AllProperties) {
                    // Add DataMember Attribute
                    AddAttribute(prop, "DataMember", ++dataOrder);
                }
                CodeRush.Documents.ActiveTextDocument.ApplyQueuedEdits();
                CodeRush.Documents.ActiveTextDocument.ParseIfNeeded();
                CodeRush.Actions.Get("FormatFile").DoExecute();


            }
        }
        private void AddNamespaceReference(string NamespaceName) {
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
    }
}