<div align="center">
  <img src="https://img.shields.io/badge/.NET%20Framework-4.8-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 4.8"/>
  <img src="https://img.shields.io/badge/Windows-10%2F11-00A4EF?style=for-the-badge&logo=windows" alt="Windows"/>
  <img src="https://img.shields.io/badge/NVIDIA-Required-76B900?style=for-the-badge&logo=nvidia" alt="NVIDIA"/>
  <br/>
  <img src="https://img.shields.io/github/v/release/user/digital-vibrance-switcher?style=flat-square" alt="Release"/>
  <img src="https://img.shields.io/github/stars/user/digital-vibrance-switcher?style=flat-square" alt="Stars"/>
  <img src="https://img.shields.io/github/license/user/digital-vibrance-switcher?style=flat-square" alt="License"/>
</div>

<br/>

<h1 align="center">🎨 Digital Vibrance Switcher</h1>

<div align="center">
  <img src="docs/screenshot-main.png" alt="Main UI Screenshot" width="420"/>
</div>

<br/>

---

| English | Español |
|---------|---------|
| **Instant NVIDIA Digital Vibrance control — no more waiting for the Control Panel.** | **Controla el Digital Vibrance de tu GPU NVIDIA al instante. Olvídate del Panel de Control.** |

---

## 🔥 The Problem / El Problema

| English | Español |
|---------|---------|
| You're in a game and the colors feel **washed out**. Or you're editing a photo and the saturation is **too high**. The routine is always the same: | Estás jugando y los colores se ven **deslavados**. O estás editando una foto y la saturación es **demasiado alta**. La rutina es siempre la misma: |
| 1. Open **NVIDIA Control Panel** _(10-20s loading)_ | 1. Abrir el **Panel de Control de NVIDIA** _(10-20s cargando)_ |
| 2. Navigate to "Adjust desktop color settings" | 2. Navegar a "Ajustar configuración de color del escritorio" |
| 3. Wait for the page to render _(another 5s)_ | 3. Esperar que cargue la página _(otros 5s)_ |
| 4. Drag the **Digital Vibrance** slider | 4. Arrastrar el slider de **Digital Vibrance** |
| 5. Click **"Apply"** | 5. Hacer clic en **"Aplicar"** |
| 6. Go back to what you were doing | 6. Volver a lo que estabas haciendo |
| **It takes 20-30 seconds** for what should be a **one-click operation**. | **Tarda 20-30 segundos** para lo que debería ser **una operación de un solo clic**. |

## ✅ The Solution / La Solución

| English | Español |
|---------|---------|
| **Digital Vibrance Switcher** talks directly to NVIDIA's driver API (`nvapi64.dll`) to change the value **instantly** — no Control Panel, no waiting, no clicking Apply. | **Digital Vibrance Switcher** se comunica directamente con la API del driver NVIDIA (`nvapi64.dll`) para cambiar el valor **al instante** — sin Panel de Control, sin esperas, sin hacer clic en Aplicar. |

| Task | NVIDIA CP | **This app** | **Esta app** |
|------|-----------|-------------|-------------|
| Change vibrance / Cambiar vibrance | ~25 seg | ⚡ **instant / instantáneo** |
| Switch presets / Cambiar preset | navegación completa | **1 clic / 1 clic** |
| From any app / Desde cualquier app | alt+tab → wait/espera | **global hotkey / atajo global** |

---

## ✨ Features / Características

| English | Español |
|---------|---------|
| 🎚 **Precision slider** — Drag to any value from 0-100% with live preview | 🎚 **Slider de precisión** — Arrastra a cualquier valor de 0-100% con vista previa |
| ⚡ **Quick presets** — One-click: 40%, 50%, 60%, 70%, 80%, MAX | ⚡ **Presets rápidos** — Un clic: 40%, 50%, 60%, 70%, 80%, MAX |
| ⌨️ **Global hotkeys** — `Ctrl+Alt+1/2/3/4` from **any app**, even fullscreen games | ⌨️ **Atajos globales** — `Ctrl+Alt+1/2/3/4` desde **cualquier app**, incluso juegos en pantalla completa |
| 🖥 **System tray** — Minimizes to tray, accessible anytime | 🖥 **Bandeja del sistema** — Se minimiza a la bandeja, accesible en todo momento |
| 🚀 **Auto-start** — Launches with Windows, restores your last value | 🚀 **Auto-inicio** — Se inicia con Windows, restaura tu último valor |
| 🎨 **Dark theme** — Modern, clean UI that respects your eyes | 🎨 **Tema oscuro** — UI moderna y limpia que cuida tus ojos |
| 🔄 **Real-time feedback** — Visual arc indicator shows current value | 🔄 **Feedback en tiempo real** — Indicador visual de arco con el valor actual |
| 📊 **Multi-monitor aware** — Detects all your NVIDIA displays | 📊 **Multi-monitor** — Detecta todos tus displays NVIDIA |

