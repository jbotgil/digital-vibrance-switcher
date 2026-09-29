<div align="center">
  <img src="https://img.shields.io/badge/.NET%20Framework-4.8-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 4.8"/>
  <img src="https://img.shields.io/badge/Windows-10%2F11-00A4EF?style=for-the-badge&logo=windows" alt="Windows"/>
  <img src="https://img.shields.io/badge/NVIDIA-Required-76B900?style=for-the-badge&logo=nvidia" alt="NVIDIA"/>
  <br/>
  <img src="https://img.shields.io/github/v/release/jbotgil/digital-vibrance-switcher?style=flat-square" alt="Release"/>
  <img src="https://img.shields.io/github/stars/jbotgil/digital-vibrance-switcher?style=flat-square" alt="Stars"/>
  <img src="https://img.shields.io/github/license/jbotgil/digital-vibrance-switcher?style=flat-square" alt="License"/>
</div>

<br/>

<h1 align="center">🎨 Digital Vibrance Switcher</h1>

<div align="center">
  <img src="docs/screenshot-main.png" alt="Main UI Screenshot" width="420"/>
</div>

> ⚙️ **Instant NVIDIA Digital Vibrance control — no more waiting for the Control Panel.**
> ⚙️ **Controla el Digital Vibrance de tu GPU NVIDIA al instante. Olvídate del Panel de Control.**

---

# 🌐 Language — Idioma

<details open>
<summary>🇬🇧 English — click for English version / clic para la versión en inglés</summary>

---

## 🔥 The Problem

You're in a game and the colors feel **washed out**. Or you're editing a photo and the saturation is **too high**. The routine is always the same:

1. Open **NVIDIA Control Panel** _(10-20s loading)_
2. Navigate to "Adjust desktop color settings"
3. Wait for the page to render _(another 5s)_
4. Drag the **Digital Vibrance** slider
5. Click **"Apply"**
6. Go back to what you were doing

**It takes 20-30 seconds** for what should be a **one-click operation**.

## ✅ The Solution

**Digital Vibrance Switcher** talks directly to NVIDIA's driver API (`nvapi64.dll`) to change the value **instantly** — no Control Panel, no waiting, no clicking Apply.

| Task | NVIDIA CP | **This app** |
|------|-----------|--------------|
| Change vibrance | ~25 sec | ⚡ **instant** |
| Switch presets | full navigation | **1 click** |
| From any app | alt+tab → wait | **global hotkey** |

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🎚 **Precision slider** | Drag to any value from 0-100% with live preview |
| ⚡ **Quick presets** | One-click: 40%, 50%, 60%, 70%, 80%, MAX |
| ⌨️ **Global hotkeys** | `Ctrl+Alt+1/2/3/4` from **any app**, even fullscreen games |
| 🖥 **System tray** | Minimizes to tray, accessible anytime |
| 🚀 **Auto-start** | Launches with Windows, restores your last value |
| 🎨 **Dark theme** | Modern, clean UI that respects your eyes |
| 🔄 **Real-time feedback** | Visual arc indicator shows current value |
| 📊 **Multi-monitor aware** | Detects all your NVIDIA displays |

## ⌨️ Hotkeys

Work **globally** — in games, fullscreen apps, anywhere. Change vibrance without even tabbing out.

| Keys | Action |
|------|--------|
| `Ctrl` + `Alt` + `1` | **50%** |
| `Ctrl` + `Alt` + `2` | **60%** |
| `Ctrl` + `Alt` + `3` | **70%** |
| `Ctrl` + `Alt` + `4` | **80%** |

## 📦 Installation

