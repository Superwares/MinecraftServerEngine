# Mappings

## Object Gravity

### Entity Arrow
```java
    public void B_() {
            ...

            if (!this.isNoGravity()) {
                this.motY -= 0.05000000074505806D;
            }
            
            ...
    }
```

### Entity Living
```java
public void a(float f, float f1, float f2) {
    ...
                    } else if (!this.isNoGravity()) {
                        this.motY -= 0.08D;  // <- It's work in normal situation!
                    }
                }

                this.motY *= 0.9800000190734863D;
                this.motX *= (double) f9;
                this.motZ *= (double) f9;
                blockposition_pooledblockposition.t();
            }
        }

        ...
    }
```

## Object Collision Detection

## Object Collision Solution