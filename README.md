<h1 align="center">JustMeetingPoint.Maui</h1>

<p align="center">
  Aplicación móvil MAUI para autenticación, navegación principal y futura experiencia de usuario de JustMeetingPoint
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com/">
    <img src="https://img.shields.io/badge/.NET-MAUI%20%7C%20C%23-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET MAUI" />
  </a>
  <img src="https://img.shields.io/badge/Arquitectura-MVVM-0F766E?style=for-the-badge" alt="MVVM" />
  <img src="https://img.shields.io/badge/Comunicación-TCP%20Sockets-0A66C2?style=for-the-badge" alt="TCP Sockets" />
  <img src="https://img.shields.io/badge/UI-XAML-111827?style=for-the-badge" alt="XAML" />
  <img src="https://img.shields.io/badge/Estado-En%20desarrollo-F59E0B?style=for-the-badge" alt="Estado" />
</p>

<p align="center">
  Cliente móvil del Proyecto Final de Grado centrado en autenticación, navegación estructurada y evolución hacia una experiencia completa para grupos y puntos de encuentro.
</p>

---

## Índice

- [Visión general](#visión-general)
- [Qué problema aborda](#qué-problema-aborda)
- [Capacidades actuales](#capacidades-actuales)
- [Arquitectura](#arquitectura)
- [Stack tecnológico](#stack-tecnológico)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Flujo de ejecución](#flujo-de-ejecución)
- [Puesta en marcha](#puesta-en-marcha)
- [Configuración](#configuración)
- [Recursos visuales e iconografía](#recursos-visuales-e-iconografía)
- [Estado del proyecto](#estado-del-proyecto)
- [Roadmap](#roadmap)
- [Por qué este proyecto tiene valor](#por-qué-este-proyecto-tiene-valor)
- [Autor](#autor)
- [Licencia](#licencia)

---

## Visión general

**JustMeetingPoint.Maui** es la aplicación cliente móvil desarrollada en **.NET MAUI** para la solución **JustMeetingPoint**.

Su objetivo es proporcionar una interfaz moderna, clara y mantenible para interactuar con el backend de la plataforma, cubriendo tanto la autenticación de usuarios como la navegación principal de la aplicación y la futura experiencia orientada a grupos y puntos de encuentro.

En su estado actual, el repositorio se centra en construir una base técnica sólida para:

- autenticación de usuarios mediante sockets
- interfaz móvil basada en XAML
- separación de responsabilidades mediante MVVM
- estructura modular por funcionalidades
- base preparada para navegación principal con `TabBar`
- evolución futura hacia grupos, mapa y perfil

No es solo una capa visual. Su valor está en servir de cliente real para un backend distribuido, manteniendo una estructura limpia, escalable y orientada a crecimiento funcional.

---

## Qué problema aborda

Una aplicación cliente para un sistema distribuido no debe limitarse a pintar pantallas. También debe:

- representar correctamente los flujos de negocio
- mantenerse alineada con el protocolo del servidor
- separar interfaz, lógica de presentación y acceso a servicios
- escalar sin convertirse en una colección de pantallas acopladas

**JustMeetingPoint.Maui** aborda ese problema desde una perspectiva de ingeniería frontend aplicada al ecosistema .NET, priorizando una base mantenible antes de añadir complejidad visual o funcional innecesaria.

---

## Capacidades actuales

### Implementado

- Pantalla de inicio de sesión
- Pantalla de registro de usuario
- Validación básica de formularios en cliente
- Comunicación con backend mediante TCP sockets
- DTOs de request y response para autenticación
- ViewModels para login y register
- Servicio de autenticación desacoplado mediante interfaz
- Navegación entre vistas de autenticación mediante Shell
- Base preparada para incorporar navegación principal con pestañas

### Puntos fuertes técnicos

- Separación clara entre UI, lógica de presentación y acceso a red
- Enfoque modular por funcionalidades (`Auth`, `Home`, etc.)
- Integración real con protocolo socket del servidor
- Base preparada para crecimiento hacia navegación principal y funcionalidades de dominio

---

## Arquitectura

La aplicación sigue una organización modular con criterio MVVM y separación por funcionalidades.

### `Features/Auth`
Gestiona el flujo de autenticación.

**Responsabilidades**
- vistas de login y registro
- DTOs de autenticación
- lógica de presentación
- servicio de autenticación

### `Features/Home`
Reservado para la futura navegación principal de la app.

**Responsabilidades previstas**
- pantallas principales tras autenticación
- home/dashboard
- grupos
- mapa
- perfil

### `NetUtils`
Contiene utilidades compartidas de red adaptadas al cliente MAUI.

**Responsabilidades**
- creación de sockets
- envío y recepción de tipos básicos
- soporte del protocolo binario usado con el servidor

### Características arquitectónicas

- **Separación por feature**
- **MVVM** para desacoplar UI y lógica
- **Servicios abstraídos mediante interfaz**
- **Diseño incremental**, alineado con la evolución del backend
- **Cliente real**, no mock visual aislado

---

## Stack tecnológico

<p>
  <img src="https://skillicons.dev/icons?i=cs,dotnet,git,github" alt="Iconos del stack" />
</p>

| Área | Tecnología |
|---|---|
| Lenguaje | C# |
| UI | XAML |
| Framework | .NET MAUI |
| Patrón | MVVM |
| Comunicación | TCP Sockets |
| Arquitectura | Modular por funcionalidades |
| Control de versiones | Git / GitHub |

---

## Estructura del proyecto

```text
JustMeetinPoint.Maui/
│
├── Features/
│   ├── Auth/
│   │   ├── Dtos/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── ViewModels/
│   │   └── Views/
│   │
│   └── Home/
│       ├── Models/
│       ├── Services/
│       ├── ViewModels/
│       └── Views/
│
├── NetUtils/
│   └── Utilidades de socket y protocolo compartido
│
├── Resources/
│   ├── AppIcon/
│   ├── Fonts/
│   ├── Images/
│   └── Splash/
│
├── App.xaml
├── AppShell.xaml
├── MauiProgram.cs
└── JustMeetinPoint.Maui.csproj
```

### Criterio de estructura

La estructura busca evitar mezclar:
- XAML con lógica de presentación
- lógica de presentación con acceso a red
- detalles de socket con la UI

Esto mejora la mantenibilidad y permite evolucionar la aplicación funcionalidad a funcionalidad sin degradar la calidad del código.

---

## Flujo de ejecución

```text
Usuario abre la app
   ↓
Se muestra flujo de autenticación
   ↓
Login / Register validan datos en cliente
   ↓
El ViewModel construye un DTO
   ↓
El servicio de autenticación abre socket con el servidor
   ↓
Se envían datos según el protocolo acordado
   ↓
El servidor responde
   ↓
La UI actualiza el estado mostrado al usuario
```

Este flujo constituye la base sobre la que se soportarán más adelante:

- navegación principal con TabBar
- gestión de grupos
- selección y cálculo de punto de encuentro
- perfil y configuración
- estados persistentes de sesión

---

## Puesta en marcha

### Requisitos previos

- .NET 9 SDK
- Workload de .NET MAUI instalado
- Visual Studio 2022 con soporte MAUI
- Android Emulator o dispositivo físico
- Backend de **JustMeetingPoint** en ejecución

### Clonar el repositorio

```bash
git clone <URL_DEL_REPOSITORIO_FRONTEND>
cd JustMeetinPoint.Maui
```

### Restaurar dependencias

```bash
dotnet restore
```

### Compilar el proyecto

```bash
dotnet build
```

### Ejecutar la app

Desde Visual Studio:
- seleccionar emulador o dispositivo
- ejecutar el proyecto MAUI

> Para que login y registro funcionen, el servidor TCP debe estar activo y accesible desde la IP y puerto configurados en el servicio de autenticación.

---

## Configuración

En el estado actual, la configuración relevante está asociada principalmente al servicio de autenticación por sockets.

Parámetros habituales a revisar:

- IP del servidor
- puerto del servidor
- coincidencia entre protocolo cliente y servidor
- rutas Shell para navegación principal
- recursos visuales disponibles en `Resources/Images`

### Evolución recomendada

Para mejorar mantenibilidad y despliegue, los siguientes pasos recomendables serían:

- externalizar host y puerto a configuración
- soportar distintos entornos
- desacoplar mejor la navegación posterior a autenticación
- incorporar gestión de sesión en cliente

---

## Recursos visuales e iconografía

La aplicación utiliza iconografía de terceros en la navegación principal.

### Créditos de iconos

Los siguientes iconos han sido utilizados en la interfaz y pertenecen a sus respectivos autores en **The Noun Project**:

- **group.png** — Created by **Jae Deasigner** from The Noun Project
- **home.png** — Created by **Miss Jannes** from The Noun Project
- **profile.png** — Created by **verry poernomo** from The Noun Project
- **map.png** — Created by **Logisstudio** from The Noun Project

### Nota de atribución

En caso de utilizar estos recursos bajo licencia con atribución, los créditos anteriores deben mantenerse visibles en la documentación del proyecto o en una futura sección de créditos dentro de la aplicación.

---

## Estado del proyecto

> **Estado actual:** en desarrollo

El repositorio ya muestra:

- autenticación funcional conectada al backend
- estructura MVVM base operativa
- navegación inicial entre login y registro
- base lista para la integración de una zona principal con `TabBar`

Debe entenderse como una **base técnica cliente** bien encaminada, todavía en evolución hacia una experiencia móvil más completa.

---

## Roadmap

### Siguientes hitos técnicos

- integración de `TabBar` con vistas principales
- home/dashboard inicial
- flujo de grupos
- pantalla de mapa
- perfil de usuario
- gestión de sesión persistente
- mejora de validaciones
- desacoplo adicional de navegación y estado
- refinamiento visual general
- integración completa con el cálculo de punto de encuentro

---

## Por qué este proyecto tiene valor

Desde un punto de vista de ingeniería frontend/mobile, este proyecto resulta interesante porque no se limita a maquetar pantallas. Trabaja sobre:

- una integración real con backend mediante sockets
- separación clara entre vistas, ViewModels y servicios
- construcción de una base móvil preparada para crecer
- alineación con una solución distribuida real

Es un buen proyecto de portfolio para demostrar trabajo práctico en:

- desarrollo móvil con .NET MAUI
- arquitectura MVVM
- comunicación con backend
- diseño modular
- integración real cliente-servidor

---

## Autor

<p>
  <strong>Sergi Garcia</strong><br />
  Backend Developer especializado en C# / .NET, arquitectura mantenible y sistemas distribuidos.
</p>

<p>
  <a href="https://github.com/SergiByte92">Perfil de GitHub</a>
</p>

---

## Licencia

Uso académico y de portfolio.
