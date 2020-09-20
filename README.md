# SCWE
This tool converts a Survivalcraft world file to ply models

## Usage
For those who aren't familiar with CLI, double click `SCWE.Windows.exe`, and then follow the instruction provided. 

Or you can go with the programmer's way
```
SCWE.Windows.exe [-c chunkx,chunky] [-r radius] [-v vertex_threshhold] [-j thread_count] [-lang en|zh] an_example_world.scworld
```
Where an example usage could be
```
SCWE.Windows.exe [-c 1,2 -r 60 -v 1000000 -j 1 -lang en example_world_name.scworld
```
In this case, the program generates all chunks within 60-chunk from the chunk (1,2), using 1 thread, and seperate the entire model into smaller models with no more than 1,000,000 vertices before writing them to disk. 

All the models will be written to a folder with the same name as the `.scworld` file, under the same directory as the `.scworld` file.
