using Rhino.PlugIns;
using Rhino.UI;
using Eto.Forms;
using Eto.Drawing;
using System;
using System.Runtime.InteropServices;

namespace SynapseRCP
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class SynapseRH : PlugIn

    {
        public SynapseRH()
        {
            Instance = this;
        }

        ///<summary>Gets the only instance of the Synapse plug-in.</summary>
        public static SynapseRH Instance
        {
            get; private set;
        }

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            Type paneltype = typeof(SPanel);
            Panels.RegisterPanel(this, paneltype, "Synapse", Properties.Resources.pluginicon);
            return base.OnLoad(ref errorMessage);
        }

        public SPanel RemotePanel { get; set; }
    }



    [Guid("6bea5af8-4529-40b9-a470-6752017adeb6")]
    public class SPanel: Panel
    {
        public SPanel()
        {
            Padding = 2;
        }

        public static Guid PanelId
        {
            get { return typeof(SPanel).GUID; }
        }
    }
}