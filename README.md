# StructViz3D

[Watch this video on YouTube](https://youtu.be/8gEaS0dXfAg)

## Overview

StructViz3D is a desktop application engineered for 3D structure visualization, built on the Microsoft .NET 8.0 platform using WPF (Windows Presentation Foundation). The application employs a hybrid architecture that combines native C# WPF capabilities with web-based rendering technologies through Microsoft's WebView2 control.

The application is designed to:

- Provide an intuitive interface for visualizing 3D models
- Support common 3D file format (.stl)
- Leverage the performance benefits of native code with the rendering capabilities of web technologies
- Provide a responsive and modern user experience with a native desktop application feel

## Architecture

### High-Level Architecture

StructViz3D follows a hybrid architecture pattern:

```
┌─────────────────────────────────────────────────────────────┐
│                    WPF Application Host                     │
│                                                             │
│  ┌─────────────┐     ┌───────────────────────────────────┐  │
│  │             │     │        WebView2 Container         │  │
│  │   Native    │     │  ┌───────────────────────────┐    │  │
│  │  WPF UI     │◄───►│  │      Web Frontend         │    │  │
│  │ Components  │     │  │  (HTML/CSS/JavaScript)    │    │  │
│  │             │     │  └───────────────────────────┘    │  │
│  └─────────────┘     └───────────────────────────────────┘  │
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐│
│  │              C# Backend Services                        ││
│  │  - File System Access                                   ││
│  │  - Native Dialogs                                       ││
│  │  - Application Lifecycle Management                     ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

### Architectural Patterns

1. **Model-View-Controller (MVC)**
    
    - Models: Data structures representing 3D models and application state
    - Views: WPF UI and web-based visualization components
    - Controllers: C# backend services and JavaScript application logic
2. **Bridge Pattern**
    
    - The WebView2 component serves as a bridge between native C# code and web technologies
    - Enables bidirectional communication while maintaining separation of concerns
3. **Composition Pattern**
    
    - The application is composed of distinct, modular components (WPF shell, WebView2 container, web frontend)
    - Each component handles specific responsibilities and communicates through well-defined interfaces

## Application Structure

### Directory Structure

```
StructViz3D/
├── Assets/
│   ├── error_icon.png
│   ├── logo.ico
│   └── logo.png
├── web/
│   ├── assets/
│   │   ├── backgroundimage-T8WR3IGg.png
│   │   ├── index-3BC41zVh.css
│   │   ├── index-BQOHRQP8.js
│   │   └── index-BroobejL.js
│   ├── index.html
│   └── vite.svg
├── App.xaml
├── App.xaml.cs
├── AssemblyInfo.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── SplashScreen.xaml
├── SplashScreen.xaml.cs
├── StructViz3D.csproj
└── StructViz3D.sln
```

### Project Files

1. **Solution Files**
    
    - `StructViz3D.sln`: Visual Studio solution file containing project references and build configurations
    - `StructViz3D.csproj`: .NET project file defining build settings, dependencies, and resource inclusions
2. **Application Files**
    
    - `App.xaml/App.xaml.cs`: Application definition and startup logic
    - `AssemblyInfo.cs`: Assembly metadata and configuration
3. **UI Files**
    
    - `MainWindow.xaml/MainWindow.xaml.cs`: Main application window definition and code-behind
    - `SplashScreen.xaml/SplashScreen.xaml.cs`: Splash screen definition and code-behind
4. **Assets**
    
    - Application icons, images, and other static resources
5. **Web Frontend**
    
    - HTML, CSS, and JavaScript files for the web-based visualization interface

## Core Components

### WPF Application Host

The WPF application provides:

- Native window chrome and system integration
- Application lifecycle management
- File system access and security
- Native dialog presentation

Key classes:

- `App`: Application entry point and initialization
- `MainWindow`: Primary application window and WebView2 container
- `SplashScreen`: Initial loading screen

### WebView2 Integration

The Microsoft WebView2 control:

- Embeds a Chromium-based browser within the WPF application
- Provides modern web standards support
- Enables bidirectional communication between C# and JavaScript
- Securely isolates web content while allowing controlled access to native capabilities

### Web Frontend

The web-based visualization component:

- Renders 3D models using WebGL or similar technologies
- Provides interactive 3D manipulation capabilities
- Communicates with the C# backend through a JavaScript bridge

## Application Lifecycle

### Startup Sequence

1. **Application Initialization**
    
    ```csharp
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Start with null main window
        this.MainWindow = null;
        
        // Show splash screen first
        var splashScreen = new SplashScreen();
        splashScreen.Show();
    }
    ```
    
2. **Splash Screen Display**
    
    - Shows application branding and loading indicators
    - Uses a DispatcherTimer to delay for 3 seconds
    - Creates and shows the main window when the timer elapses
3. **Main Window Initialization**
    
    - Initializes the WebView2 control asynchronously
    - Configures WebView2 settings
    - Sets up virtual host mapping for local content
    - Navigates to the web frontend entry point
4. **Web Frontend Loading**
    
    - Loads HTML/CSS/JavaScript content
    - Initializes the JavaScript bridge to C#
    - Prepares the 3D visualization environment

### Shutdown Process

- Proper disposal of WebView2 resources
- Cleanup of temporary files if needed
- Handling of application exit events

## WebView2 Integration

### Initialization

```csharp
private async void InitializeWebView()
{
    // Initialize the WebView2 control
    await webView.EnsureCoreWebView2Async(null);
    
    // Configure WebView2 settings
    webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
    webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
    webView.CoreWebView2.Settings.AreDevToolsEnabled = true; // Development only
    
    // Set up virtual host mapping
    string webFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web");
    webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
        "structviz3d.local", 
        webFolderPath,
        CoreWebView2HostResourceAccessKind.Allow);
        
    // Add event handlers
    webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
    
    // Navigate to web content
    webView.CoreWebView2.Navigate("https://structviz3d.local/index.html");
    
    // Set up JavaScript bridge
    ExposeHostObjectToWebView();
}
```

### Virtual Host Mapping

To avoid CORS restrictions and provide secure access to local resources, the application creates a virtual host:

```csharp
webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
    "structviz3d.local", // Virtual host name
    webFolderPath,       // Physical folder path
    CoreWebView2HostResourceAccessKind.Allow);
