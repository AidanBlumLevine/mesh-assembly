# mesh-assembly

Takes in arbitrary 3D meshes and calculates how they would fit together visually along axis-aligned faces. Then assembles those pieces following those rules and user-inputted weights for different pieces. The algorithm for assembly was inspired by the [wave function collapse algorithm](https://github.com/mxgmn/WaveFunctionCollapse).

Here you can see a few examples of what it can do:

<img src="">

This example used all equal weights:

<img src="https://github.com/AidanBlumLevine/mesh-assembly/blob/d2d7de6525c3ede51a4e3bdad7c0529110b64c64/even_weights.png">

This example was weighted to favor straight pieces:

<img src="https://github.com/AidanBlumLevine/mesh-assembly/blob/d2d7de6525c3ede51a4e3bdad7c0529110b64c64/more_straight.png">

