
using Common;

namespace MinecraftServerEngine
{
    public enum Block : int
    {
        Air,                            // 0:0
        Stone,                          // 1:0
        Granite,                        // 1:1
        PolishedGranite,                // 1:2
        Diorite,                        // 1:3
        PolishedDiorite,                // 1:4
        Andesite,                       // 1:5
        PolishedAndesite,               // 1:6
        GrassBlock,                     // 2:0
        Dirt,                           // 3:0
        CoarseDirt,                     // 3:1
        Podzol,                         // 3:2
        Cobblestone,                    // 4:0
        OakWoodPlanks,                  // 5:0
        SpruceWoodPlanks,               // 5:1
        BirchWoodPlanks,                // 5:2
        JungleWoodPlanks,               // 5:3
        AcaciaWoodPlanks,               // 5:4
        DarkOakWoodPlanks,              // 5:5
        OakSapling,                     // 6:0
        SpruceSapling,                  // 6:1
        BirchSapling,                   // 6:2
        JungleSapling,                  // 6:3
        AcaciaSapling,                  // 6:4
        DarkOakSapling,                 // 6:5
        Bedrock,                        // 7:0
        FlowingWater,                   // 8:0
        StillWater,                     // 9:0
        FlowingLava,                    // 10:0
        StillLava,                      // 11:0
        Sand,                           // 12:0
        RedSand,                        // 12:1
        Gravel,                         // 13:0
        GoldOre,                        // 14:0
        IronOre,                        // 15:0
        CoalOre,                        // 16:0
        OakWood,                        // 17:0
        SpruceWood,                     // 17:1
        BirchWood,                      // 17:2
        JungleWood,                     // 17:3
        OakLeaves,                      // 18:0
        SpruceLeaves,                   // 18:1
        BirchLeaves,                    // 18:2
        JungleLeaves,                   // 18:3
        Sponge,                         // 19:0
        WetSponge,                      // 19:1
        Glass,                          // 20:0
        LapisLazuliOre,                 // 21:0
        LapisLazuliBlock,               // 22:0

        BottomDispenser,                // 23:0
        TopDispenser,                   // 23:1
        NorthDispenser,                 // 23:2
        SouthDispenser,                 // 23:3
        WestDispenser,                  // 23:4
        EastDispenser,                  // 23:5

        Sandstone,                      // 24:0
        ChiseledSandstone,              // 24:1
        SmoothSandstone,                // 24:2
        NoteBlock,                      // 25:0


        BottomStickyPiston,             // 29:0
        TopStickyPiston,                // 29:1
        NorthStickyPiston,              // 29:2
        SouthStickyPiston,              // 29:3
        WestStickyPiston,               // 29:4
        EastStickyPiston,               // 29:5

        DeadShrub,                      // 31:0
        Grass,                          // 31:1
        Fern,                           // 31:2
        DeadBush,                       // 32:0
        BottomPiston,                   // 33:0
        TopPiston,                      // 33:1
        NorthPiston,                    // 33:2
        SouthPiston,                    // 33:3
        WestPiston,                     // 33:4
        EastPiston,                     // 33:5

        WhiteWool,                      // 35:0
        OrangeWool,                     // 35:1
        MagentaWool,                    // 35:2
        LightBlueWool,                  // 35:3
        YellowWool,                     // 35:4
        LimeWool,                       // 35:5
        PinkWool,                       // 35:6
        GrayWool,                       // 35:7
        LightGrayWool,                  // 35:8
        CyanWool,                       // 35:9
        PurpleWool,                     // 35:10
        BlueWool,                       // 35:11
        BrownWool,                      // 35:12
        GreenWool,                      // 35:13
        RedWool,                        // 35:14
        BlackWool,                      // 35:15

        Dandelion,                      // 37:0
        Poppy,                          // 38:0
        BlueOrchid,                     // 38:1
        Allium,                         // 38:2
        AzureBluet,                     // 38:3
        RedTulip,                       // 38:4
        OrangeTulip,                    // 38:5
        WhiteTulip,                     // 38:6
        PinkTulip,                      // 38:7
        OxeyeDaisy,                     // 38:8
        BrownMushroom,                  // 39:0
        RedMushroom,                    // 40:0
        GoldBlock,                      // 41:0
        IronBlock,                      // 42:0
        DoubleStoneSlab,                // 43:0
        DoubleSandstoneSlab,            // 43:1
        DoubleWoodenSlab,               // 43:2
        DoubleCobblestoneSlab,          // 43:3
        DoubleBrickSlab,                // 43:4
        DoubleStoneBrickSlab,           // 43:5
        DoubleNetherBrickSlab,          // 43:6
        DoubleQuartzSlab,               // 43:7

        StoneBottomSlab,                // 44:0
        SandstoneBottomSlab,            // 44:1
        WoodenBottomSlab,               // 44:2
        CobblestoneBottomSlab,          // 44:3
        BrickBottomSlab,                // 44:4
        StoneBrickBottomSlab,           // 44:5
        NetherBrickBottomSlab,          // 44:6
        QuartzBottomSlab,               // 44:7
        StoneTopSlab,                   // 44:8
        SandstoneTopSlab,               // 44:9
        WoodenTopSlab,                  // 44:10
        CobblestoneTopSlab,             // 44:11
        BrickTopSlab,                   // 44:12
        StoneBrickTopSlab,              // 44:13
        NetherBrickTopSlab,             // 44:14
        QuartzTopSlab,                  // 44:15

