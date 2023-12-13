using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using System;
using SuchByte.MacroDeck.Logging;
using Newtonsoft.Json.Linq;

namespace hashashin.FxCommands
{
    public partial class FxCommandsActionConfWnd : ActionConfigControl
    {
        PluginAction pluginAction;

        public FxCommandsActionConfWnd(PluginAction pluginAction)
        {
            this.pluginAction = pluginAction;

            InitializeComponent();
            this.LoadConfig();
        }

        public override bool OnActionSave()
        {
            if (String.IsNullOrWhiteSpace(this.TextCommand.Text))
            {
                MacroDeckLogger.Error(Main.Instance, "command is empty: create action failed");
                return false;
            }


            JObject configurationObject = JObject.FromObject(new
            {
                command = this.TextCommand.Text,
            });
            this.pluginAction.Configuration = configurationObject.ToString();
            this.pluginAction.ConfigurationSummary = this.TextCommand.Text;

            return true;
        }



        private void LoadConfig()
        {
            if (string.IsNullOrWhiteSpace(this.pluginAction.Configuration))
            {
                return;
            }
            try
            {
                JObject configurationObject = JObject.Parse(this.pluginAction.Configuration);
                this.TextCommand.Text = configurationObject["command"].ToString();
            }
            catch (Exception ex)
            {
                MacroDeckLogger.Error(Main.Instance, $"hashashin.FxCommands LoadConfig {ex.Message}");
            }

        }

    }
}