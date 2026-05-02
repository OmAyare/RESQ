# 🚨 RESQ – Emergency Alert & Live Location Tracking System

RESQ is a mobile safety application built using **.NET MAUI** that enables users to trigger emergency alerts, share real-time location, and notify trusted contacts instantly. The system is designed for **hands-free emergency activation** and reliable communication even in low-connectivity scenarios.

---

## 📱 Features

### 🚨 Emergency Activation

* Trigger emergency using a **single action (button / gesture)**
* Automatically starts tracking and alert workflow
* Works in **foreground service mode (Android)**

### 📍 Live Location Tracking

* Captures GPS location in real-time
* Sends updates at regular intervals
* Generates **Google Maps tracking links**

### 📩 Smart SMS Alerts

* Sends emergency SMS to all saved contacts
* Includes:

  * User name
  * Live tracking link
  * Timestamp
* Works even **without internet (offline fallback)**

### 🌐 Online + Offline Support

* **Online Mode**

  * Syncs data with remote API
  * Maintains session-based tracking
* **Offline Mode**

  * Sends SMS directly
  * Stores events locally (SQLite)
  * Syncs later when internet is available

### 🧠 Session-Based Tracking

* Each emergency creates a unique **Session ID**
* Enables continuous tracking across updates
* Data synced with backend API

### 🗂 Emergency Event History

* Stores all events locally
* Displays past emergency logs in UI

### 🔐 Permissions Handling

* Runtime permissions for:

  * Location
  * SMS
  * Phone access
* Handles GPS OFF scenarios gracefully

---

## 🏗 Tech Stack

### Frontend (Mobile)

* .NET MAUI (v8)
* MVVM Architecture
* CommunityToolkit.Mvvm

### Backend

* .NET Core Web API
* SQL Server

### Local Storage

* SQLite (offline persistence)

### Android Features

* Foreground Services
* SMS Manager / Intent
* Geolocation APIs
* Preferences (state management)

---

## 📂 Project Structure

```
RESQ/
│── Models/
│── Views/
│── ViewModels/
│── Services/
│── Data/
│── Platforms/Android/
│── Helpers/
```

---

## ⚙️ How It Works

1. User triggers emergency
2. App:

   * Fetches GPS location
   * Creates emergency event
3. If **internet available**:

   * Creates session via API
   * Sends tracking link
4. If **offline**:

   * Sends SMS directly
5. Location updates:

   * Sent to API (online)
   * Sent via SMS (offline fallback)
6. When internet restores:

   * Unsynced events are uploaded

---

## 📦 Installation

### Prerequisites

* Visual Studio 2022+
* .NET 8 SDK
* Android SDK

### Steps

```bash
git clone https://github.com/OmAyare/RESQ.git
cd RESQ
```

Open in Visual Studio and run on:

* Android Emulator OR
* Physical Android device

---

## 🔑 Permissions Required

* Location (Fine)
* SMS
* Phone
* Internet
* Network State

---

## 🚀 Future Enhancements

* Real-time live tracking (SignalR / WebSockets)
* Auto send sms without clikcing send buton 
* Voice-triggered SOS
* Wearable device integration
* AI-based emergency detection

---

## 🤝 Contribution

Contributions are welcome. Feel free to fork the repo and submit a pull request.


---

## 📬 Contact

For queries or suggestions, feel free to reach out via GitHub Issues.

---

⭐ If you find this project useful, consider giving it a star!