---

## ⌨️ Hotkeys / Atajos de teclado

| English | Español |
|---------|---------|
| Work **globally** — in games, fullscreen apps, anywhere. Change vibrance without even tabbing out. | Funcionan **globalmente** — en juegos, apps a pantalla completa, en cualquier lugar. Cambia el vibrance sin ni siquiera salir de la app. |

| Keys / Teclas | Action / Acción |
|---------------|-----------------|
| `Ctrl` + `Alt` + `1` | **50%** |
| `Ctrl` + `Alt` + `2` | **60%** |
| `Ctrl` + `Alt` + `3` | **70%** |
| `Ctrl` + `Alt` + `4` | **80%** |

---

## 📦 Installation / Instalación

| English | Español |
|---------|---------|
| **Quick start** | **Inicio rápido** |
| 1. Download the latest `DigitalVibrance.exe` from [Releases](https://github.com/user/digital-vibrance-switcher/releases) | 1. Descarga el último `DigitalVibrance.exe` desde [Releases](https://github.com/user/digital-vibrance-switcher/releases) |
| 2. **Double-click** the `.exe` — no installation needed | 2. **Haz doble clic** en el `.exe` — no necesita instalación |
| 3. The app opens and **immediately applies** the last used vibrance value | 3. La app se abre y **aplica inmediatamente** el último valor usado |
| 4. Use the **slider** or click a **preset** (40%, 50%, 60%, 70%, 80%, MAX) to change | 4. Usa el **slider** o haz clic en un **preset** (40%, 50%, 60%, 70%, 80%, MAX) |
| 5. Close the window → it **minimizes to tray** (it keeps running) | 5. Cierra la ventana → se **minimiza a la bandeja** (sigue funcionando) |
| 6. To reopen: **double-click the tray icon** or right-click → "Show Window" | 6. Para reabrir: **doble clic en el icono de la bandeja** o clic derecho → "Show Window" |
| 7. To exit completely: right-click tray icon → **"Exit"** | 7. Para salir: clic derecho en la bandeja → **"Exit"** |
| The app **remembers** your last value and restores it on next launch. | La app **recuerda** tu último valor y lo restaura al siguiente inicio. |

### Requirements / Requisitos

| English | Español |
|---------|---------|
| **Windows 10 or 11** (64-bit) | **Windows 10 u 11** (64 bits) |
| **NVIDIA GPU** with drivers installed | **GPU NVIDIA** con drivers instalados |
| **nvapi64.dll** (included with NVIDIA drivers) | **nvapi64.dll** (incluida con los drivers NVIDIA) |
| [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) (usually pre-installed) | [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) (generalmente pre-instalado) |

### Build from source / Compilar desde el código

```bash
git clone https://github.com/yourname/digital-vibrance-switcher.git
cd digital-vibrance-switcher

# Option A: Using MSBuild (Visual Studio Build Tools required)
# Opción A: Usando MSBuild (requiere Visual Studio Build Tools)
msbuild DigitalVibranceSwitcher.csproj /p:Configuration=Release /p:Platform=x64

# Option B: Using csc.exe directly
# Opción B: Usando csc.exe directamente
csc -nologo -platform:x64 -target:winexe -out:DigitalVibrance.exe ^
    -reference:System.dll -reference:System.Drawing.dll ^
    -reference:System.Windows.Forms.dll -reference:System.Runtime.Serialization.dll ^
    Program.cs Native\NvApi.cs Core\VibranceController.cs Core\SettingsManager.cs ^
    Core\HotkeyManager.cs UI\ModernTheme.cs UI\ModernTrackBar.cs UI\MainForm.cs UI\TrayManager.cs
```

---

## 🏗 Architecture / Arquitectura

```
DigitalVibranceSwitcher/
├── Program.cs                  # Entry point / Punto de entrada
├── Native/
│   └── NvApi.cs                # NVAPI P/Invoke bindings / Enlaces NVAPI
├── Core/
│   ├── VibranceController.cs   # Business logic / Lógica de negocio
│   ├── SettingsManager.cs      # JSON preferences / Preferencias JSON
│   └── HotkeyManager.cs        # Global hotkeys / Atajos globales
├── UI/
│   ├── ModernTheme.cs          # Color palette & typography / Paleta y tipografía
│   ├── ModernTrackBar.cs       # Custom slider / Slider personalizado
│   ├── MainForm.cs             # Main window / Ventana principal
│   └── TrayManager.cs          # System tray / Bandeja del sistema
├── Properties/
│   └── AssemblyInfo.cs         # Version metadata / Metadatos de versión
├── build.bat / build.ps1       # Build scripts / Scripts de compilación
├── app.manifest                # DPI / Windows compatibility
├── DigitalVibranceSwitcher.csproj
├── LICENSE
└── README.md
```

---

## 🔧 How It Works / Cómo funciona

| English | Español |
|---------|---------|
| The app uses **P/Invoke** to call NVIDIA's proprietary API (`nvapi64.dll`) directly: | La app usa **P/Invoke** para llamar directamente a la API propietaria de NVIDIA (`nvapi64.dll`): |
| 1. **QueryInterface** → get function pointers for each NVAPI function | 1. **QueryInterface** → obtener punteros a cada función NVAPI |
| 2. **NvAPI_Initialize** → establish a session with the driver | 2. **NvAPI_Initialize** → establecer sesión con el driver |
| 3. **NvAPI_EnumPhysicalGPUs** → enumerate available NVIDIA GPUs | 3. **NvAPI_EnumPhysicalGPUs** → enumerar las GPUs NVIDIA disponibles |
| 4. **NvAPI_GetDVCInfoEx** → read current Digital Vibrance range/values | 4. **NvAPI_GetDVCInfoEx** → leer rango/valores actuales de Digital Vibrance |
| 5. **NvAPI_SetDVCInfoEx** → write new Digital Vibrance value | 5. **NvAPI_SetDVCInfoEx** → escribir el nuevo valor de Digital Vibrance |
| The change takes effect **immediately** — no driver reload, no display restart, no "Apply" button. | El cambio tiene efecto **inmediatamente** — sin recargar el driver, sin reiniciar el display, sin botón "Aplicar". |

---

## 📋 Project Status / Estado del proyecto

| English | Español |
|---------|---------|
| ✅ Core NVAPI integration | ✅ Integración NVAPI principal |
| ✅ Preset system (quick buttons) | ✅ Sistema de presets (botones rápidos) |
| ✅ Global hotkeys (`Ctrl+Alt+1/2/3/4`) | ✅ Atajos globales (`Ctrl+Alt+1/2/3/4`) |
| ✅ System tray with context menu | ✅ Bandeja del sistema con menú contextual |
| ✅ Dark theme UI | ✅ Interfaz de tema oscuro |
| ✅ Auto-start with Windows | ✅ Inicio automático con Windows |
| ✅ Multi-monitor support | ✅ Soporte multi-monitor |
| ✅ Smooth transitions (animated value changes) | ✅ Transiciones suaves (cambios animados) |
| ✅ Per-application profiles | ✅ Perfiles por aplicación |
| ✅ Game detection & auto-switching | ✅ Detección de juegos y cambio automático |
| ✅ DDC/CI monitor control | ✅ Control de monitor DDC/CI |

---

## 🤝 Contributing / Contribuciones

| English | Español |
|---------|---------|
| Contributions are welcome! Open an issue or submit a PR. | ¡Las contribuciones son bienvenidas! Abre un issue o envía un PR. |
| 1. Fork the repo / Haz fork del repositorio | 2. Create your feature branch / Crea tu rama de funcionalidad |
| 3. Commit your changes / Haz commit de tus cambios | 4. Push to the branch / Sube los cambios |
| 5. Open a Pull Request / Abre un Pull Request | |

---

## 📄 License / Licencia

| English | Español |
|---------|---------|
| MIT — see [LICENSE](LICENSE). | MIT — ver [LICENSE](LICENSE). |

---

<div align="center">
  <sub>Built with ❤️ because waiting for NVIDIA Control Panel is not an option.<br/>
  Hecho con ❤️ porque esperar al Panel de Control de NVIDIA no es una opción.</sub>
</div>