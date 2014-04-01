using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Serialization;
using DevExpress.CodeRush.Core;
using DevExpress.CodeRush.PlugInCore;
using DevExpress.CodeRush.StructuralParser;

namespace CR_AddDataContract
{
    public partial class PlugIn1 : StandardPlugIn
    {
        // DXCore-generated code...
        #region InitializePlugIn
        public override void InitializePlugIn()
        {
            base.InitializePlugIn();
            registerAddDataContract();
        }
        #endregion
        #region FinalizePlugIn
        public override void FinalizePlugIn()
        {
            //
            // TODO: Add your finalization code here.
            //

            base.FinalizePlugIn();
        }
        #endregion
        public void registerAddDataContract()
        {
            DevExpress.CodeRush.Core.CodeProvider AddDataContract = new DevExpress.CodeRush.Core.CodeProvider(components);
            ((System.ComponentModel.ISupportInitialize)(AddDataContract)).BeginInit();
            AddDataContract.ProviderName = "AddDataContract"; // Should be Unique
            AddDataContract.DisplayName = "Add DataContract";
            AddDataContract.CheckAvailability += AddDataContract_CheckAvailability;
            AddDataContract.Apply += AddDataContract_Apply;
            ((System.ComponentModel.ISupportInitialize)(AddDataContract)).EndInit();
        }
        private void AddDataContract_CheckAvailability(Object sender, CheckContentAvailabilityEventArgs ea)
        {
            // Limit availability to when the caret is within the name of the active class.
            if (CodeRush.Source.ActiveClass == null)
                return; // No active class
            if (!CodeRush.Source.ActiveClass.NameRange.Contains(CodeRush.Caret.SourcePoint))
                return;  // Caret not in class name
            ea.Available = true;
        }

        private void AddDataContract_Apply(Object sender, ApplyContentEventArgs ea)
        {
            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;

            using (ActiveDoc.NewCompoundAction("Add DataContract"))
            {
                // Add Namespace Reference
                AddNamespaceReference("System.Runtime.Serialization");
                CodeRush.Project.AddReference(ActiveDoc.ProjectElement, "System.Runtime.Serialization");

                // Add DataContract Attribute
                AddAttribute(CodeRush.Source.ActiveClass, "DataContract");
                foreach (Property prop in CodeRush.Source.ActiveClass.AllProperties)
                {
                    // Add DataMember Attribute
                    AddAttribute(prop, "DataMember");
                }
                CodeRush.Documents.ActiveTextDocument.ApplyQueuedEdits();
                CodeRush.Documents.ActiveTextDocument.ParseIfNeeded();
                CodeRush.Actions.Get("FormatFile").DoExecute();
            }
        }
        private void AddNamespaceReference(string NamespaceName)
        {
            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
            var finder = new ElementEnumerable(ActiveDoc.FileNode, LanguageElementType.NamespaceReference, true);
            var NamespaceReferences = finder.OfType<NamespaceReference>();
            if (NamespaceReferences.Any(ns => ns.Name == NamespaceName))
                return;

            // Calculate Insert Location
            SourcePoint InsertionPoint;
            if (ActiveDoc.NamespaceReferences.Count <= 0)
            {
                InsertionPoint = ActiveDoc.Range.Start;
            }
            else
            {
                InsertionPoint = NamespaceReferences.Last().Range.Start;
            }

            // Generate new NamespaceReference
            var Code = CodeRush.CodeMod.GenerateCode(new NamespaceReference(NamespaceName));
            ActiveDoc.QueueInsert(InsertionPoint, Code);
        }
        private void AddAttribute(LanguageElement element, string AttributeName)
        {
            var Builder = new ElementBuilder();
            var Attribute = Builder.BuildAttribute(AttributeName);
            var Section = Builder.BuildAttributeSection();
            Section.AddAttribute(Attribute);
            var Code = CodeRush.CodeMod.GenerateCode(Section, false);
            SourcePoint InsertionPoint = new SourcePoint(element.Range.Start.Line, 1);
            CodeRush.Documents.ActiveTextDocument.QueueInsert(InsertionPoint, Code);
        }


    }
}