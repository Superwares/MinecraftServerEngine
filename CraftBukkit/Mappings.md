# Mappings

## net.minecraft.server

```java
package net.minecraft.server;

import com.google.common.annotations.VisibleForTesting;
import javax.annotation.Nullable;

public class AxisAlignedBB {
    public final double a;
    public final double b;
    public final double c;
    public final double d;
    public final double e;
    public final double f;

    public AxisAlignedBB(double var1, double var3, double var5, double var7, double var9, double var11) {
        this.a = Math.min(var1, var7);
        this.b = Math.min(var3, var9);
        this.c = Math.min(var5, var11);
        this.d = Math.max(var1, var7);
        this.e = Math.max(var3, var9);
        this.f = Math.max(var5, var11);
    }

    public AxisAlignedBB(BlockPosition var1) {
        this((double)var1.getX(), (double)var1.getY(), (double)var1.getZ(), (double)(var1.getX() + 1), (double)(var1.getY() + 1), (double)(var1.getZ() + 1));
    }

    public AxisAlignedBB(BlockPosition var1, BlockPosition var2) {
        this((double)var1.getX(), (double)var1.getY(), (double)var1.getZ(), (double)var2.getX(), (double)var2.getY(), (double)var2.getZ());
    }

    public AxisAlignedBB e(double var1) {
        return new AxisAlignedBB(this.a, this.b, this.c, this.d, var1, this.f);
    }

    public boolean equals(Object var1) {
        if (this == var1) {
            return true;
        } else if (!(var1 instanceof AxisAlignedBB)) {
            return false;
        } else {
            AxisAlignedBB var2 = (AxisAlignedBB)var1;
            if (Double.compare(var2.a, this.a) != 0) {
                return false;
            } else if (Double.compare(var2.b, this.b) != 0) {
                return false;
            } else if (Double.compare(var2.c, this.c) != 0) {
                return false;
            } else if (Double.compare(var2.d, this.d) != 0) {
                return false;
            } else if (Double.compare(var2.e, this.e) != 0) {
                return false;
            } else {
                return Double.compare(var2.f, this.f) == 0;
            }
        }
    }

    public int hashCode() {
        long var1 = Double.doubleToLongBits(this.a);
        int var3 = (int)(var1 ^ var1 >>> 32);
        var1 = Double.doubleToLongBits(this.b);
        var3 = 31 * var3 + (int)(var1 ^ var1 >>> 32);
        var1 = Double.doubleToLongBits(this.c);
        var3 = 31 * var3 + (int)(var1 ^ var1 >>> 32);
        var1 = Double.doubleToLongBits(this.d);
        var3 = 31 * var3 + (int)(var1 ^ var1 >>> 32);
        var1 = Double.doubleToLongBits(this.e);
        var3 = 31 * var3 + (int)(var1 ^ var1 >>> 32);
        var1 = Double.doubleToLongBits(this.f);
        var3 = 31 * var3 + (int)(var1 ^ var1 >>> 32);
        return var3;
    }
    public AxisAlignedBB a(double var1, double var3, double var5) {
        double var7 = this.a;
        double var9 = this.b;
        double var11 = this.c;
        double var13 = this.d;
        double var15 = this.e;
        double var17 = this.f;
        if (var1 < 0.0) {
            var7 -= var1;
        } else if (var1 > 0.0) {
            var13 -= var1;
        }

        if (var3 < 0.0) {
            var9 -= var3;
        } else if (var3 > 0.0) {
            var15 -= var3;
        }

        if (var5 < 0.0) {
            var11 -= var5;
        } else if (var5 > 0.0) {
            var17 -= var5;
        }

        return new AxisAlignedBB(var7, var9, var11, var13, var15, var17);
    }

    public AxisAlignedBB b(double var1, double var3, double var5) {
        double var7 = this.a;
        double var9 = this.b;
        double var11 = this.c;
        double var13 = this.d;
        double var15 = this.e;
        double var17 = this.f;
        if (var1 < 0.0) {
            var7 += var1;
        } else if (var1 > 0.0) {
            var13 += var1;
        }

        if (var3 < 0.0) {
            var9 += var3;
        } else if (var3 > 0.0) {
            var15 += var3;
        }

        if (var5 < 0.0) {
            var11 += var5;
        } else if (var5 > 0.0) {
            var17 += var5;
        }

        return new AxisAlignedBB(var7, var9, var11, var13, var15, var17);
    }

    public AxisAlignedBB grow(double var1, double var3, double var5) {
        double var7 = this.a - var1;
        double var9 = this.b - var3;
        double var11 = this.c - var5;
        double var13 = this.d + var1;
        double var15 = this.e + var3;
        double var17 = this.f + var5;
        return new AxisAlignedBB(var7, var9, var11, var13, var15, var17);
    }

    public AxisAlignedBB g(double var1) {
        return this.grow(var1, var1, var1);
    }

    public AxisAlignedBB a(AxisAlignedBB var1) {
        double var2 = Math.max(this.a, var1.a);
        double var4 = Math.max(this.b, var1.b);
        double var6 = Math.max(this.c, var1.c);
        double var8 = Math.min(this.d, var1.d);
        double var10 = Math.min(this.e, var1.e);
        double var12 = Math.min(this.f, var1.f);
        return new AxisAlignedBB(var2, var4, var6, var8, var10, var12);
    }

    public AxisAlignedBB b(AxisAlignedBB var1) {
        double var2 = Math.min(this.a, var1.a);
        double var4 = Math.min(this.b, var1.b);
        double var6 = Math.min(this.c, var1.c);
        double var8 = Math.max(this.d, var1.d);
        double var10 = Math.max(this.e, var1.e);
        double var12 = Math.max(this.f, var1.f);
        return new AxisAlignedBB(var2, var4, var6, var8, var10, var12);
    }

    public AxisAlignedBB d(double var1, double var3, double var5) {
        return new AxisAlignedBB(this.a + var1, this.b + var3, this.c + var5, this.d + var1, this.e + var3, this.f + var5);
    }

    public AxisAlignedBB a(BlockPosition var1) {
        return new AxisAlignedBB(this.a + (double)var1.getX(), this.b + (double)var1.getY(), this.c + (double)var1.getZ(), this.d + (double)var1.getX(), this.e + (double)var1.getY(), this.f + (double)var1.getZ());
    }

    public AxisAlignedBB a(Vec3D var1) {
        return this.d(var1.x, var1.y, var1.z);
    }

    public double a(AxisAlignedBB var1, double var2) {
        if (!(var1.e <= this.b) && !(var1.b >= this.e) && !(var1.f <= this.c) && !(var1.c >= this.f)) {
            double var4;
            if (var2 > 0.0 && var1.d <= this.a) {
                var4 = this.a - var1.d;
                if (var4 < var2) {
                    var2 = var4;
                }
            } else if (var2 < 0.0 && var1.a >= this.d) {
                var4 = this.d - var1.a;
                if (var4 > var2) {
                    var2 = var4;
                }
            }

            return var2;
        } else {
            return var2;
        }
    }

    public double b(AxisAlignedBB var1, double var2) {
        if (!(var1.d <= this.a) && !(var1.a >= this.d) && !(var1.f <= this.c) && !(var1.c >= this.f)) {
            double var4;
            if (var2 > 0.0 && var1.e <= this.b) {
                var4 = this.b - var1.e;
                if (var4 < var2) {
                    var2 = var4;
                }
            } else if (var2 < 0.0 && var1.b >= this.e) {
                var4 = this.e - var1.b;
                if (var4 > var2) {
                    var2 = var4;
                }
            }

            return var2;
        } else {
            return var2;
        }
    }

    public double c(AxisAlignedBB var1, double var2) {
        if (!(var1.d <= this.a) && !(var1.a >= this.d) && !(var1.e <= this.b) && !(var1.b >= this.e)) {
            double var4;
            if (var2 > 0.0 && var1.f <= this.c) {
                var4 = this.c - var1.f;
                if (var4 < var2) {
                    var2 = var4;
                }
            } else if (var2 < 0.0 && var1.c >= this.f) {
                var4 = this.f - var1.c;
                if (var4 > var2) {
                    var2 = var4;
                }
            }

            return var2;
        } else {
            return var2;
        }
    }

    public boolean c(AxisAlignedBB var1) {
        return this.a(var1.a, var1.b, var1.c, var1.d, var1.e, var1.f);
    }

    public boolean a(double var1, double var3, double var5, double var7, double var9, double var11) {
        return this.a < var7 && this.d > var1 && this.b < var9 && this.e > var3 && this.c < var11 && this.f > var5;
    }

    public boolean b(Vec3D var1) {
        if (!(var1.x <= this.a) && !(var1.x >= this.d)) {
            if (!(var1.y <= this.b) && !(var1.y >= this.e)) {
                return !(var1.z <= this.c) && !(var1.z >= this.f);
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    public double a() {
        double var1 = this.d - this.a;
        double var3 = this.e - this.b;
        double var5 = this.f - this.c;
        return (var1 + var3 + var5) / 3.0;
    }

    public AxisAlignedBB shrink(double var1) {
        return this.g(-var1);
    }

    @Nullable
    public MovingObjectPosition b(Vec3D var1, Vec3D var2) {
        Vec3D var3 = this.a(this.a, var1, var2);
        EnumDirection var4 = EnumDirection.WEST;
        Vec3D var5 = this.a(this.d, var1, var2);
        if (var5 != null && this.a(var1, var3, var5)) {
            var3 = var5;
            var4 = EnumDirection.EAST;
        }

        var5 = this.b(this.b, var1, var2);
        if (var5 != null && this.a(var1, var3, var5)) {
            var3 = var5;
            var4 = EnumDirection.DOWN;
        }

        var5 = this.b(this.e, var1, var2);
        if (var5 != null && this.a(var1, var3, var5)) {
            var3 = var5;
            var4 = EnumDirection.UP;
        }

        var5 = this.c(this.c, var1, var2);
        if (var5 != null && this.a(var1, var3, var5)) {
            var3 = var5;
            var4 = EnumDirection.NORTH;
        }

        var5 = this.c(this.f, var1, var2);
        if (var5 != null && this.a(var1, var3, var5)) {
            var3 = var5;
            var4 = EnumDirection.SOUTH;
        }

        return var3 == null ? null : new MovingObjectPosition(var3, var4);
    }

    @VisibleForTesting
    boolean a(Vec3D var1, @Nullable Vec3D var2, Vec3D var3) {
        return var2 == null || var1.distanceSquared(var3) < var1.distanceSquared(var2);
    }

    @Nullable
    @VisibleForTesting
    Vec3D a(double var1, Vec3D var3, Vec3D var4) {
        Vec3D var5 = var3.a(var4, var1);
        return var5 != null && this.c(var5) ? var5 : null;
    }

    @Nullable
    @VisibleForTesting
    Vec3D b(double var1, Vec3D var3, Vec3D var4) {
        Vec3D var5 = var3.b(var4, var1);
        return var5 != null && this.d(var5) ? var5 : null;
    }

    @Nullable
    @VisibleForTesting
    Vec3D c(double var1, Vec3D var3, Vec3D var4) {
        Vec3D var5 = var3.c(var4, var1);
        return var5 != null && this.e(var5) ? var5 : null;
    }

    @VisibleForTesting
    public boolean c(Vec3D var1) {
        return var1.y >= this.b && var1.y <= this.e && var1.z >= this.c && var1.z <= this.f;
    }

    @VisibleForTesting
    public boolean d(Vec3D var1) {
        return var1.x >= this.a && var1.x <= this.d && var1.z >= this.c && var1.z <= this.f;
    }

    @VisibleForTesting
    public boolean e(Vec3D var1) {
        return var1.x >= this.a && var1.x <= this.d && var1.y >= this.b && var1.y <= this.e;
    }

    public String toString() {
        return "box[" + this.a + ", " + this.b + ", " + this.c + " -> " + this.d + ", " + this.e + ", " + this.f + "]";
    }
}

```

