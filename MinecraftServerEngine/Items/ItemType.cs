namespace MinecraftServerEngine.Items
{
    public enum ItemType : int
    {
        Stone,                     // 1:0  minecraft:stone
        Granite,                   // 1:1  minecraft:stone
        PolishedGranite,           // 1:2  minecraft:stone
        Diorite,                   // 1:3  minecraft:stone
        PolishedDiorite,           // 1:4  minecraft:stone
        Andesite,                  // 1:5  minecraft:stone
        PolishedAndesite,          // 1:6  minecraft:stone
        GrassBlock,                // 2:0  minecraft:grass
        Dirt,                      // 3:0  minecraft:dirt
        CoarseDirt,                // 3:1  minecraft:dirt
        Podzol,                    // 3:2  minecraft:dirt
        Cobblestone,               // 4:0  minecraft:cobblestone
        OakWoodPlanks,             // 5:0  minecraft:planks
        SpruceWoodPlanks,          // 5:1  minecraft:planks
        BirchWoodPlanks,           // 5:2  minecraft:planks
        JungleWoodPlanks,          // 5:3  minecraft:planks
        AcaciaWoodPlanks,          // 5:4  minecraft:planks
        DarkOakWoodPlanks,         // 5:5  minecraft:planks
        OakSapling,                // 6:0  minecraft:sapling
        SpruceSapling,             // 6:1  minecraft:sapling
        BirchSapling,              // 6:2  minecraft:sapling
        JungleSapling,             // 6:3  minecraft:sapling
        AcaciaSapling,             // 6:4  minecraft:sapling
        DarkOakSapling,            // 6:5  minecraft:sapling
        Bedrock,                   // 7:0  minecraft:bedrock
        FlowingWater,              // 8:0  minecraft:flowing_water
        StillWater,                // 9:0  minecraft:water
        FlowingLava,               // 10:0  minecraft:flowing_lava
        StillLava,                 // 11:0  minecraft:lava
        Sand,                      // 12:0  minecraft:sand
        RedSand,                   // 12:1  minecraft:sand
        Gravel,                    // 13:0  minecraft:gravel
        GoldOre,                   // 14:0  minecraft:gold_ore
        IronOre,                   // 15:0  minecraft:iron_ore
        CoalOre,                   // 16:0  minecraft:coal_ore
        OakWood,                   // 17:0  minecraft:log
        SpruceWood,                // 17:1  minecraft:log
        BirchWood,                 // 17:2  minecraft:log
        JungleWood,                // 17:3  minecraft:log
        OakLeaves,                 // 18:0  minecraft:leaves
        SpruceLeaves,              // 18:1  minecraft:leaves
        BirchLeaves,               // 18:2  minecraft:leaves
        JungleLeaves,              // 18:3  minecraft:leaves
        Sponge,                    // 19:0  minecraft:sponge
        WetSponge,                 // 19:1  minecraft:sponge
        Glass,                     // 20:0  minecraft:glass
        LapisLazuliOre,            // 21:0  minecraft:lapis_ore
        LapisLazuliBlock,          // 22:0  minecraft:lapis_block

        Dispenser,                 // 23:0  minecraft:dispenser

        Sandstone,                 // 24:0  minecraft:sandstone
        ChiseledSandstone,         // 24:1  minecraft:sandstone
        SmoothSandstone,           // 24:2  minecraft:sandstone
        NoteBlock,                 // 25:0  minecraft:noteblock

        StickyPiston,                    // 29:0  minecraft:sticky_piston

        DeadShrub,                 // 31:0  minecraft:tallgrass
        Grass,                     // 31:1  minecraft:tallgrass
        Fern,                      // 31:2  minecraft:tallgrass
        DeadBush,                  // 32:0  minecraft:deadbush

        Piston,                    // 33:0  minecraft:piston

        WhiteWool,                 // 35:0  minecraft:wool
        OrangeWool,                // 35:1  minecraft:wool
        MagentaWool,               // 35:2  minecraft:wool
        LightBlueWool,             // 35:3  minecraft:wool
        YellowWool,                // 35:4  minecraft:wool
        LimeWool,                  // 35:5  minecraft:wool
        PinkWool,                  // 35:6  minecraft:wool
        GrayWool,                  // 35:7  minecraft:wool
        LightGrayWool,             // 35:8  minecraft:wool
        CyanWool,                  // 35:9  minecraft:wool
        PurpleWool,                // 35:10  minecraft:wool
        BlueWool,                  // 35:11  minecraft:wool
        BrownWool,                 // 35:12  minecraft:wool
        GreenWool,                 // 35:13  minecraft:wool
        RedWool,                   // 35:14  minecraft:wool
        BlackWool,                 // 35:15  minecraft:wool

        Dandelion,                 // 37:0  minecraft:yellow_flower
        Poppy,                     // 38:0  minecraft:red_flower
        BlueOrchid,                // 38:1  minecraft:red_flower
        Allium,                    // 38:2  minecraft:red_flower
        AzureBluet,                // 38:3  minecraft:red_flower
        RedTulip,                  // 38:4  minecraft:red_flower
        OrangeTulip,               // 38:5  minecraft:red_flower
        WhiteTulip,                // 38:6  minecraft:red_flower
        PinkTulip,                 // 38:7  minecraft:red_flower
        OxeyeDaisy,                // 38:8  minecraft:red_flower
        BrownMushroom,             // 39:0  minecraft:brown_mushroom
        RedMushroom,               // 40:0  minecraft:red_mushroom
        GoldBlock,                 // 41:0  minecraft:gold_block
        IronBlock,                 // 42:0  minecraft:iron_block
        DoubleStoneSlab,           // 43:0  minecraft:double_stone_slab
        DoubleSandstoneSlab,       // 43:1  minecraft:double_stone_slab
        DoubleWoodenSlab,          // 43:2  minecraft:double_stone_slab
        DoubleCobblestoneSlab,     // 43:3  minecraft:double_stone_slab
        DoubleBrickSlab,           // 43:4  minecraft:double_stone_slab
        DoubleStoneBrickSlab,      // 43:5  minecraft:double_stone_slab
        DoubleNetherBrickSlab,     // 43:6  minecraft:double_stone_slab
        DoubleQuartzSlab,          // 43:7  minecraft:double_stone_slab

        StoneBottomSlab,           // 44:0  minecraft:stone_slab
        SandstoneBottomSlab,       // 44:1  minecraft:stone_slab
        WoodenBottomSlab,          // 44:2  minecraft:stone_slab
        CobblestoneBottomSlab,     // 44:3  minecraft:stone_slab
        BrickBottomSlab,           // 44:4  minecraft:stone_slab
        StoneBrickBottomSlab,      // 44:5  minecraft:stone_slab
        NetherBrickBottomSlab,     // 44:6  minecraft:stone_slab
        QuartzBottomSlab,          // 44:7  minecraft:stone_slab
        StoneTopSlab,              // 44:8  minecraft:stone_slab
        SandstoneTopSlab,          // 44:9  minecraft:stone_slab
        WoodenTopSlab,             // 44:10  minecraft:stone_slab
        CobblestoneTopSlab,        // 44:11  minecraft:stone_slab
        BrickTopSlab,              // 44:12  minecraft:stone_slab
        StoneBrickTopSlab,         // 44:13  minecraft:stone_slab
        NetherBrickTopSlab,        // 44:14  minecraft:stone_slab
        QuartzTopSlab,             // 44:15  minecraft:stone_slab

        Bricks,                    // 45:0  minecraft:brick_block
        TNT,                       // 46:0  minecraft:tnt
        Bookshelf,                 // 47:0  minecraft:bookshelf
        MossStone,                 // 48:0  minecraft:mossy_cobblestone
        Obsidian,                  // 49:0  minecraft:obsidian

        Torch,                     // 50:0  minecraft:torch

        OakWoodStairs,             // 53:0  minecraft:oak_stairs

        DiamondBlock,              // 57:0  minecraft:diamond_block
        CraftingTable,             // 58:0  minecraft:crafting_table
        WheatCrops,                // 59:0  minecraft:wheat
        Farmland,                  // 60:0  minecraft:farmland

        CobblestoneStairs,         // 67:0  minecraft:stone_stairs

        RedstoneOre,               // 73:0  minecraft:redstone_ore
        // It is not shown in client...
        // It is only used for block.
        //GlowingRedstoneOre,        // 74:0  minecraft:lit_redstone_ore

        OakFence,                  // 85:0  minecraft:fence

        Pumpkin,                   // 86:0  minecraft:pumpkin

        JackOLantern,              // 91:0  minecraft:lit_pumpkin

        WhiteStainedGlass,              // 95:0  minecraft:stained_glass
        OrangeStainedGlass,             // 95:1  minecraft:stained_glass
        MagentaStainedGlass,            // 95:2  minecraft:stained_glass
        LightBlueStainedGlass,          // 95:3  minecraft:stained_glass
        YellowStainedGlass,             // 95:4  minecraft:stained_glass
        LimeStainedGlass,               // 95:5  minecraft:stained_glass
        PinkStainedGlass,               // 95:6  minecraft:stained_glass
        GrayStainedGlass,               // 95:7  minecraft:stained_glass
        LightGrayStainedGlass,          // 95:8  minecraft:stained_glass
        CyanStainedGlass,               // 95:9  minecraft:stained_glass
        PurpleStainedGlass,             // 95:10  minecraft:stained_glass
        BlueStainedGlass,               // 95:11  minecraft:stained_glass
        BrownStainedGlass,              // 95:12  minecraft:stained_glass
        GreenStainedGlass,              // 95:13  minecraft:stained_glass
        RedStainedGlass,                // 95:14  minecraft:stained_glass
        BlackStainedGlass,              // 95:15  minecraft:stained_glass

        StoneMonsterEgg,                // 97:0  minecraft:monster_egg
        CobblestoneMonsterEgg,          // 97:1  minecraft:monster_egg
        StoneBrickMonsterEgg,           // 97:2  minecraft:monster_egg
        MossyStoneBrickMonsterEgg,      // 97:3  minecraft:monster_egg
        CrackedStoneBrickMonsterEgg,    // 97:4  minecraft:monster_egg
        ChiseledStoneBrickMonsterEgg,   // 97:5  minecraft:monster_egg

        StoneBricks,                    // 98:0  minecraft:stonebrick
        MossyStoneBricks,               // 98:1  minecraft:stonebrick
        CrackedStoneBricks,             // 98:2  minecraft:stonebrick
        ChiseledStoneBricks,            // 98:3  minecraft:stonebrick
        BrownMushroomBlock,             // 99:0  minecraft:brown_mushroom_block
        RedMushroomBlock,               // 100:0  minecraft:red_mushroom_block

        IronBars,                       // 101:0  minecraft:iron_bars
        GlassPane,                      // 102:0  minecraft:glass_pane
        MelonBlock,                     // 103:0  minecraft:melon_block

        Vines,                          // 106:0  minecraft:vine

        StoneBrickStairs,               // 109:0  minecraft:stone_brick_stairs

        InactiveRedstoneLamp,           // 123:0  minecraft:redstone_lamp

        // It is not working...
        //ActiveRedstoneLamp,             // 124:0  minecraft:lit_redstone_lamp

        DoubleOakWoodSlab,              // 125:0  minecraft:double_wooden_slab
        DoubleSpruceWoodSlab,           // 125:1  minecraft:double_wooden_slab
        DoubleBirchWoodSlab,            // 125:2  minecraft:double_wooden_slab
        DoubleJungleWoodSlab,           // 125:3  minecraft:double_wooden_slab
        DoubleAcaciaWoodSlab,           // 125:4  minecraft:double_wooden_slab
        DoubleDarkOakWoodSlab,          // 125:5  minecraft:double_wooden_slab

        OakWoodSlab,                    // 126:0  minecraft:wooden_slab

        SandstoneStairs,                // 128:0  minecraft:sandstone_stairs

        EmeraldBlock,                   // 133:0  minecraft:emerald_block

        SpruceWoodStairs,               // 134:0  minecraft:spruce_stairs
        BirchWoodStairs,                // 135:0  minecraft:birch_stairs
        JungleWoodStairs,               // 136:0  minecraft:jungle_stairs


        CobblestoneWall,                // 139:0  minecraft:cobblestone_wall
        MossyCobblestoneWall,           // 139:1  minecraft:cobblestone_wall

        RedstoneBlock,                  // 152:0  minecraft:redstone_block


        WhiteTerracotta,           // 159:0  minecraft:stained_hardened_clay
        OrangeTerracotta,          // 159:1  minecraft:stained_hardened_clay
        MagentaTerracotta,         // 159:2  minecraft:stained_hardened_clay
        LightBlueTerracotta,       // 159:3  minecraft:stained_hardened_clay
        YellowTerracotta,          // 159:4  minecraft:stained_hardened_clay
        LimeTerracotta,            // 159:5  minecraft:stained_hardened_clay
        PinkTerracotta,            // 159:6  minecraft:stained_hardened_clay
        GrayTerracotta,            // 159:7  minecraft:stained_hardened_clay
        LightGrayTerracotta,       // 159:8  minecraft:stained_hardened_clay
        CyanTerracotta,            // 159:9  minecraft:stained_hardened_clay
        PurpleTerracotta,          // 159:10  minecraft:stained_hardened_clay
        BlueTerracotta,            // 159:11  minecraft:stained_hardened_clay
        BrownTerracotta,           // 159:12  minecraft:stained_hardened_clay
        GreenTerracotta,           // 159:13  minecraft:stained_hardened_clay
        RedTerracotta,             // 159:14  minecraft:stained_hardened_clay
        BlackTerracotta,           // 159:15  minecraft:stained_hardened_clay

        WhiteStainedGlassPane,     // 160:0  minecraft:stained_glass_pane
        OrangeStainedGlassPane,    // 160:1  minecraft:stained_glass_pane
        MagentaStainedGlassPane,   // 160:2  minecraft:stained_glass_pane
        LightBlueStainedGlassPane, // 160:3  minecraft:stained_glass_pane
        YellowStainedGlassPane,    // 160:4  minecraft:stained_glass_pane
        LimeStainedGlassPane,      // 160:5  minecraft:stained_glass_pane
        PinkStainedGlassPane,      // 160:6  minecraft:stained_glass_pane
        GrayStainedGlassPane,      // 160:7  minecraft:stained_glass_pane
        LightGrayStainedGlassPane, // 160:8  minecraft:stained_glass_pane
        CyanStainedGlassPane,      // 160:9  minecraft:stained_glass_pane
        PurpleStainedGlassPane,    // 160:10  minecraft:stained_glass_pane
        BlueStainedGlassPane,      // 160:11  minecraft:stained_glass_pane
        BrownStainedGlassPane,     // 160:12  minecraft:stained_glass_pane
        GreenStainedGlassPane,     // 160:13  minecraft:stained_glass_pane
        RedStainedGlassPane,       // 160:14  minecraft:stained_glass_pane
        BlackStainedGlassPane,     // 160:15  minecraft:stained_glass_pane

        HayBale,                        // 170:0  minecraft:hay_block

        WhiteCarpet,                    // 171:0  minecraft:carpet
        OrangeCarpet,                   // 171:1  minecraft:carpet
        MagentaCarpet,                  // 171:2  minecraft:carpet
        LightBlueCarpet,                // 171:3  minecraft:carpet
        YellowCarpet,                   // 171:4  minecraft:carpet
        LimeCarpet,                     // 171:5  minecraft:carpet
        PinkCarpet,                     // 171:6  minecraft:carpet
        GrayCarpet,                     // 171:7  minecraft:carpet
        LightGrayCarpet,                // 171:8  minecraft:carpet
        CyanCarpet,                     // 171:9  minecraft:carpet
        PurpleCarpet,                   // 171:10  minecraft:carpet
        BlueCarpet,                     // 171:11  minecraft:carpet
        BrownCarpet,                    // 171:12  minecraft:carpet
        GreenCarpet,                    // 171:13  minecraft:carpet
        RedCarpet,                      // 171:14  minecraft:carpet
        BlackCarpet,                    // 171:15  minecraft:carpet

        IronSword,                 // 267:0  minecraft:iron_sword
        WoodenSword,               // 268:0  minecraft:wooden_sword

        StoneSword,                // 272:0  minecraft:stone_sword

        DiamondSword,              // 276:0  minecraft:diamond_sword
        DiamondShovel,             // 277:0  minecraft:diamond_shovel
        DiamondPickaxe,            // 278:0  minecraft:diamond_pickaxe
        DiamondAxe,                // 279:0  minecraft:diamond_axe

        Stick,                     // 280:0  minecraft:stick

        GoldenSword,               // 283:0  minecraft:golden_sword

        Feather,                   // 288:0  minecraft:feather

        Flint,                     // 318:0  minecraft:flint

        Sign,                      // 323:0  minecraft:sign

        /*  // This item is handled by clientside.
        Snowball,                  // 332:0  minecraft:snowball
        */

        Paper,                     // 339:0  minecraft:paper

        RedstoneRepeater,          // 356:0  minecraft:repeater

        GoldNugget,                // 371:0  minecraft:gold_nugget

        EyeOfEnder,                // 381:0  minecraft:ender_eye

        PlayerSkull,               // 397:0  minecraft:skull

        ArmorStand,                // 416:0  minecraft:armor_stand

        IronHorseArmor,            // 417:0  minecraft:iron_horse_armor
        GoldenHorseArmor,          // 418:0  minecraft:golden_horse_armor
        DiamondHorseArmor,         // 419:0  minecraft:diamond_horse_armor


        EndCrystal,                // 426:0  minecraft:end_crystal


        MusicDisc_C418_13,         // 2256:0  minecraft:record_13 
        MusicDisc_C418_cat,        // 2257:0  minecraft:record_cat 
        MusicDisc_C418_blocks,     // 2258:0  minecraft:record_blocks 
        MusicDisc_C418_chirp,      // 2259:0  minecraft:record_chirp 
        MusicDisc_C418_far,        // 2260:0  minecraft:record_far 
        MusicDisc_C418_mall,       // 2261:0  minecraft:record_mall 
        MusicDisc_C418_mellohi,    // 2262:0  minecraft:record_mellohi 
        MusicDisc_C418_stal,       // 2263:0  minecraft:record_stal 
        MusicDisc_C418_strad,      // 2264:0  minecraft:record_strad 
        MusicDisc_C418_ward,       // 2265:0  minecraft:record_ward 
        MusicDisc_C418_11,         // 2266:0  minecraft:record_11 
        MusicDisc_C418_wait,       // 2267:0  minecraft:record_wait 

    }
}