        Bricks,                         // 45:0
        TNT,                            // 46:0
        Bookshelf,                      // 47:0
        MossStone,                      // 48:0
        Obsidian,                       // 49:0
        
        //Torch,                          // 50:0  minecraft:torch
        EastTorch,                      // 50:1  minecraft:torch
        WestTorch,                      // 50:2  minecraft:torch
        SouthTorch,                     // 50:3  minecraft:torch
        NorthTorch,                     // 50:4  minecraft:torch
        Torch,                          // 50:5  minecraft:torch

        Fire,                           // 51:0

        EastBottomOakWoodStairs,        // 53:0
        WestBottomOakWoodStairs,        // 53:1
        SouthBottomOakWoodStairs,       // 53:2
        NorthBottomOakWoodStairs,       // 53:3
        EastTopOakWoodStairs,           // 53:4
        WestTopOakWoodStairs,           // 53:5
        SouthTopOakWoodStairs,          // 53:6
        NorthTopOakWoodStairs,          // 53:7

        DiamondBlock,                   // 57:0
        CraftingTable,                  // 58:0
        WheatCrops,                     // 59:0
        Farmland,                       // 60:0

        EastBottomCobblestoneStairs,     // 67:0
        WestBottomCobblestoneStairs,     // 67:1
        SouthBottomCobblestoneStairs,    // 67:2
        NorthBottomCobblestoneStairs,    // 67:3
        EastTopCobblestoneStairs,        // 67:4
        WestTopCobblestoneStairs,        // 67:5
        SouthTopCobblestoneStairs,       // 67:6
        NorthTopCobblestoneStairs,       // 67:7


        OakFence,                       // 85:0
        SouthPumpkin,                   // 86:0
        WestPumpkin,                    // 86:1
        NorthPumpkin,                   // 86:2
        EastPumpkin,                    // 86:3


        SouthJackOLantern,              // 86:0  minecraft:lit_pumpkin
        WestJackOLantern,               // 86:1  minecraft:lit_pumpkin
        NorthJackOLantern,              // 86:2  minecraft:lit_pumpkin
        EastJackOLantern,               // 86:3  minecraft:lit_pumpkin

        WhiteStainedGlass,              // 95:0
        OrangeStainedGlass,             // 95:1
        MagentaStainedGlass,            // 95:2
        LightBlueStainedGlass,          // 95:3
        YellowStainedGlass,             // 95:4
        LimeStainedGlass,               // 95:5
        PinkStainedGlass,               // 95:6
        GrayStainedGlass,               // 95:7
        LightGrayStainedGlass,          // 95:8
        CyanStainedGlass,               // 95:9
        PurpleStainedGlass,             // 95:10
        BlueStainedGlass,               // 95:11
        BrownStainedGlass,              // 95:12
        GreenStainedGlass,              // 95:13
        RedStainedGlass,                // 95:14
        BlackStainedGlass,              // 95:15

        StoneMonsterEgg,                // 97:0
        CobblestoneMonsterEgg,          // 97:1
        StoneBrickMonsterEgg,           // 97:2
        MossyStoneBrickMonsterEgg,      // 97:3
        CrackedStoneBrickMonsterEgg,    // 97:4
        ChiseledStoneBrickMonsterEgg,   // 97:5
        StoneBricks,                    // 98:0
        MossyStoneBricks,               // 98:1
        CrackedStoneBricks,             // 98:2
        ChiseledStoneBricks,            // 98:3
        BrownMushroomBlock,             // 99:0
        RedMushroomBlock,               // 100:0


        IronBars,                       // 101:0
        GlassPane,                      // 102:0
        MelonBlock,                     // 103:0

        Vines,                          // 106:0  minecraft:vine
        SouthVines,                     // 106:1  minecraft:vine
        WestVines,                      // 106:2  minecraft:vine
        SouthWestVines,                 // 106:3  minecraft:vine
        NorthVines,                     // 106:4  minecraft:vine 
        NorthSouthVines,                // 106:5  minecraft:vine
        NorthWestVines,                 // 106:6  minecraft:vine
        NorthSouthWestVines,            // 106:7  minecraft:vine
        EastVines,                      // 106:8  minecraft:vine
        EastSouthVines,                 // 106:9  minecraft:vine
        EastWestVines,                  // 106:10  minecraft:vine
        EastSouthWestVines,             // 106:11  minecraft:vine
        EastNorthVines,                 // 106:12  minecraft:vine
        EastNorthSouthVines,            // 106:13  minecraft:vine
        EastNorthWestVines,             // 106:14  minecraft:vine 
        EastNorthSouthWestVines,        // 106:15  minecraft:vine

