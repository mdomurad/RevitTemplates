﻿using System;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Nice3point.Revit.AddIn.Commands;
using Nice3point.Revit.AddIn.RevitUtils;

namespace Nice3point.Revit.AddIn
{
    public class Application : IExternalApplication
    {
        private const string ButtonImageUrl = "pack://application:,,,/Nice3point.Revit.AddIn;component/Resources/Icons/RibbonIcon16.png";
        private const string ButtonLargeImageUrl = "pack://application:,,,/Nice3point.Revit.AddIn;component/Resources/Icons/RibbonIcon32.png";

        public Result OnStartup(UIControlledApplication application)
        {
            var panel = RibbonUtils.CreateRibbonPanel(application, "Panel name", "Nice3point.Revit.AddIn");/*caret*/
            
            var showButton = panel.AddPushButton(typeof(Command), nameof(Command), "Button text");
            showButton.ToolTip    = "Tooltip";
            showButton.Image      = new BitmapImage(new Uri(ButtonImageUrl));
            showButton.LargeImage = new BitmapImage(new Uri(ButtonLargeImageUrl));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}