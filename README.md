# Imp

Check out the demo at https://davidtme.github.io/imp

## Sample code:

``` F#
let sampleView () =
    view () {
        sceneRenderer (backgroundColor = { R = 30; G = 30; B = 30 }) {
            singleRenderer () {
                sprite (
                    x = 50,
                    y = 50,
                    z = 0,
                    width = 32,
                    height = 32,
                    texture = "mega",
                    textureX = 563,
                    textureY = 791,
                    textureWidth = 32,
                    textureHeight = 32
                )
            }
        }
    }
```