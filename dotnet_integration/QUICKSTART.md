# .NET Integration Quick Start

## For .NET Developers

This directory contains everything you need to use PyZK from .NET applications.

### Quick Start (3 steps)

1. **Install Python dependencies**
   ```bash
   pip install -r requirements.txt
   ```

2. **Add files to your .NET project**
   - Copy `PyZKClient.cs` to your project
   - Copy `pyzk_wrapper.py` to your project directory
   - Add NuGet package: `dotnet add package pythonnet --version 3.0.3`

3. **Use in your code**
   ```csharp
   using PyZK.DotNet;
   
   PyZKClient.InitializePython();
   
   using (var client = new PyZKClient("192.168.1.201"))
   {
       var result = client.Connect();
       if (result.Success)
       {
           var users = client.GetUsers();
           // ... use the data
           client.Disconnect();
       }
   }
   ```

### Files Included

- **PyZKClient.cs** - C# client library (copy to your project)
- **pyzk_wrapper.py** - Python wrapper (copy to your project)
- **Program.cs** - Complete example application
- **PyZKExample.csproj** - Example project file
- **README.md** - Full documentation
- **requirements.txt** - Python dependencies

### Run the Example

```bash
# Install dependencies
pip install -r requirements.txt

# Build and run
dotnet run --project PyZKExample.csproj
```

### Documentation

See `README.md` for complete documentation including:
- Detailed API reference
- Usage examples
- Troubleshooting guide
- Deployment options
- Performance tips

### Requirements

- Python 3.7+
- .NET 6.0+
- pythonnet NuGet package
- pyzk Python package
