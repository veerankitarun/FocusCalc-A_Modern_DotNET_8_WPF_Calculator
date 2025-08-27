

# 📌 FocusCalc – A Modern .NET 8 WPF Calculator

![GitHub release (latest by date)](https://img.shields.io/github/v/release/veerankitarun/FocusCalc-A_Modern_DotNET_8_WPF_Calculator?style=for-the-badge)
![GitHub last commit](https://img.shields.io/github/last-commit/veerankitarun/FocusCalc-A_Modern_DotNET_8_WPF_Calculator?style=for-the-badge)
![GitHub repo size](https://img.shields.io/github/repo-size/veerankitarun/FocusCalc-A_Modern_DotNET_8_WPF_Calculator?style=for-the-badge)

A modern, lightweight calculator app built with **.NET 8 and WPF**, featuring a custom UI, history panel, keyboard shortcuts, and portable distribution.  



## 🚀 Download & Run
1. Go to the **[Releases](https://github.com/veerankitarun/FocusCalc-A_Modern_DotNET_8_WPF_Calculator/releases)** page.  
2. Download the latest `.zip` or `.exe` under **Assets** (e.g., `FocusCalc-1.3.0.zip`).  
3. Extract (if zipped) and double-click `Calculator.exe`.  

✔ No .NET runtime required — the app is **self-contained**.  
✔ Works on **Windows 10/11** (x64).  


## 🛠 Tech Stack
- **.NET 8 (C#)** – Core framework & logic  
- **WPF (Windows Presentation Foundation)** – Desktop UI  
- **XAML** – Declarative layout  
- **ObservableCollection** – Real-time history updates  
- **Custom Control Templates** – macOS-style window controls, modern UI styling  
- **Self-contained Publish** – Single `.exe` distribution  



## ✨ Features
- 🖥️ Custom chrome window with macOS-style traffic-light controls  
- 🔽 Collapsible/expandable UI  
- ➕ Basic arithmetic operations (+, −, ×, ÷, %, sign toggle)  
- ⌨️ Full keyboard support (digits, operators, Enter, Backspace, Esc, Ctrl+H for history)  
- 📜 Dedicated history window  
- 🚀 Portable `.exe` (no runtime install needed)  

---

## This is the outcome 
<img width="813" height="668" alt="image" src="https://github.com/user-attachments/assets/2083cf3d-1bea-4867-ab4e-90abac2678ff" />

## 🧑‍💻 Build Instructions
For developers who want to build from source:  

```bash
# clone repo
git clone https://github.com/veerankitarun/FocusCalc-A_Modern_DotNET_8_WPF_Calculator.git
cd FocusCalc-A_Modern_DotNET_8_WPF_Calculator/Calculator


# build
dotnet build

# publish portable exe
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:PublishReadyToRun=true --output ../publish-portable



