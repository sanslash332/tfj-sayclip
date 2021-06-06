using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using sayclip;

namespace sayclipTray
{
    /// <summary>
    /// Interaction logic for PluginsTab.xaml
    /// </summary>
    public partial class PluginsTab : TabItem
    {
        public DelegateCommand setActiveSelectedPluginCommand;
        public DelegateCommand showSelectedPluginConfigWindowCommand;
        PluginManager pluginManager;
        List<iSayclipPluginTranslator> plugins;

        public PluginsTab()
        {
            InitializeComponent();
            this.pluginManager = PluginManager.getInstanse;
            this.plugins = this.pluginManager.getPlugins;
            buildCommands();
            buildPluginsListbox();
            
        }

        private void buildCommands()
        {
            setActiveSelectedPluginCommand = new DelegateCommand()
            {
                CanExecuteFunc = () =>
                {
                    if (pluginsListbox.SelectedIndex < 0)
                    {
                        return (false);
                    }
                    iSayclipPluginTranslator selectedPlugin = plugins[pluginsListbox.SelectedIndex];
                    return (selectedPlugin.getName() != pluginManager.getActivePlugin.getName());

                },
                CommandAction = () =>
                {
                    iSayclipPluginTranslator selectedPlugin = plugins[pluginsListbox.SelectedIndex];
                    if(!selectedPlugin.initialize())
                    {
                        ScreenReaderControl.speech(String.Format(App.dictlang["menu.plugin.changeError"].ToString(), selectedPlugin.getName()), true);
                        return;
                    }
                    pluginManager.setActivePlugin(selectedPlugin.getName());
                    ((App)App.Current).resetSayclip();
                    App.getNotifyIcon.buildPluginsMenu();
                    App.getNotifyIcon.buildLanguajeMenuItems();
                    App.getNotifyIcon.buidlLanguajeMenuHeaders();

                    
                }
            };
            showSelectedPluginConfigWindowCommand = new DelegateCommand()
            {
                CanExecuteFunc = () =>
                {
                    if (pluginsListbox.SelectedIndex < 0)
                    {
                        return (false);
                    }
                    iSayclipPluginTranslator selectedPlugin = plugins[pluginsListbox.SelectedIndex];
                    return (selectedPlugin.haveConfigWindow());
                },
                CommandAction = () =>
                {
                    iSayclipPluginTranslator selectedPlugin = plugins[pluginsListbox.SelectedIndex];
                    selectedPlugin.showConfigWindow(App.uiLang);
                }
            };
            setSelectedPluginActiveButton.Command = setActiveSelectedPluginCommand;
            showSelectedPluginConfigWindowButton.Command = showSelectedPluginConfigWindowCommand;
        }

        public void buildPluginsListbox()
        {
            List<String> pluginsNames = pluginManager.getPluginsNames();
            pluginsListbox.Items.Clear();
            pluginsNames.ForEach((string x ) =>
            {
                ListBoxItem pluginItem = new ListBoxItem();
                pluginItem.Content = pluginManager.getActivePlugin.getName() == x ? $"{App.dictlang["current"].ToString()} {x}" : x;
                pluginsListbox.Items.Add(pluginItem);
            });
            pluginsListbox.Items.Refresh();
        }

    private void pluginsListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pluginsDescriptionTextBox.Text = pluginsListbox.SelectedIndex >= 0 ?
                plugins[pluginsListbox.SelectedIndex].getDescription(App.uiLang)
                : "";

        }
    }
}
