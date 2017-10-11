# csgeom
Computational Geometry

The end goal of this project is to produce a computational geometry library written in C#.

The high level goals of this project ARE:
  -Provide reasonable approximations for geometry problems
  -Never produce degenerate output from non-degenerate input
  -Be fast enough for real time applications (not as a fast as a full physics engine)
  
The high level goals of this project are NOT:
  -Provide numerically accurate results
  -Accept degenerate input
  -Use optimal-time-complexity algorithms (unless they are easy to implement)

There are a number of problems this library aims to solve, divided into categories;
Problems are sorted roughly by their difficulty.

  2D Statics:
    -Describe a vertex in 2 dimensional space
    -Vertices can store additional vector parameters (such as density, temperature, or uv coordinates)
    -Transform a vertex with matrix multiplication (scale, translate, rotate, shear)

    -Describe a line segment composed of 2 vertices
    -For a 3rd point lying on the line, interpolate all vector parametrs using some function (very important for graphics)

    -Describe a string of line segments, also known as a discretized curve

    -Describe a triangle
    -Calculate the area of a triangle
    -Calculate the center of area of a triangle
    -Calculate the area moment of inertia of a triangle about a given point

    -Describe a closed loop of line segments, also known as a polygon
    -Determine if a polygon is simple
    -Determine if a polygon is convex or concave
    -Describe a simple polygon with holes
    -Convert a polygon with holes int a weakly simple polygon
    -Decompose a simple polygon into triangles
    -Determine if a point is inside the solid area of a polygon with holes

    -Calculate the intersection of two line segments' hyperplanes
    -Project a point onto a line segment's hyperplane
    -Perform boolean operations on polygons with holes (CSG)
    
  
  These functions provide the ability to fully describe a solid 2D object and interact with it in various ways.
  
  
  3D Statics:
    -Everything above but in 3D
    
    -Describe 
    
