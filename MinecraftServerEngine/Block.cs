
using Common;

namespace MinecraftServerEngine
{
    public enum Block : int
    {
        Air,
        Stone,
        Granite,
        PolishedGranite,
        Diorite,
        PolishedDiorite,
        Andesite,
        PolishedAndesite,
        GrassBlock,
        Dirt,
        CoarseDirt,
        Podzol,
        Cobblestone,
        OakWoodPlanks,
        SpruceWoodPlanks,
        BirchWoodPlanks,
        JungleWoodPlanks,
        AcaciaWoodPlanks,
        DarkOakWoodPlanks,
        OakSapling,
        SpruceSapling,
        BirchSapling,
        JungleSapling,
        AcaciaSapling,
        DarkOakSapling,
        Bedrock,
        FlowingWater,
        StillWater,
        FlowingLava,
        StillLava,
        Sand,
        RedSand,
        Gravel,
        GoldOre,
        IronOre,
        CoalOre,
        OakWood,
        SpruceWood,
        BirchWood,
        JungleWood,
        OakLeaves,         // 18:0
        SpruceLeaves,
        BirchLeaves,
        JungleLeaves,
        Sponge,
        WetSponge,
        Glass,
        LapisLazuliOre,
        LapisLazuliBlock,
        Dispenser,
        Sandstone,
        ChiseledSandstone,
        SmoothSandstone,
        NoteBlock,        // 25:0


        BottomStickyPiston,     // 29:0
        TopStickyPiston,        // 29:1
        NorthStickyPiston,      // 29:2
        SouthStickyPiston,      // 29:3
        WestStickyPiston,       // 29:4
        EastStickyPiston,       // 29:5

        DeadShrub,        // 31:0
        Grass,            // 31:1
        Fern,             // 31:2
        DeadBush,         // 32:0
        BottomPiston,     // 33:0
        TopPiston,        // 33:1
        NorthPiston,      // 33:2
        SouthPiston,      // 33:3
        WestPiston,       // 33:4
        EastPiston,       // 33:5

        WhiteWool,        // 35:0
        OrangeWool,       // 35:1
        MagentaWool,      // 35:2
        LightBlueWool,    // 35:3
        YellowWool,       // 35:4
        LimeWool,         // 35:5
        PinkWool,         // 35:6
        GrayWool,         // 35:7
        LightGrayWool,    // 35:8
        CyanWool,         // 35:9
        PurpleWool,       // 35:10
        BlueWool,         // 35:11
        BrownWool,        // 35:12
        GreenWool,        // 35:13
        RedWool,          // 35:14
        BlackWool,        // 35:15


        Dandelion,                // 37:0
        Poppy,                    // 38:0
        BlueOrchid,               // 38:1
        Allium,                   // 38:2
        AzureBluet,               // 38:3
        RedTulip,                 // 38:4
        OrangeTulip,              // 38:5
        WhiteTulip,               // 38:6
        PinkTulip,                // 38:7
        OxeyeDaisy,               // 38:8
        BrownMushroom,            // 39:0
        RedMushroom,              // 40:0
        GoldBlock,                // 41:0
        IronBlock,                // 42:0
        DoubleStoneSlab,          // 43:0
        DoubleSandstoneSlab,      // 43:1
        DoubleWoodenSlab,         // 43:2
        DoubleCobblestoneSlab,    // 43:3
        DoubleBrickSlab,          // 43:4
        DoubleStoneBrickSlab,     // 43:5
        DoubleNetherBrickSlab,    // 43:6
        DoubleQuartzSlab,         // 43:7

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

        Bricks,                   // 45:0
        TNT,                      // 46:0
        Bookshelf,                // 47:0
        MossStone,                // 48:0
        Obsidian,                 // 49:0
        Torch,                    // 50:0
        Fire,                     // 51:0

