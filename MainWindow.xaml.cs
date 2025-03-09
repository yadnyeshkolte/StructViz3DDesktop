using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace StructViz3D
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            // Initialize the WebView2 control
            await webView.EnsureCoreWebView2Async(null);

            // Set up WebView2 settings
            webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true; // Set to false for production

            // Add security settings to allow file access
            webView.CoreWebView2.Settings.IsWebMessageEnabled = true;

            // Allow local file access without CORS restrictions
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                "window.addEventListener('DOMContentLoaded', () => { document.body.style.margin = '0'; });");

            // Add virtual host mapping for the application resources
            string webFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web");
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "structviz3d.local", // Virtual host name
                webFolderPath,       // Physical folder path
                CoreWebView2HostResourceAccessKind.Allow);

            // Add event handlers
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            // Create a folder for the web content
            SetupWebFolder();

            // Navigate using the virtual host instead of file:// protocol
            webView.CoreWebView2.Navigate("https://structviz3d.local/index.html");

            // Expose C# methods to JavaScript
            ExposeHostObjectToWebView();
        }

        private void SetupWebFolder()
        {
            // Create the web folder if it doesn't exist
            string webFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web");
            if (!Directory.Exists(webFolderPath))
            {
                Directory.CreateDirectory(webFolderPath);
            }

            // Create assets folder
            string assetsPath = Path.Combine(webFolderPath, "assets");
            if (!Directory.Exists(assetsPath))
            {
                Directory.CreateDirectory(assetsPath);
            }

            // Copy the required files from your project to the web folder
            // In a real application, you would ensure these files exist in your project
            // and are set to "Copy to Output Directory" as "Copy if newer"

            // For development, you might want to copy files from a known location
            // For example:
            /*
            string sourcePath = @"C:\Your\React\Build\Path";
            if (Directory.Exists(sourcePath))
            {
                // Copy all files from the build directory to web folder
                CopyDirectory(sourcePath, webFolderPath);
            }
            */
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Create destination directory if it doesn't exist
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // Copy files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // Process subdirectories
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }

        private void ExposeHostObjectToWebView()
        {
            // Add a script to create a global object that can call into C#
            webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                window.csharpBridge = {
                    openFile: () => {
                        chrome.webview.postMessage({ action: 'openFile' });
                        // We'll get the result back through a message
                        return new Promise((resolve) => {
                            window.resolveOpenFile = resolve;
                        });
                    },
                    saveFile: (content, fileName) => {
                        chrome.webview.postMessage({ 
                            action: 'saveFile', 
                            content: content, 
                            fileName: fileName 
                        });
                    }
                };
            ");
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // Parse the message from the web app
                string jsonMessage = e.WebMessageAsJson;
                dynamic message = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonMessage);
                string action = message.action;

                switch (action)
                {
                    case "openFile":
                        HandleOpenFile();
                        break;
                    case "saveFile":
                        HandleSaveFile(message.content.ToString(), message.fileName.ToString());
                        break;
                        // Add more actions as needed
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing web message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleOpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "3D Models (*.stl;*.obj)|*.stl;*.obj|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileContent = "";

                // For binary files like STL, you might need to convert them to a format your web app can use
                // This is a simplified example
                try
                {
                    // For STL files, we might want to read as binary data and convert to base64
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    fileContent = Convert.ToBase64String(fileBytes);

                    // Send the file data back to the web app
                    string fileName = Path.GetFileName(filePath);
                    string fileExt = Path.GetExtension(filePath).ToLower();

                    string jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        fileName = fileName,
                        fileType = fileExt,
                        content = fileContent
                    });

                    // Send the response to JavaScript
                    webView.CoreWebView2.ExecuteScriptAsync($"window.resolveOpenFile({jsonResponse})");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    webView.CoreWebView2.ExecuteScriptAsync("window.resolveOpenFile(null)");
                }
            }
            else
            {
                // User canceled the dialog
                webView.CoreWebView2.ExecuteScriptAsync("window.resolveOpenFile(null)");
            }
        }

        private void HandleSaveFile(string content, string suggestedFileName)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = suggestedFileName;
            saveFileDialog.Filter = "All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Depending on what you're saving, you might need to decode from base64
                    if (IsBase64String(content))
                    {
                        byte[] bytes = Convert.FromBase64String(content);
                        File.WriteAllBytes(saveFileDialog.FileName, bytes);
                    }
                    else
                    {
                        File.WriteAllText(saveFileDialog.FileName, content);
                    }

                    MessageBox.Show("File saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool IsBase64String(string s)
        {
            // Simple check if the string might be base64 encoded
            if (string.IsNullOrEmpty(s))
                return false;

            try
            {
                Convert.FromBase64String(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}