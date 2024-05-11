# Mappings

## Size

| Entity      | Width | Height | Code                                                                                                        |
|-------------|-------|--------|-------------------------------------------------------------------------------------------------------------|
| EntityItem  | 0.25  | 0.25   | [EntityItem](src/main/java/net/minecraft/server/EntityItem.java)::EntityItem(World, double, double, double) |
| EntityHuman | 0.6   | 1.8    | [EntityHuman](src/main/java/net/minecraft/server/EntityHuman.java)::a(boolean, boolean, boolean)            |

## Force

```
v' = v + f
p' = p + v'
```

### EntityItem
| Force f                                | Normal                                                        | Code                                                                   |
|----------------------------------------|---------------------------------------------------------------|------------------------------------------------------------------------|
| Gravity                                | f = (0, -0.03999999910593033, 0)                              | [EntityItem](src/main/java/net/minecraft/server/EntityItem.java)::B_() |
| Damping Force                          | f = (-(1 - 0.98), -(1 - 0.9800000190734863), -(1 - 0.98)) * v | [EntityItem](src/main/java/net/minecraft/server/EntityItem.java)::B_() |
| Friction <br/>(With Friction Factor c) | f = (-(1 - c), 0, -(1 - c)) * v                               | [EntityItem](src/main/java/net/minecraft/server/EntityItem.java)::B_() |

### EntityLiving
| Force f                                | Normal                                                        | Code                                                                                       |
|----------------------------------------|---------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| Gravity                                | f = (0, -0.08, 0)                                             | [EntityLiving](src/main/java/net/minecraft/server/EntityLiving.java)::a(float, float, float) |
| Damping Force                          | f = (-(1 - 0.91), -(1 - 0.9800000190734863), -(1 - 0.91)) * v | [EntityLiving](src/main/java/net/minecraft/server/EntityLiving.java)::a(float, float, float) |
| Friction <br/>(With Friction Factor c) | f = (-(1 - c), 0, -(1 - c)) * v                               | [EntityLiving](src/main/java/net/minecraft/server/EntityLiving.java)::a(float, float, float) |
