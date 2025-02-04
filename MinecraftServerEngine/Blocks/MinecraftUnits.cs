using Common;

namespace MinecraftServerEngine.Blocks
{

    // TODO: Suggestion nameing: MinecraftUnits
    public static class MinecraftUnits
    {

        // Blocks

        public const int BlocksInChunkSectionWidth = 16;
        public const int BlocksInChunkSectionHeight = 16;

        public const int ChunkSectionsInChunkWidth = 1;
        public const int ChunkSectionsInChunkHeight = 16;

        public const int ChunksInRegionWidth = 32;
        public const int ChunksInRegionHeight = 1;



        public const int BlocksInChunkSection = BlocksInChunkSectionWidth * BlocksInChunkSectionWidth * BlocksInChunkSectionHeight;
        public const int ChunkSectionsInChunk = ChunkSectionsInChunkWidth * ChunkSectionsInChunkWidth * ChunkSectionsInChunkHeight;
        public const int ChunksInRegion = ChunksInRegionWidth * ChunksInRegionWidth * ChunksInRegionHeight;



        public const int BlocksInChunkWidth = BlocksInChunkSectionWidth * ChunkSectionsInChunkWidth;
        public const int BlocksInChunkHeight = BlocksInChunkSectionHeight * ChunkSectionsInChunkHeight;

        public const int BlocksInRegionWidth = BlocksInChunkWidth * ChunksInRegionWidth;
        public const int BlocksInRegionHeight = BlocksInChunkHeight * ChunksInRegionHeight;

        public const int ChunkSectionsInRegionWidth = ChunkSectionsInChunkWidth * ChunksInRegionWidth;
        public const int ChunkSectionsInRegionHeight = ChunkSectionsInChunkHeight * ChunksInRegionHeight;




        public const double BlockWidth = 1.0;
        public const double BlockHeight = 1.0;

        public const double ChunkSectionWidth = BlockWidth * BlocksInChunkSectionWidth;
        public const double ChunkSectionHeight = BlockHeight * BlocksInChunkSectionHeight;

        public const double ChunkWidth = BlockWidth * BlocksInChunkWidth;
        public const double ChunkHeight = BlockHeight * BlocksInChunkHeight;

        public const double RegionWidth = BlockWidth * BlocksInRegionWidth;
        public const double RegionHeight = BlockHeight * BlocksInRegionHeight;


    }

}
