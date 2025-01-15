package examples.minecraft.utils;

import java.io.DataInputStream;
import java.io.File;

public class Main {
    public static void main(String[] args) {

        System.out.println("Hello world!");

        int i = 0;
        int j = 0;

        int chunkX = 1;
        int chunkY = 1;

//        File file1 = new File("region");
//        File file2 = new File(file1, "r." + (i >> 5) + "." + (j >> 5) + ".mca");

        File file2 = new File("C:\\Users\\Peach\\Documents\\Superwares\\MinecraftServerEngine\\Testing\\r.0.0 - Copy.mca");

//        System.out.println(file2.exists());

        RegionFile regionfile1 = new RegionFile(file2);

        DataInputStream stream = regionfile1.a(chunkX & 31, chunkY & 31);

    }
}