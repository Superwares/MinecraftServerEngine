# Mappings

## Size

| Entity     | Width | Height | Link                                                                         |
|------------|-------|--------|------------------------------------------------------------------------------|
| EntityItem | 0.25  | 0.25   | [EntityItem](src/main/java/net/minecraft/server/EntityItem.java)::EntityItem |

## Force

```
v' = v + f
p' = p + v'
```


### EntityItem
| Force f                                                    | Normal                                                                 | Link                                                                 |
|------------------------------------------------------------|------------------------------------------------------------------------|----------------------------------------------------------------------|
| Gravity                                                    | f = (0, -0.03999999910593033, 0)                                       | [EntityItem](src/main/java/net/minecraft/server/EntityItem.java)::B_ |
| Damping Force                                              | f = (-(1 - 0.98), -(1 - 0.9800000190734863), -(1 - 0.98)) * v          | [EntityItem](src/main/java/net/minecraft/server/EntityItem.java)::B_ |
| Friction and Damping Force <br/>(With Friction Constant c) | f = (-(1 - c), -(1 - 0.9800000190734863) - (1 - (-0.5)), -(1 - c)) * v | [EntityItem](src/main/java/net/minecraft/server/EntityItem.java)::B_ |

### EntityLiving
