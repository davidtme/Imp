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