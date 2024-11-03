# COSC 441 Project Implementation

This is a repo for the implementation of a Pie Menu interaction technique to combat the issues of high movement time and inaccuracy when using regular point-cursors on large displays. Typically, larger displays means that targets are clustered together in zones, and the clusters themselves are sparsely distributed (perhaps in corners of the screen).

## Pie Menu description

Pressing an arbitrary button (like scroll wheel or some uncommon combination of mouse button presses) brings up a "pie"-like menu surrounding the cursor. The menu contains options for different "zones" on the screen, which could be determined algorithmically or simply corresponding to quadrants of the screen. The menu also contains the last 3 recently used apps. Selecting any of the zones teleports the cursor to the centroid of that zone, after which the cursor operates as normal. Selecting any of the "apps" in the pie menu instantly selects the respective target on the screen.

Add pictures here.

## Techniques that will be compared in this study

We will compare the point cursor to the pie menu.

## Instructions on how to use these project files

1. Ensure you have Unity Editor version: 2022.3.39.f1, otherwise there might be some issues with opening the project in Unity.
2. Clone this repo onto your system.
3. Open Unity Hub.
4. Click on "Add" > "Add project from disk", and select the folder where you cloned the repo.