        EastBottomOakWoodStairs,    // 53:0
        WestBottomOakWoodStairs,    // 53:1
        SouthBottomOakWoodStairs,   // 53:2
        NorthBottomOakWoodStairs,   // 53:3
        EastTopOakWoodStairs,       // 53:4
        WestTopOakWoodStairs,       // 53:5
        SouthTopOakWoodStairs,      // 53:6
        NorthTopOakWoodStairs,      // 53:7

        DiamondBlock,             // 57:0
        CraftingTable,            // 58:0
        WheatCrops,               // 59:0
        Farmland,                 // 60:0

        EastBottomCobblestoneStairs,     // 67:0
        WestBottomCobblestoneStairs,     // 67:1
        SouthBottomCobblestoneStairs,    // 67:2
        NorthBottomCobblestoneStairs,    // 67:3
        EastTopCobblestoneStairs,        // 67:4
        WestTopCobblestoneStairs,        // 67:5
        SouthTopCobblestoneStairs,       // 67:6
        NorthTopCobblestoneStairs,       // 67:7


        OakFence,      // 85:0
        SouthPumpkin,  // 86:0
        WestPumpkin,   // 86:1
        NorthPumpkin,  // 86:2
        EastPumpkin,   // 86:3


        WhiteStainedGlass,        // 95:0
        OrangeStainedGlass,       // 95:1
        MagentaStainedGlass,      // 95:2
        LightBlueStainedGlass,    // 95:3
        YellowStainedGlass,       // 95:4
        LimeStainedGlass,         // 95:5
        PinkStainedGlass,         // 95:6
        GrayStainedGlass,         // 95:7
        LightGrayStainedGlass,    // 95:8
        CyanStainedGlass,         // 95:9
        PurpleStainedGlass,       // 95:10
        BlueStainedGlass,         // 95:11
        BrownStainedGlass,        // 95:12
        GreenStainedGlass,        // 95:13
        RedStainedGlass,          // 95:14
        BlackStainedGlass,        // 95:15

        StoneMonsterEgg,                  // 97:0
        CobblestoneMonsterEgg,            // 97:1
        StoneBrickMonsterEgg,             // 97:2
        MossyStoneBrickMonsterEgg,        // 97:3
        CrackedStoneBrickMonsterEgg,      // 97:4
        ChiseledStoneBrickMonsterEgg,     // 97:5
        StoneBricks,                      // 98:0
        MossyStoneBricks,                 // 98:1
        CrackedStoneBricks,               // 98:2
        ChiseledStoneBricks,              // 98:3
        BrownMushroomBlock,               // 99:0
        RedMushroomBlock,                 // 100:0
        IronBars,                         // 101:0
        GlassPane,                        // 102:0
        MelonBlock,                       // 103:0

        InactiveRedstoneLamp,     // 123:0
        ActiveRedstoneLamp,       // 124:0


        DoubleOakWoodSlab,              // 125:0
        DoubleSpruceWoodSlab,           // 125:1
        DoubleBirchWoodSlab,            // 125:2
        DoubleJungleWoodSlab,           // 125:3
        DoubleAcaciaWoodSlab,           // 125:4
        DoubleDarkOakWoodSlab,          // 125:5
        OakWoodSlab,                    // 126:0
        SpruceWoodSlab,                 // 126:1
        BirchWoodSlab,                  // 126:2
        JungleWoodSlab,                 // 126:3
        AcaciaWoodSlab,                 // 126:4
        DarkOakWoodSlab,                // 126:5

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

        WhiteCarpet,       // 171:0
        OrangeCarpet,      // 171:1
        MagentaCarpet,     // 171:2
        LightBlueCarpet,   // 171:3
        YellowCarpet,      // 171:4
        LimeCarpet,        // 171:5
        PinkCarpet,        // 171:6
        GrayCarpet,        // 171:7
        LightGrayCarpet,   // 171:8
        CyanCarpet,        // 171:9
        PurpleCarpet,      // 171:10
        BlueCarpet,        // 171:11
        BrownCarpet,       // 171:12
        GreenCarpet,       // 171:13
        RedCarpet,         // 171:14
        BlackCarpet,       // 171:15



    }

}
