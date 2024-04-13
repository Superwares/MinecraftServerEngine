package net.minecraft.server;

import com.google.common.annotations.VisibleForTesting;
import javax.annotation.Nullable;

public class AxisAlignedBB {
    public final double a;  // xMin
    public final double b;  // yMin
    public final double c;  // zMin
    public final double d;  // xMax
    public final double e;  // yMax
    public final double f;  // zMax

    public AxisAlignedBB(double var1, double var3, double var5, double var7, double var9, double var11) {
        this.a = Math.min(var1, var7);
        this.b = Math.min(var3, var9);
        this.c = Math.min(var5, var11);
        this.d = Math.max(var1, var7);
        this.e = Math.max(var3, var9);
        this.f = Math.max(var5, var11);
    }

    // Construct AxisAlignedBB of block
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

    /*
        var1, var3, var5 : movement vector
        Returns the total bounding box that the movement vector and the current bounding box combined.
     */
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

    // Transform object space to world space.
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

    // Is the vector var1 in the current bounding box.
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
