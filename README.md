Project Status: Preliminary Design Stage

# csgeom: Computational Geometry in C#

---

### The end goal of this project is to produce a computational geometry library written in C#.

### The high level goals of this project ARE:
  * Never produce degenerate output from non-degenerate input
  * Be fast enough for real time applications
  
### The high level goals of this project are NOT:
  * Accept degenerate input
  * Use optimal-time-complexity algorithms
  
---

### There are a number of problems this library aims to solve:

### In 2D:

  * Describe a simple polygon with holes (referred to as a polygon)
  * Triangulate a polygon
  * Find area of a polygon
  * Find center of area of a polygon
  * Find area moment of inertia of a polygon about a point
  * Perform boolean operations on polygons
  * Transform a polygon with matrix multiplication (translate, scale, shear, rotate)
  
### In 3D:

  * Describe a hyperplane
  * Find the intersection of a line and a hyperplane
  * Project points onto hyperplane
  * Describe a manifold solid with polygon faces (referred to as a solid)
  * Tetrahedronate a solid
  * Find the volume of a solid
  * Find the center of volume of a solid
  * Find the moment of inertia of a solid about an axis
  * Perform boolean operations on solids
  * Transform a solid with matrix multiplication (translate, scale, shear, rotate)
  
### Dynamics:

  * Find the velocity of a point in a rotating coordinate system
  * Find the acceleration of a point in a rotating coordinate system

### Generic:
  * Find shortest path between two points inside a polygon or solid
 
### Other (less important and easier) problems:

  * Find exact solutions to line integrals of polynomial vector fields over discrete curves (line segment chains)
  * Find exact solutions to area integrals of polynomial fields over polygons
  * Find exact solutions to volume integrals of polynomial fields over solids
  * Find approximate solutions to line, area, and volume integrals of fields over their respective geometry
  * Find exact solutions to line, area, and volume integrals of discrete fields over their respective geometry
  
  
  
  
  
