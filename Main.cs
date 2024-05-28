using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.Plugins;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace hashashin.FxCommands
{
    public class Main : MacroDeckPlugin
    {
        public static MacroDeckPlugin Instance { get; internal set; }

        // Optional; If your plugin can be configured, set to "true". It'll make the "Configure" button appear in the package manager.
        public override bool CanConfigure => false;

        // Gets called when the plugin is loaded
        public override void Enable()
        {
            this.Actions = new List<PluginAction>{
            // add the instances of your actions here
            new FxCommands(),
            };
        }

        public class FxCommands : PluginAction
        {
            private ConnectionManager connectionManager;
            // The name of the action
            public override string Name => "FxCommand";

            // A short description what the action can do
            public override string Description => "Envía un comando a la consola del cliente de Fivem, no añadir el / al principio";

            // Optional; Add if this action can be configured. This will make the ActionConfigurator calling GetActionConfigurator();
            public override bool CanConfigure => true;
            // Optional; Add if you added CanConfigure; Gets called when the action can be configured and the action got selected in the ActionSelector. You need to return a user control with the "ActionConfigControl" class as base class
            public override ActionConfigControl GetActionConfigControl(ActionConfigurator actionConfigurator)
            {
                return new FxCommandsActionConfWnd(this);
            }

            // Gets called when the action is triggered by a button press or an event
            public override void Trigger(string clientId, ActionButton actionButton)
            {
                JObject configurationObject = JObject.Parse(this.Configuration);
                var command = configurationObject["command"].ToString();
                connectionManager = new ConnectionManager();
                connectionManager.InitializeClients();
                connectionManager.SendMessage(command);
                connectionManager.Dispose();
            }
        }
    }
}
