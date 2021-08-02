using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.UI;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace SynapseRCP
{
    public class SLaunch : Command
    {
        public SLaunch()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static SLaunch Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "SynapsePanel"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Guid pid = SPanel.PanelId;
            bool open = Panels.IsPanelVisible(pid);
            if (open) Panels.ClosePanel(pid);
            else Panels.OpenPanel(pid);


            return Result.Success;
        }
    }
}