```

This enables the web content to be loaded from a virtual HTTPS origin instead of using the file:// protocol.

### JavaScript Injection

The application injects JavaScript code to establish the communication bridge:

```csharp
webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
    window.csharpBridge = {
        openFile: () => {
            chrome.webview.postMessage({ action: 'openFile' });
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
```

## Communication Protocol

### Message Format

Communication between C# and JavaScript uses JSON messages with a standard format:

```json
{
  "action": "actionName",
  "content": "data content or base64 encoded binary",
  "fileName": "optional file name",
  "fileType": "optional file extension or MIME type"
}
```

### C# to JavaScript Communication

```csharp
// Example: Sending file data to JavaScript
string jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(new
{
    fileName = fileName,
    fileType = fileExt,
    content = fileContent
});

// Execute JavaScript with the data
webView.CoreWebView2.ExecuteScriptAsync($"window.resolveOpenFile({jsonResponse})");
```

### JavaScript to C# Communication

```javascript
// Example: Requesting file open from JavaScript
async function openModelFile() {
    try {
        const fileData = await window.csharpBridge.openFile();
        if (fileData) {
            // Process the file data
            loadModel(fileData.content, fileData.fileType);
        }
    } catch (error) {
        console.error("Error opening file:", error);
    }
}
```

### Message Handling in C#

```csharp
private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
{
    try {
        // Parse the message from the web app
        string jsonMessage = e.WebMessageAsJson;
        dynamic message = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonMessage);
        string action = message.action;

        switch (action) {
            case "openFile":
                HandleOpenFile();
                break;
            case "saveFile":
                HandleSaveFile(message.content.ToString(), message.fileName.ToString());
                break;
            // Additional actions...
        }
    }
    catch (Exception ex) {
        MessageBox.Show($"Error processing web message: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

## File Operations

### File Opening

1. **JavaScript Request**
    
    ```javascript
    const fileData = await window.csharpBridge.openFile();
    ```
    
2. **C# Handler**
    
    ```csharp
    private void HandleOpenFile()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "3D Models (*.stl;*.obj)|*.stl;*.obj|All files (*.*)|*.*";
    
        if (openFileDialog.ShowDialog() == true)
        {
            string filePath = openFileDialog.FileName;
            
            // Read file content
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string fileContent = Convert.ToBase64String(fileBytes);
            
            // Send data back to JavaScript
            string fileName = Path.GetFileName(filePath);
            string fileExt = Path.GetExtension(filePath).ToLower();
            
            string jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(new {
                fileName = fileName,
                fileType = fileExt,
                content = fileContent
            });
            
            webView.CoreWebView2.ExecuteScriptAsync($"window.resolveOpenFile({jsonResponse})");
        }
        else
        {
            // User canceled
            webView.CoreWebView2.ExecuteScriptAsync("window.resolveOpenFile(null)");
        }
    }
    ```
    

### File Saving

1. **JavaScript Request**
    
    ```javascript
    window.csharpBridge.saveFile(modelData, "model.stl");
    ```
    
2. **C# Handler**
    
    ```csharp
    private void HandleSaveFile(string content, string suggestedFileName)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.FileName = suggestedFileName;
        saveFileDialog.Filter = "All files (*.*)|*.*";
    
        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                // Check if content is base64 encoded
                if (IsBase64String(content))
                {
                    byte[] bytes = Convert.FromBase64String(content);
                    File.WriteAllBytes(saveFileDialog.FileName, bytes);
                }
                else
                {
                    File.WriteAllText(saveFileDialog.FileName, content);
                }
                
                MessageBox.Show("File saved successfully.", "Success", 
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    ```
    

### Binary Data Handling

The application uses base64 encoding to transfer binary data between C# and JavaScript:

```csharp
// C# to JavaScript: Encoding binary data
byte[] fileBytes = File.ReadAllBytes(filePath);
string base64Content = Convert.ToBase64String(fileBytes);

// JavaScript to C#: Decoding binary data
if (IsBase64String(content))
{
    byte[] bytes = Convert.FromBase64String(content);
    File.WriteAllBytes(saveFileDialog.FileName, bytes);
}
```

## UI Components

### Splash Screen

The splash screen is displayed during application startup:

```xml
<Window x:Class="StructViz3D.SplashScreen"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="StructViz3D"
        Height="400" Width="600"
        WindowStyle="None"
        ResizeMode="NoResize"
        WindowStartupLocation="CenterScreen"
        Background="#1E1E1E">
    <Grid>
        <StackPanel VerticalAlignment="Center">
            <Image Source="/Assets/logo.png" Width="200" Height="200" Margin="0,0,0,20"/>
            <TextBlock Text="StructViz3D" FontSize="36" Foreground="White" 
                       HorizontalAlignment="Center" FontWeight="Bold"/>
            <TextBlock Text="3D Structure Visualization" FontSize="18" Foreground="#CCCCCC" 
                       HorizontalAlignment="Center" Margin="0,10,0,20"/>
            <ProgressBar IsIndeterminate="True" Width="300" Height="5" />
            <TextBlock Text="Version 1.0" FontSize="12" Foreground="#999999" 
                       HorizontalAlignment="Center" Margin="0,30,0,0"/>
        </StackPanel>
    </Grid>
</Window>
```

Key features:

- Dark-themed design with application branding
- Indeterminate progress bar for loading indication
- Centrally positioned on screen with no window chrome
- Auto-closes after a specified timeout

### Main Window

The main window contains the WebView2 control and serves as the primary application interface:

```xml
<Window x:Class="StructViz3D.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:wv2="clr-namespace:Microsoft.Web.WebView2.Wpf;assembly=Microsoft.Web.WebView2.Wpf"
        Title="StructViz3D - 3D Structure Visualization"
        Icon="/Assets/logo.png"
        Height="650" Width="900"
        WindowStartupLocation="CenterScreen">
    <Grid>
        <wv2:WebView2 x:Name="webView" />
    </Grid>
</Window>
```

Key features:

- WebView2 control filling the entire window
- Standard window chrome with application icon
- Default size and centered positioning

### Web Frontend

The web frontend is loaded into the WebView2 control and provides the interactive 3D visualization experience:

- `index.html`: Entry point for the web application
- `assets/index-*.css`: Styling for the web interface
- `assets/index-*.js`: JavaScript application logic and 3D rendering

## Security Considerations

### WebView2 Security

1. **Content Isolation**
    
    - Web content runs in an isolated process
    - Limited access to system resources
2. **Local Content Access**
    
    - Virtual host mapping provides secure access to local files
    - Prevents unintended file system access
3. **Script Execution Controls**
    
    - Controlled JavaScript execution context
    - Selective exposure of C# functionality

### File System Security

1. **Sandboxed Operations**
    
    - All file operations go through C# handlers
    - Web frontend cannot directly access file system
2. **Input Validation**
    
    - File paths and content are validated
    - Prevention of path traversal attacks
3. **Secure Binary Handling**
    
    - Base64 encoding for binary data transfer
    - Proper validation of encoded content

## Deployment and Distribution

### Installation Package

The application includes a Visual Studio setup project (StructViz3DSetup.vdproj) for creating Windows Installer packages:

```
Project("{54435603-DBB4-11D2-8724-00A0C9A8B90C}") = "StructViz3DSetup", "..\StructViz3DSetup\StructViz3DSetup.vdproj", "{E6940F87-4F04-F177-8B0E-C6CE3D375C6D}"
```

Key deployment considerations:

- WebView2 runtime detection and installation
- Proper file associations for supported 3D model formats
- Application shortcut creation
- Uninstallation support

### Prerequisites

Required components for running the application:

- .NET 8.0 Runtime
- WebView2 Runtime
- Windows 7 or later operating system

### Content Packaging

The application packages its web frontend content in the output directory:

- Web assets are marked with `CopyToOutputDirectory="PreserveNewest"` in the .csproj file
- Assets are loaded from the local filesystem through the virtual host mapping

## Build Configuration

### Platform Targets

The solution supports multiple build configurations:

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net8.0-windows</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <UseWPF>true</UseWPF>
  <Platforms>AnyCPU;x64</Platforms>
</PropertyGroup>
```

- `AnyCPU`: Compatible with both 32-bit and 64-bit systems
- `x64`: Optimized for 64-bit systems

### Build Configurations

1. **Debug Configuration**
    
    - Includes debugging symbols
    - Enables WebView2 developer tools
    - Additional diagnostics and logging
2. **Release Configuration**
    
    - Optimized for performance
    - Disables developer tools
    - Minimal logging

### Dependencies

External NuGet packages:

- `Microsoft.Web.WebView2` (v1.0.3065.39): Provides the WebView2 control
- `Newtonsoft.Json` (v13.0.3): JSON serialization/deserialization

## Performance Optimization

### WebView2 Optimizations

1. **Resource Loading**
    
    - Local resource loading via virtual host mapping
    - Static asset caching
2. **Script Execution**
    
    - Efficient script injection
    - Minimized cross-boundary calls
3. **Rendering Performance**
    
    - Hardware acceleration for 3D rendering
    - Optimized WebGL usage in web frontend

### Large File Handling

1. **Chunked Data Transfer**
    
    - For very large files, consider implementing chunked data transfer
    - Avoid memory limitations with base64 encoding of large binary data
2. **Asynchronous Operations**
    
    - All file operations are performed asynchronously
    - UI remains responsive during file loading/saving

## Error Handling

### Exception Management

Comprehensive error handling throughout the application:

```csharp
try
{
    // Operation that might fail
}
catch (Exception ex)
{
    MessageBox.Show($"Error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
    // Log error or take recovery action
}
```

### WebView2 Error Handling

Special consideration for WebView2-specific errors:

- Navigation failures
- Script execution errors
- Web message parsing errors

### User Feedback

Clear error messages and recovery options:

```csharp
MessageBox.Show($"Error processing file: {ex.Message}\n\nPlease try again with a different file.", 
                "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
```

## Extensibility

### Adding New File Formats

To add support for additional 3D file formats:

1. Update file filters in open/save dialogs:

```csharp
openFileDialog.Filter = "3D Models (*.stl;*.obj;*.ply)|*.stl;*.obj;*.ply|All files (*.*)|*.*";
```

1. Implement format-specific handling in the web frontend
2. Update MIME type detection and handling

### Plugin Architecture

Consider implementing a plugin system:

- Dynamic loading of file format handlers
- Extensible visualization capabilities
- Custom data processing modules

## Development Guidelines

### Coding Standards

- Follow standard C# coding conventions
- Use async/await for asynchronous operations
- Implement proper exception handling
- Document public APIs and complex logic

### Architecture Principles

1. **Separation of Concerns**
    
    - Native code handles system integration
    - Web frontend handles visualization
    - Clear communication protocols between layers
2. **Single Responsibility**
    
    - Each class has a specific purpose
    - Methods perform a single logical operation
3. **Dependency Inversion**
    
    - Interfaces define communication contracts
    - Implementation details are encapsulated

### Testing Strategy

1. **Unit Testing**
    
    - Test individual components in isolation
    - Mock dependencies and external systems
2. **Integration Testing**
    
    - Test C# and JavaScript integration
    - Verify file loading/saving workflows
3. **UI Testing**
    
    - Verify user interface behavior
    - Test responsiveness and error handling

## Troubleshooting Guide

### Common Issues

1. **WebView2 Initialization Failures**
    
    - **Symptoms**: Blank window, error messages about missing WebView2 runtime
    - **Solution**: Ensure WebView2 Runtime is installed, check for runtime version compatibility
2. **File Loading Errors**
    
    - **Symptoms**: Error messages when attempting to open files
    - **Solution**: Verify file permissions, check file format compatibility, validate file path
3. **JavaScript Bridge Communication Issues**
    
    - **Symptoms**: Actions not being triggered, missing responses
    - **Solution**: Check browser console for JavaScript errors, verify message format, validate WebView2 settings

### Diagnostic Techniques

1. **Logging**
    
    - Implement comprehensive logging
    - Log all cross-boundary communication
    - Track file operations and error conditions
2. **WebView2 Developer Tools**
    
    - Enable developer tools in debug builds:
    
    ```csharp
    webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
    ```
    
    - Use browser console for JavaScript debugging
    - Inspect network operations and resource loading
3. **Application Event Logging**
    
    - Monitor application lifecycle events
    - Track WebView2 navigation events
    - Log user interactions and error conditions

## Appendix

### JavaScript Bridge API Reference

```javascript
// Open a 3D model file
window.csharpBridge.openFile()
  .then(fileData => {
    // fileData structure:
    // {
    //   fileName: "model.stl",
    //   fileType: ".stl",
    //   content: "base64EncodedString"
    // }
  })
  .catch(error => {
    // Handle errors
  });

// Save a 3D model file
window.csharpBridge.saveFile(content, fileName);
  // content: String or base64 encoded binary
  // fileName: Suggested file name with extension
```

### Virtual Host Configuration

```csharp
// Virtual host structure
"https://structviz3d.local/" -> [AppDir]/web/
"https://structviz3d.local/assets/" -> [AppDir]/web/assets/
"https://structviz3d.local/index.html" -> [AppDir]/web/index.html
```

### WebView2 Settings Reference

```csharp
// Essential WebView2 settings
webView.CoreWebView2.Settings.IsWebMessageEnabled = true;          // Enable JS-to-C# messaging
webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true; // Allow JS dialogs
webView.CoreWebView2.Settings.AreDevToolsEnabled = true;           // Enable dev tools (debug)
```

### Application Flow Diagram

```
┌─────────────┐     ┌─────────────┐     ┌─────────────────────┐
│ Application │     │   Splash    │     │     Main Window     │
│    Start    │────►│   Screen    │────►│  (WebView2 Init)    │
└─────────────┘     │  (3s delay) │     └─────────────────────┘
                    └─────────────┘                │
                                                   ▼
                    ┌─────────────┐     ┌─────────────────────┐
                    │ User Views  │     │   Web Frontend      │
                    │ and Modifies│◄────│      Loaded         │
                    │  3D Models  │     │ (JS Bridge Ready)   │
                    └─────────────┘     └─────────────────────┘
```

### Data Flow Between C# and JavaScript

```
┌───────────────────┐                      ┌───────────────────┐
│                   │  1. JS Bridge Call   │                   │
│                   │─────────────────────►│                   │
│   JavaScript      │                      │      C#           │
│   Web Frontend    │                      │    Backend        │
│                   │◄─────────────────────│                   │
│                   │  2. Execute Script   │                   │
└───────────────────┘                      └───────────────────┘
         │                                         │
         │                                         │
         ▼                                         ▼
┌───────────────────┐                      ┌───────────────────┐
│  3D Visualization │                      │   File System     │
│     WebGL/Three.js│                      │      Access       │
└───────────────────┘                      └───────────────────┘
```



StructViz3D application.
