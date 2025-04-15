﻿#pragma checksum "..\..\..\Windows\GaussWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "2F36484D52EDA5FC0F489C5410D55F5342CD44CAE3D5FC0E67886F65C1304232"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using LinearProgramming.Windows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace LinearProgramming.Windows {
    
    
    /// <summary>
    /// GaussWindow
    /// </summary>
    public partial class GaussWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 11 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button QuitButton;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button FullScreen;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button MinimizeButton;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border ActionsBorder;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button numberSwitchButton;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button autoBasisButton;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border InputBorder;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer InputScroll;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel InputCanvas;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border TaskBorder;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\Windows\GaussWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ProgressBar progressBar;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/LinearProgramming;component/windows/gausswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\GaussWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 8 "..\..\..\Windows\GaussWindow.xaml"
            ((LinearProgramming.Windows.GaussWindow)(target)).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Window_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.QuitButton = ((System.Windows.Controls.Button)(target));
            
            #line 11 "..\..\..\Windows\GaussWindow.xaml"
            this.QuitButton.Click += new System.Windows.RoutedEventHandler(this.QuitButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.FullScreen = ((System.Windows.Controls.Button)(target));
            
            #line 12 "..\..\..\Windows\GaussWindow.xaml"
            this.FullScreen.Click += new System.Windows.RoutedEventHandler(this.FullScreenButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.MinimizeButton = ((System.Windows.Controls.Button)(target));
            
            #line 13 "..\..\..\Windows\GaussWindow.xaml"
            this.MinimizeButton.Click += new System.Windows.RoutedEventHandler(this.MinimizeButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ActionsBorder = ((System.Windows.Controls.Border)(target));
            return;
            case 6:
            this.numberSwitchButton = ((System.Windows.Controls.Button)(target));
            
            #line 16 "..\..\..\Windows\GaussWindow.xaml"
            this.numberSwitchButton.Click += new System.Windows.RoutedEventHandler(this.numberSwitchButton_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.autoBasisButton = ((System.Windows.Controls.Button)(target));
            
            #line 17 "..\..\..\Windows\GaussWindow.xaml"
            this.autoBasisButton.Click += new System.Windows.RoutedEventHandler(this.autoBasisButton_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.InputBorder = ((System.Windows.Controls.Border)(target));
            return;
            case 9:
            this.InputScroll = ((System.Windows.Controls.ScrollViewer)(target));
            
            #line 25 "..\..\..\Windows\GaussWindow.xaml"
            this.InputScroll.SizeChanged += new System.Windows.SizeChangedEventHandler(this.InputScroll_SizeChanged);
            
            #line default
            #line hidden
            return;
            case 10:
            this.InputCanvas = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 11:
            this.TaskBorder = ((System.Windows.Controls.Border)(target));
            return;
            case 12:
            this.progressBar = ((System.Windows.Controls.ProgressBar)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

