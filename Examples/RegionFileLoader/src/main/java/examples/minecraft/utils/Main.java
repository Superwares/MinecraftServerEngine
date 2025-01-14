package examples.minecraft.utils;

import java.io.DataInputStream;
import java.io.File;

public class Main {
    public static void main(String[] args) {

        System.out.println("Hello world!");

        int i = 0;
        int j = 0;

        File file1 = new File("region");
        File file2 = new File(file1, "r." + (i >> 5) + "." + (j >> 5) + ".mca");

//        System.out.println(file2.exists());

        RegionFile regionfile1 = new RegionFile(file2);

        DataInputStream stream = regionfile1.a(1, 30);


    }
}