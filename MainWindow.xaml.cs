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

            // Add event handlers
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            // Create a folder for the web content
            SetupWebFolder();

            // Navigate to the index.html
            string webPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web", "index.html");
            webView.CoreWebView2.Navigate(new Uri(webPath).AbsoluteUri);

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

            // In a real application, you would copy your built files here
            // or include them in the project with "Copy to Output Directory" set to "Copy if newer"
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