```java
package net.minecraft.server;

import java.util.Random;
import java.util.UUID;

public class MathHelper {
    public static final float a = c(2.0F);
    private static final float[] b = new float[65536];
    private static final Random c = new Random();
    private static final int[] d;
    private static final double e;
    private static final double[] f;
    private static final double[] g;

    public static float sin(float var0) {
        return b[(int)(var0 * 10430.378F) & '\uffff'];
    }

    public static float cos(float var0) {
        return b[(int)(var0 * 10430.378F + 16384.0F) & '\uffff'];
    }

    public static float c(float var0) {
        return (float)Math.sqrt((double)var0);
    }

    public static float sqrt(double var0) {
        return (float)Math.sqrt(var0);
    }

    public static int d(float var0) {
        int var1 = (int)var0;
        return var0 < (float)var1 ? var1 - 1 : var1;
    }

    public static int floor(double var0) {
        int var2 = (int)var0;
        return var0 < (double)var2 ? var2 - 1 : var2;
    }

    public static long d(double var0) {
        long var2 = (long)var0;
        return var0 < (double)var2 ? var2 - 1L : var2;
    }

    public static float e(float var0) {
        return var0 >= 0.0F ? var0 : -var0;
    }

    public static int a(int var0) {
        return var0 >= 0 ? var0 : -var0;
    }

    public static int f(float var0) {
        int var1 = (int)var0;
        return var0 > (float)var1 ? var1 + 1 : var1;
    }

    public static int f(double var0) {
        int var2 = (int)var0;
        return var0 > (double)var2 ? var2 + 1 : var2;
    }

    public static int clamp(int var0, int var1, int var2) {
        if (var0 < var1) {
            return var1;
        } else {
            return var0 > var2 ? var2 : var0;
        }
    }

    public static float a(float var0, float var1, float var2) {
        if (var0 < var1) {
            return var1;
        } else {
            return var0 > var2 ? var2 : var0;
        }
    }

    public static double a(double var0, double var2, double var4) {
        if (var0 < var2) {
            return var2;
        } else {
            return var0 > var4 ? var4 : var0;
        }
    }

    public static double b(double var0, double var2, double var4) {
        if (var4 < 0.0) {
            return var0;
        } else {
            return var4 > 1.0 ? var2 : var0 + (var2 - var0) * var4;
        }
    }

    public static double a(double var0, double var2) {
        if (var0 < 0.0) {
            var0 = -var0;
        }

        if (var2 < 0.0) {
            var2 = -var2;
        }

        return var0 > var2 ? var0 : var2;
    }

    public static int nextInt(Random var0, int var1, int var2) {
        return var1 >= var2 ? var1 : var0.nextInt(var2 - var1 + 1) + var1;
    }

    public static float a(Random var0, float var1, float var2) {
        return var1 >= var2 ? var1 : var0.nextFloat() * (var2 - var1) + var1;
    }

    public static double a(Random var0, double var1, double var3) {
        return var1 >= var3 ? var1 : var0.nextDouble() * (var3 - var1) + var1;
    }

    public static double a(long[] var0) {
        long var1 = 0L;
        long[] var3 = var0;
        int var4 = var0.length;

        for(int var5 = 0; var5 < var4; ++var5) {
            long var6 = var3[var5];
            var1 += var6;
        }

        return (double)var1 / (double)var0.length;
    }

    public static float g(float var0) {
        var0 %= 360.0F;
        if (var0 >= 180.0F) {
            var0 -= 360.0F;
        }

        if (var0 < -180.0F) {
            var0 += 360.0F;
        }

        return var0;
    }

    public static double g(double var0) {
        var0 %= 360.0;
        if (var0 >= 180.0) {
            var0 -= 360.0;
        }

        if (var0 < -180.0) {
            var0 += 360.0;
        }

        return var0;
    }

    public static int b(int var0) {
        var0 %= 360;
        if (var0 >= 180) {
            var0 -= 360;
        }

        if (var0 < -180) {
            var0 += 360;
        }

        return var0;
    }

    public static int a(String var0, int var1) {
        try {
            return Integer.parseInt(var0);
        } catch (Throwable var3) {
            return var1;
        }
    }

    public static int a(String var0, int var1, int var2) {
        return Math.max(var2, a(var0, var1));
    }

    public static double a(String var0, double var1) {
        try {
            return Double.parseDouble(var0);
        } catch (Throwable var4) {
            return var1;
        }
    }

    public static double a(String var0, double var1, double var3) {
        return Math.max(var3, a(var0, var1));
    }

    public static int c(int var0) {
        int var1 = var0 - 1;
        var1 |= var1 >> 1;
        var1 |= var1 >> 2;
        var1 |= var1 >> 4;
        var1 |= var1 >> 8;
        var1 |= var1 >> 16;
        return var1 + 1;
    }

    private static boolean g(int var0) {
        return var0 != 0 && (var0 & var0 - 1) == 0;
    }

    public static int d(int var0) {
        var0 = g(var0) ? var0 : c(var0);
        return d[(int)((long)var0 * 125613361L >> 27) & 31];
    }

    public static int e(int var0) {
        return d(var0) - (g(var0) ? 0 : 1);
    }

    public static int c(int var0, int var1) {
        if (var1 == 0) {
            return 0;
        } else if (var0 == 0) {
            return var1;
        } else {
            if (var0 < 0) {
                var1 *= -1;
            }

            int var2 = var0 % var1;
            return var2 == 0 ? var0 : var0 + var1 - var2;
        }
    }

    public static long c(int var0, int var1, int var2) {
        long var3 = (long)(var0 * 3129871) ^ (long)var2 * 116129781L ^ (long)var1;
        var3 = var3 * var3 * 42317861L + var3 * 11L;
        return var3;
    }

    public static UUID a(Random var0) {
        long var1 = var0.nextLong() & -61441L | 16384L;
        long var3 = var0.nextLong() & 4611686018427387903L | Long.MIN_VALUE;
        return new UUID(var1, var3);
    }

    public static UUID a() {
        return a(c);
    }

    public static double c(double var0, double var2, double var4) {
        return (var0 - var2) / (var4 - var2);
    }

    // This is used for transform from delta X and delta Y to pitch and yaw, in EntityArrow.
    // It is only used with other forms, not direct use.
    // var0 : x component of vector
    // var2 : z component of vector
    public static double c(double var0, double var2) {
        double var4 = var2 * var2 + var0 * var0;  // Get squared length of plane
        if (Double.isNaN(var4)) {
            return Double.NaN;
        } else {
            // Change the value of var0 into positive, if it is negative.
            boolean var6 = var0 < 0.0;
            if (var6) {
                var0 = -var0;
            }
            
            // Change the value of var2 into positive, if it is negative.
            boolean var7 = var2 < 0.0;
            if (var7) {
                var2 = -var2;
            }

            // If not var2 > var0, they swap.
            boolean var8 = var0 > var2;
            double var9;
            if (var8) {
                var9 = var2;
                var2 = var0;
                var0 = var9;
            }

            var9 = i(var4);  // Fast inverse square root.
            var2 *= var9;
            var0 *= var9;
            double var11 = e + var0;
            int var13 = (int)Double.doubleToRawLongBits(var11);
            double var14 = f[var13];
            double var16 = g[var13];
            double var18 = var11 - e;
            double var20 = var0 * var16 - var2 * var18;
            double var22 = (6.0 + var20 * var20) * var20 * 0.16666666666666666;
            double var24 = var14 + var22;
            if (var8) {
                var24 = 1.5707963267948966 - var24;
            }

            if (var7) {
                var24 = Math.PI - var24;
            }

            if (var6) {
                var24 = -var24;
            }

            return var24;
        }
    }

    // Fast inverse square root.
    public static double i(double var0) {
        double var2 = 0.5 * var0;
        long var4 = Double.doubleToRawLongBits(var0);
        var4 = 6910469410427058090L - (var4 >> 1);
        var0 = Double.longBitsToDouble(var4);
        var0 *= 1.5 - var2 * var0 * var0;
        return var0;
    }

    public static int f(int var0) {
        var0 ^= var0 >>> 16;
        var0 *= -2048144789;
        var0 ^= var0 >>> 13;
        var0 *= -1028477387;
        var0 ^= var0 >>> 16;
        return var0;
    }

    static {
        int var0;
        for(var0 = 0; var0 < 65536; ++var0) {
            b[var0] = (float)Math.sin((double)var0 * Math.PI * 2.0 / 65536.0);
        }

        d = new int[]{0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9};
        e = Double.longBitsToDouble(4805340802404319232L);
        f = new double[257];
        g = new double[257];

        for(var0 = 0; var0 < 257; ++var0) {
            double var1 = (double)var0 / 256.0;
            double var3 = Math.asin(var1);
            g[var0] = Math.cos(var3);
            f[var0] = var3;
        }

    }
}

```