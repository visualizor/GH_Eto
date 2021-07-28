using System;
using Rhino;
using Rhino.Commands;

namespace SynapseRCP
{
    public class SCleanse : Command
    {
        public SCleanse()
        {
            Instance = this;
        }

        ///<summary>The only instance of the SynapseCleanse command.</summary>
        public static SCleanse Instance
        {
            get; private set;
        }

        public override string EnglishName
        {
            get { return "SynapseCleanse"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            if (SynapseRH.Instance.RemotePanel != null)
                SynapseRH.Instance.RemotePanel.Content = null;
            return Result.Success;
        }
    }
}