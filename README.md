﻿# Imp  

**Imp** is a lightweight, sprite-based rendering framework for **F#**, supporting both **OpenGL** (desktop) and **WebGL** (web). Inspired by **Elmish** and **React Native**, it provides a declarative way to build high-performance 2D applications with a functional architecture.  

🔹 **Why "Imp"?** The name comes from an *Imp*, a small mischievous creature—just similar to a sprite! **Imp** is designed to be a fast and lightweight sprite renderer for building graphical applications.  

🚀 **Try the demo**: [davidtme.github.io/imp](https://davidtme.github.io/imp)  

## Features  
- 🎮 **Sprite-Based Rendering** – Optimized for rendering 2D sprites efficiently.  
- 🏗 **Elmish-Like Architecture** – Write declarative UI code, similar to React Native in F#.  
- 🖥 **Cross-Platform Support** – OpenGL for desktop apps and WebGL for browser-based applications.  
- 🎨 **Declarative Scene Composition** – Manage scenes and sprites with a structured, functional approach.  
- 🚀 **GPU-Accelerated Performance** – Leverages OpenGL/WebGL for smooth, high-performance rendering.  
- 📦 **Web-Based & Local Asset Management** – Load and manage textures, sprites, and other resources effortlessly.  
- 💡 **F#-First Development** – Functional, type-safe, and expressive API.  
- 🌐 **Fable Integration** – Uses [Fable](https://fable.io/) to compile F# to JavaScript for seamless web development.  

## Example Usage  
With **Imp**, you can quickly set up a WebGL scene:  

```fsharp
module Client.SimpleWeb.App

// Import required modules for application setup and rendering
open Imp
open Imp.Setup

// Define the name of the texture to be used in the application
let texture = "f-sharp-logo"

// Initialize a data manager for handling web-based data operations
let dataManager = WebDataManager(baseUrl = ".")
// Register the texture name with the data manager
dataManager.AddTextureName(texture)

// Define a function to create the main view of the application
let sampleView () =
    // The view function serves as the root container for the scene
    view () {
        // The sceneRenderer sets up the rendering environment, including the background color
        sceneRenderer (backgroundColor = { R = 29; G = 41; B = 61 }) {
            // The singleRenderer renders individual sprites one at a time
            singleRenderer () { 
                // Define a sprite with its position, dimensions, and texture properties
                sprite (
                    x = 50, // Horizontal position of the sprite
                    y = 50, // Vertical position of the sprite
                    z = 0, // Depth position of the sprite
                    width = 255, // Width of the sprite in pixels
                    height = 255, // Height of the sprite in pixels
                    texture = texture, // Name of the texture to apply to the sprite
                    textureX = 0, // X-coordinate of the texture region to use
                    textureY = 0, // Y-coordinate of the texture region to use
                    textureWidth = 255, // Width of the texture region to use
                    textureHeight = 255 // Height of the texture region to use
                )
            }
        }
    }

// Create a WebGL display for rendering the application
let display = WebGLDisplay(elementId = "App", width = 800, height = 600) // Specify the HTML element and display dimensions
// Attach the data manager, the root view, and an optional per-frame callback to the display
display.AttachView dataManager (sampleView ()) None
// Start the rendering loop for the display
display.Run()

```


## Getting Started

Clone the repository and explore the [sample applications](https://github.com/davidtme/Imp/tree/main/src/Client.SimpleWeb) to see Imp in action!