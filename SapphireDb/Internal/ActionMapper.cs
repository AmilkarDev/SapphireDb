﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SapphireDb.Actions;
using SapphireDb.Command.Execute;
using SapphireDb.Helper;

namespace SapphireDb.Internal
{
    class ActionMapper
    {
        public readonly Dictionary<string, Type> actionHandlerTypes;

        public ActionMapper()
        {
            actionHandlerTypes = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => typeof(ActionHandlerBase).IsAssignableFrom(t) && t.Name.EndsWith("Actions"))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Actions", StringComparison.Ordinal)).ToCamelCase(), t => t);
        }

        public Type GetHandler(ExecuteCommand executeCommand)
        {
            return actionHandlerTypes[executeCommand.ActionHandlerName.ToCamelCase()];
        }

        public MethodInfo GetAction(ExecuteCommand executeCommand, Type actionHandlerType)
        {
            return actionHandlerType.GetMethods().FirstOrDefault(m => m.Name.ToCamelCase() == executeCommand.ActionName.ToCamelCase());
        }
    }
}