**Quick start:**
1. Download the latest `DigitalVibrance.exe` from [Releases](https://github.com/jbotgil/digital-vibrance-switcher/releases)
2. **Double-click** the `.exe` — no installation needed
3. The app opens and **immediately applies** the last used vibrance value
4. Use the **slider** or click a **preset** to change
5. Close the window → it **minimizes to tray** (it keeps running)
6. To reopen: **double-click the tray icon** or right-click → "Show Window"
7. To exit completely: right-click tray icon → **"Exit"**

The app **remembers** your last value and restores it on next launch.

### Requirements
- **Windows 10 or 11** (64-bit)
- **NVIDIA GPU** with drivers installed
- **nvapi64.dll** (included with NVIDIA drivers)
- [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)

### Build from source
```bash
git clone https://github.com/jbotgil/digital-vibrance-switcher.git
cd digital-vibrance-switcher
msbuild DigitalVibranceSwitcher.csproj /p:Configuration=Release /p:Platform=x64
```

## 🔧 How It Works

The app uses **P/Invoke** to call NVIDIA's proprietary API (`nvapi64.dll`) directly:
1. **QueryInterface** → get function pointers for each NVAPI function
2. **NvAPI_Initialize** → establish a session with the driver
3. **NvAPI_EnumPhysicalGPUs** → enumerate available NVIDIA GPUs
4. **NvAPI_GetDVCInfoEx** → read current Digital Vibrance range/values
5. **NvAPI_SetDVCInfoEx** → write new Digital Vibrance value

The change takes effect **immediately** — no driver reload, no display restart, no "Apply" button.

## 📋 Project Status

- ✅ Core NVAPI integration
- ✅ Preset system (quick buttons)
- ✅ Global hotkeys (`Ctrl+Alt+1/2/3/4`)
- ✅ System tray with context menu
- ✅ Dark theme UI
- ✅ Auto-start with Windows
- ✅ Multi-monitor support
- ✅ Smooth transitions (animated value changes)
- ✅ Per-application profiles
- ✅ Game detection & auto-switching
- ✅ DDC/CI monitor control

## 🤝 Contributing

Contributions are welcome! Open an issue or submit a PR.
1. Fork the repo
2. Create your feature branch (`git checkout -b feature/amazing-idea`)
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📄 License

MIT — see [LICENSE](LICENSE).

</details>

<br/>

<details>
<summary>🇪🇸 Español — haz clic para la versión en español / click for Spanish version</summary>

---

## 🔥 El Problema

Estás jugando y los colores se ven **deslavados**. O estás editando una foto y la saturación es **demasiado alta**. La rutina es siempre la misma:

1. Abrir el **Panel de Control de NVIDIA** _(10-20s cargando)_
2. Navegar a "Ajustar configuración de color del escritorio"
3. Esperar que cargue la página _(otros 5s)_
4. Arrastrar el slider de **Digital Vibrance**
5. Hacer clic en **"Aplicar"**
6. Volver a lo que estabas haciendo

**Tarda 20-30 segundos** para lo que debería ser **una operación de un solo clic**.

## ✅ La Solución

**Digital Vibrance Switcher** se comunica directamente con la API del driver NVIDIA (`nvapi64.dll`) para cambiar el valor **al instante** — sin Panel de Control, sin esperas, sin hacer clic en Aplicar.

| Tarea | NVIDIA CP | **Esta app** |
|-------|-----------|--------------|
| Cambiar vibrance | ~25 seg | ⚡ **instantáneo** |
| Cambiar preset | navegación completa | **1 clic** |
| Desde cualquier app | alt+tab → espera | **atajo global** |

## ✨ Características

| Característica | Descripción |
|----------------|-------------|
| 🎚 **Slider de precisión** | Arrastra a cualquier valor de 0-100% con vista previa |
| ⚡ **Presets rápidos** | Un clic: 40%, 50%, 60%, 70%, 80%, MAX |
| ⌨️ **Atajos globales** | `Ctrl+Alt+1/2/3/4` desde **cualquier app**, incluso juegos a pantalla completa |
| 🖥 **Bandeja del sistema** | Se minimiza a la bandeja, accesible en todo momento |
| 🚀 **Auto-inicio** | Se inicia con Windows, restaura tu último valor |
| 🎨 **Tema oscuro** | UI moderna y limpia que cuida tus ojos |
| 🔄 **Feedback en tiempo real** | Indicador visual de arco con el valor actual |
| 📊 **Multi-monitor** | Detecta todos tus displays NVIDIA |

## ⌨️ Atajos de teclado

Funcionan **globalmente** — en juegos, apps a pantalla completa, en cualquier lugar. Cambia el vibrance sin ni siquiera salir de la app.

| Teclas | Acción |
|--------|--------|
| `Ctrl` + `Alt` + `1` | **50%** |
| `Ctrl` + `Alt` + `2` | **60%** |
| `Ctrl` + `Alt` + `3` | **70%** |
| `Ctrl` + `Alt` + `4` | **80%** |

## 📦 Instalación

**Inicio rápido:**
1. Descarga el último `DigitalVibrance.exe` desde [Releases](https://github.com/jbotgil/digital-vibrance-switcher/releases)
2. **Haz doble clic** en el `.exe` — no necesita instalación
3. La app se abre y **aplica inmediatamente** el último valor usado
4. Usa el **slider** o haz clic en un **preset** para cambiar
5. Cierra la ventana → se **minimiza a la bandeja** (sigue funcionando)
6. Para reabrir: **doble clic en el icono de la bandeja** o clic derecho → "Show Window"
7. Para salir: clic derecho en la bandeja → **"Exit"**

La app **recuerda** tu último valor y lo restaura al siguiente inicio.

### Requisitos
- **Windows 10 u 11** (64 bits)
- **GPU NVIDIA** con drivers instalados
- **nvapi64.dll** (incluida con los drivers NVIDIA)
- [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)

### Compilar desde el código
```bash
git clone https://github.com/jbotgil/digital-vibrance-switcher.git
cd digital-vibrance-switcher
msbuild DigitalVibranceSwitcher.csproj /p:Configuration=Release /p:Platform=x64
```

## 🔧 Cómo funciona

La app usa **P/Invoke** para llamar directamente a la API propietaria de NVIDIA (`nvapi64.dll`):
1. **QueryInterface** → obtener punteros a cada función NVAPI
2. **NvAPI_Initialize** → establecer sesión con el driver
3. **NvAPI_EnumPhysicalGPUs** → enumerar las GPUs NVIDIA disponibles
4. **NvAPI_GetDVCInfoEx** → leer rango/valores actuales de Digital Vibrance
5. **NvAPI_SetDVCInfoEx** → escribir el nuevo valor de Digital Vibrance

El cambio tiene efecto **inmediatamente** — sin recargar el driver, sin reiniciar el display, sin botón "Aplicar".

## 📋 Estado del proyecto

- ✅ Integración NVAPI principal
- ✅ Sistema de presets (botones rápidos)
- ✅ Atajos globales (`Ctrl+Alt+1/2/3/4`)
- ✅ Bandeja del sistema con menú contextual
- ✅ Interfaz de tema oscuro
- ✅ Inicio automático con Windows
- ✅ Soporte multi-monitor
- ✅ Transiciones suaves (cambios animados)
- ✅ Perfiles por aplicación
- ✅ Detección de juegos y cambio automático
- ✅ Control de monitor DDC/CI

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Abre un issue o envía un PR.
1. Haz fork del repositorio
2. Crea tu rama de funcionalidad (`git checkout -b feature/amazing-idea`)
3. Haz commit de tus cambios
4. Sube los cambios
5. Abre un Pull Request

## 📄 Licencia

MIT — ver [LICENSE](LICENSE).

</details>

---

<div align="center">
  <sub>MIT License · Windows 10/11 · .NET Framework 4.8</sub>
</div>