        EastBottomStoneBrickStairs,     // 109:0  minecraft:stone_brick_stairs
        WestBottomStoneBrickStairs,     // 109:1  minecraft:stone_brick_stairs
        SouthBottomStoneBrickStairs,    // 109:2  minecraft:stone_brick_stairs
        NorthBottomStoneBrickStairs,    // 109:3  minecraft:stone_brick_stairs
        EastTopStoneBrickStairs,        // 109:4  minecraft:stone_brick_stairs
        WestTopStoneBrickStairs,        // 109:5  minecraft:stone_brick_stairs
        SouthTopStoneBrickStairs,       // 109:6  minecraft:stone_brick_stairs
        NorthTopStoneBrickStairs,       // 109:7  minecraft:stone_brick_stairs

        InactiveRedstoneLamp,           // 123:0
        ActiveRedstoneLamp,             // 124:0


        DoubleOakWoodSlab,              // 125:0
        DoubleSpruceWoodSlab,           // 125:1
        DoubleBirchWoodSlab,            // 125:2
        DoubleJungleWoodSlab,           // 125:3
        DoubleAcaciaWoodSlab,           // 125:4
        DoubleDarkOakWoodSlab,          // 125:5

        BottomOakWoodSlab,              // 126:0  minecraft:wooden_slab
        BottomSpruceWoodSlab,           // 126:1  minecraft:wooden_slab
        BottomBirchWoodSlab,            // 126:2  minecraft:wooden_slab
        BottomJungleWoodSlab,           // 126:3  minecraft:wooden_slab
        BottomAcaciaWoodSlab,           // 126:4  minecraft:wooden_slab
        BottomDarkOakWoodSlab,          // 126:5  minecraft:wooden_slab
        TopOakWoodSlab,                 // 126:8  minecraft:wooden_slab
        TopSpruceWoodSlab,              // 126:9  minecraft:wooden_slab
        TopBirchWoodSlab,               // 126:10  minecraft:wooden_slab
        TopJungleWoodSlab,              // 126:11  minecraft:wooden_slab
        TopAcaciaWoodSlab,              // 126:12  minecraft:wooden_slab
        TopDarkOakWoodSlab,             // 126:13  minecraft:wooden_slab

        EastBottomSandstoneStairs,      // 128:0
        WestBottomSandstoneStairs,      // 128:1
        SouthBottomSandstoneStairs,     // 128:2
        NorthBottomSandstoneStairs,     // 128:3
        EastTopSandstoneStairs,         // 128:4
        WestTopSandstoneStairs,         // 128:5
        SouthTopSandstoneStairs,        // 128:6
        NorthTopSandstoneStairs,        // 128:7

        EmeraldBlock,                   // 133:0

        EastBottomSpruceWoodStairs,     // 134:0
        WestBottomSpruceWoodStairs,     // 134:1
        SouthBottomSpruceWoodStairs,    // 134:2
        NorthBottomSpruceWoodStairs,    // 134:3
        EastTopSpruceWoodStairs,        // 134:4
        WestTopSpruceWoodStairs,        // 134:5
        SouthTopSpruceWoodStairs,       // 134:6
        NorthTopSpruceWoodStairs,       // 134:7
        
        EastBottomBirchWoodStairs,      // 135:0
        WestBottomBirchWoodStairs,      // 135:1
        SouthBottomBirchWoodStairs,     // 135:2
        NorthBottomBirchWoodStairs,     // 135:3
        EastTopBirchWoodStairs,         // 135:4
        WestTopBirchWoodStairs,         // 135:5
        SouthTopBirchWoodStairs,        // 135:6
        NorthTopBirchWoodStairs,        // 135:7

        EastBottomJungleWoodStairs,     // 136:0
        WestBottomJungleWoodStairs,     // 136:1
        SouthBottomJungleWoodStairs,    // 136:2
        NorthBottomJungleWoodStairs,    // 136:3
        EastTopJungleWoodStairs,        // 136:4
        WestTopJungleWoodStairs,        // 136:5
        SouthTopJungleWoodStairs,       // 136:6
        NorthTopJungleWoodStairs,       // 136:7

        CobblestoneWall,                // 139:0
        MossyCobblestoneWall,           // 139:1

        RedstoneBlock,                  // 152:0  minecraft:redstone_block

        HayBale_AxisY,                  // 170:0  minecraft:hay_block
        HayBale_AxisX,                  // 170:4  minecraft:hay_block
        HayBale_AxisZ,                  // 170:8  minecraft:hay_block

        WhiteCarpet,                    // 171:0
        OrangeCarpet,                   // 171:1
        MagentaCarpet,                  // 171:2
        LightBlueCarpet,                // 171:3
        YellowCarpet,                   // 171:4
        LimeCarpet,                     // 171:5
        PinkCarpet,                     // 171:6
        GrayCarpet,                     // 171:7
        LightGrayCarpet,                // 171:8
        CyanCarpet,                     // 171:9
        PurpleCarpet,                   // 171:10
        BlueCarpet,                     // 171:11
        BrownCarpet,                    // 171:12
        GreenCarpet,                    // 171:13
        RedCarpet,                      // 171:14
        BlackCarpet,                    // 171:15



    }

}
