package net.minecraft.server;

import org.bukkit.craftbukkit.event.CraftEventFactory; // CraftBukkit

public abstract class EntityFireball extends Entity {

    public EntityLiving shooter;
    private int e;
    private int f;

    // power vector
    public double dirX;
    public double dirY;
    public double dirZ;

    public float bukkitYield = 1; // CraftBukkit
    public boolean isIncendiary = true; // CraftBukkit

    public EntityFireball(World world) {
        super(world);
        this.setSize(1.0F, 1.0F);
    }

    protected void i() {}

    // d0, d1, d2: entity position
    // d3, d4, d5: power?
    public EntityFireball(World world, double d0, double d1, double d2, double d3, double d4, double d5) {
        super(world);
        this.setSize(1.0F, 1.0F);
        this.setPositionRotation(d0, d1, d2, this.yaw, this.pitch);
        this.setPosition(d0, d1, d2);
        double d6 = (double) MathHelper.sqrt(d3 * d3 + d4 * d4 + d5 * d5);  // get length of power vector

        this.dirX = d3 / d6 * 0.1D;
        this.dirY = d4 / d6 * 0.1D;
        this.dirZ = d5 / d6 * 0.1D;
    }

    public EntityFireball(World world, EntityLiving entityliving, double d0, double d1, double d2) {
        super(world);
        this.shooter = entityliving;
        this.projectileSource = (org.bukkit.entity.LivingEntity) entityliving.getBukkitEntity(); // CraftBukkit
        this.setSize(1.0F, 1.0F);
        this.setPositionRotation(entityliving.locX, entityliving.locY, entityliving.locZ, entityliving.yaw, entityliving.pitch);
        this.setPosition(this.locX, this.locY, this.locZ);
        this.motX = 0.0D;
        this.motY = 0.0D;
        this.motZ = 0.0D;
        // CraftBukkit start - Added setDirection method
        this.setDirection(d0, d1, d2);
    }

    public void setDirection(double d0, double d1, double d2) {
        // CraftBukkit end
        d0 += this.random.nextGaussian() * 0.4D;
        d1 += this.random.nextGaussian() * 0.4D;
        d2 += this.random.nextGaussian() * 0.4D;
        double d3 = (double) MathHelper.sqrt(d0 * d0 + d1 * d1 + d2 * d2);

        this.dirX = d0 / d3 * 0.1D;
        this.dirY = d1 / d3 * 0.1D;
        this.dirZ = d2 / d3 * 0.1D;
    }

    public void B_() {
        if (!this.world.isClientSide && (this.shooter != null && this.shooter.dead || !this.world.isLoaded(new BlockPosition(this)))) {
            this.die();
        } else {
            super.B_();
            if (this.k()) {
                this.setOnFire(1);
            }

            ++this.f;
            MovingObjectPosition movingobjectposition = ProjectileHelper.a(this, true, this.f >= 25, this.shooter);

            if (movingobjectposition != null) {
                this.a(movingobjectposition);

                // CraftBukkit start - Fire ProjectileHitEvent
                if (this.dead) {
                    CraftEventFactory.callProjectileHitEvent(this, movingobjectposition);
                }
                // CraftBukkit end
            }

            this.locX += this.motX;
            this.locY += this.motY;
            this.locZ += this.motZ;
            ProjectileHelper.a(this, 0.2F);
            float f = this.l();  // return 0.95F;

            if (this.isInWater()) {
                for (int i = 0; i < 4; ++i) {
                    float f1 = 0.25F;

                    this.world.addParticle(EnumParticle.WATER_BUBBLE, this.locX - this.motX * 0.25D, this.locY - this.motY * 0.25D, this.locZ - this.motZ * 0.25D, this.motX, this.motY, this.motZ, new int[0]);
                }

                f = 0.8F;
            }

            this.motX += this.dirX;
            this.motY += this.dirY;
            this.motZ += this.dirZ;
            this.motX *= (double) f;
            this.motY *= (double) f;
            this.motZ *= (double) f;
            this.world.addParticle(this.j(), this.locX, this.locY + 0.5D, this.locZ, 0.0D, 0.0D, 0.0D, new int[0]);
            this.setPosition(this.locX, this.locY, this.locZ);
        }
    }

    protected boolean k() {
        return true;
    }

    protected EnumParticle j() {
        return EnumParticle.SMOKE_NORMAL;
    }

    protected float l() {
        return 0.95F;
    }

    protected abstract void a(MovingObjectPosition movingobjectposition);

    public static void a(DataConverterManager dataconvertermanager, String s) {}

    public void b(NBTTagCompound nbttagcompound) {
        nbttagcompound.set("direction", this.a(new double[] { this.motX, this.motY, this.motZ}));
        nbttagcompound.set("power", this.a(new double[] { this.dirX, this.dirY, this.dirZ}));
        nbttagcompound.setInt("life", this.e);
    }

    public void a(NBTTagCompound nbttagcompound) {
        NBTTagList nbttaglist;

        if (nbttagcompound.hasKeyOfType("power", 9)) {
            nbttaglist = nbttagcompound.getList("power", 6);
            if (nbttaglist.size() == 3) {
                this.dirX = nbttaglist.f(0);
                this.dirY = nbttaglist.f(1);
                this.dirZ = nbttaglist.f(2);
            }
        }

        this.e = nbttagcompound.getInt("life");
        if (nbttagcompound.hasKeyOfType("direction", 9) && nbttagcompound.getList("direction", 6).size() == 3) {
            nbttaglist = nbttagcompound.getList("direction", 6);
            this.motX = nbttaglist.f(0);
            this.motY = nbttaglist.f(1);
            this.motZ = nbttaglist.f(2);
        } else {
            this.die();
        }

    }

    public boolean isInteractable() {
        return true;
    }

    public float aI() {
        return 1.0F;
    }

    public boolean damageEntity(DamageSource damagesource, float f) {
        if (this.isInvulnerable(damagesource)) {
            return false;
        } else {
            this.ax();  // this.velocityChanged = true;
            if (damagesource.getEntity() != null) {
                // CraftBukkit start
                if (CraftEventFactory.handleNonLivingEntityDamageEvent(this, damagesource, f)) {
                    return false;
                }
                // CraftBukkit end
                // Get opposite direction of entity's pitch and yaw.
                // What is the magnitude of that vector?
                Vec3D vec3d = damagesource.getEntity().aJ();

                if (vec3d != null) {
                    this.motX = vec3d.x;
                    this.motY = vec3d.y;
                    this.motZ = vec3d.z;
                    // Scaling the vector down to 1/10
                    this.dirX = this.motX * 0.1D;
                    this.dirY = this.motY * 0.1D;
                    this.dirZ = this.motZ * 0.1D;
                }

                if (damagesource.getEntity() instanceof EntityLiving) {
                    this.shooter = (EntityLiving) damagesource.getEntity();
                    this.projectileSource = (org.bukkit.projectiles.ProjectileSource) this.shooter.getBukkitEntity();
                }

                return true;
            } else {
                return false;
            }
        }
    }

    public float aw() {
        return 1.0F;
    }
}
