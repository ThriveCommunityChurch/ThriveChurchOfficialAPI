# Docker Debugging in Visual Studio

## 🐳 Overview

You can now debug your API running in a Linux Docker container directly from Visual Studio! This gives you:

- ✅ **True Linux environment** - Debug exactly as it runs in production
- ✅ **Breakpoints** - Set breakpoints and step through code
- ✅ **Hot reload** - Changes reflect immediately (in debug mode)
- ✅ **Full debugging experience** - Watch variables, call stack, etc.

---

## 🚀 How to Use

### **Option 1: Using the Debug Dropdown (Easiest)**

1. **Open Visual Studio** and load the solution
2. **Look at the debug dropdown** (green play button at the top)
3. **Select "Docker"** from the dropdown instead of "IIS Express" or "ThriveChurchOfficialAPI"
4. **Press F5** or click the green play button
5. Visual Studio will:
   - Build the Docker image with debug support
   - Start the container
   - Attach the debugger
   - Open Swagger in your browser

### **Option 2: Right-Click Project**

1. Right-click on **ThriveChurchOfficialAPI** project
2. Select **Debug** → **Start New Instance**
3. Choose **Docker** profile

---

## 🔧 What Happens Behind the Scenes

When you debug with Docker:

1. **Dockerfile builds with `debug` target** - Includes source code and debugger
2. **Container starts with Development environment** - Uses your `.env` file
3. **Visual Studio Remote Debugger (vsdbg)** - Installed in the container
4. **Source code is mounted** - Changes reflect immediately (via volumes)
5. **Debugger attaches** - Full debugging experience

---

## 🎯 Debug Profiles Available

| Profile | Environment | Description |
|---------|-------------|-------------|
| **IIS Express** | Windows | Traditional Windows debugging via IIS |
| **ThriveChurchOfficialAPI** | Windows | Kestrel on Windows (direct run) |
| **Docker** | Linux | Linux container debugging |

---

## 📋 Requirements

- ✅ Docker Desktop installed and running
- ✅ `.env` file with your secrets (already created)
- ✅ Visual Studio 2022 (or 2019 with Docker support)
- ✅ "Container Development Tools" workload installed in Visual Studio

---

## 🐛 Debugging Features

### **Set Breakpoints**
- Click in the left margin of any code line
- Breakpoint will hit when code executes in the Linux container

### **Watch Variables**
- Hover over variables to see values
- Use Watch window to monitor specific variables

### **Step Through Code**
- F10 - Step Over
- F11 - Step Into
- Shift+F11 - Step Out

### **Hot Reload**
- Make code changes while debugging
- Changes apply immediately (in most cases)

---

## 🔍 Troubleshooting

### **"Docker is not running"**
- Start Docker Desktop
- Wait for it to fully start (whale icon in system tray)

### **"Cannot connect to Docker daemon"**
- Open Docker Desktop settings
- Ensure "Expose daemon on tcp://localhost:2375 without TLS" is enabled (if needed)
- Restart Docker Desktop

### **"Port 8080 is already in use"**
- Stop any running containers: `docker ps` then `docker stop <container-id>`
- Or change the port in `launchSettings.json`

### **Breakpoints not hitting**
- Ensure you're building in **Debug** configuration (not Release)
- Check that the Dockerfile `debug` target is being used
- Verify source code is mounted correctly in `docker-compose.debug.yml`

### **Environment variables not loading**
- Ensure `.env` file exists in `API/ThriveChurchOfficialAPI/`
- Check that `docker-compose.debug.yml` references the `.env` file
- Verify environment variable names match (case-sensitive)

---

## 📊 Performance Notes

**First Run:**
- Takes longer (builds image, installs debugger)
- ~2-3 minutes

**Subsequent Runs:**
- Much faster (uses cached layers)
- ~10-30 seconds

**Hot Reload:**
- Near instant for most code changes
- Some changes require container restart

---

## 🎓 Tips

1. **Use Docker for Linux-specific testing** - Test path handling, case sensitivity, etc.
2. **Use Windows debugging for faster iteration** - When you don't need Linux-specific behavior
3. **Check container logs** - View Output window → Show output from: Docker
4. **Clean up containers** - Visual Studio usually cleans up, but check `docker ps -a` occasionally

---

## 🔗 Related Files

- `Dockerfile` - Contains `debug` target for debugging
- `docker-compose.debug.yml` - Debug-specific compose configuration
- `Properties/launchSettings.json` - Debug profiles including Docker
- `.env` - Environment variables for local development

---

## 🚀 Next Steps

Try it out:
1. Set a breakpoint in a controller (e.g., `SermonsController.cs`)
2. Select "Docker" from the debug dropdown
3. Press F5
4. Navigate to the API endpoint in Swagger
5. Watch your breakpoint hit! 🎉

