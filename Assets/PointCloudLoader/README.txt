Unity Point Cloud Loader
(C) 2016 Ryan Theriot, Eric Wu, Jack Lam Laboratory for Advanced Visualization & Applications, University of Hawaii at Manoa.
Version: February 17th, 2017

Questions? Email: rtheriot@hawaii.edu 

---Februrary 17th 2017 ---
This project is still in development and this is an experimental release.

To Load a cloud:
Select LoadCloud in the MenuBar at Window>PointClouds

Field Descriptions
Elements Per Data Line: The number of elements per data line in the datafile. This allows the loader to filter out data points the data.
Data Line Delimiter: The delimiter between each element on each data line. Leave blank if white space between element.
X Index: The X index on each data line.
Y Index: The Y index on each data line.
Z Index: The Z index on each data line.
Color Range: NONE - Point Cloud has no color
	     NORMALIZED - Color data is between 0.0 - 1.0
	     RGB - Color data is between 0 - 255
Red Index: The Red index on each data line.
Green Index: The Green index on each data line.
Blue Index: The Blue index on each data line.

For Example:
.pts Format - XYZIRGB
-28.736666 -24.107421 19.453160 113 30 145 170

Elements Per Data Line: 7
Data Line Elimiter: Leave Blank (White Space)
X Index: 1
Y Index: 2
Z Index: 3
Color Range: RGB (0-255)
Red Index: 5
Green Index: 6
Blue Index: 7

Once the point cloud is finished loaded it will be saved as a mesh to the PointClouds folder in your project's Asset folder.
A Prefab and a new material will also be created.


