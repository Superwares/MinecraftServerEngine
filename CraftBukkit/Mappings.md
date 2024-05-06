# Mappings

## Force

v' = v + f,
p' = p + v

### EntityItem
| Force                                                 | Normal                                                             |                                                                                                                     |
|-------------------------------------------------------|--------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------|
| Gravity                                               | (0, -0.03999999910593033, 0)                                       | [code](https://github.com/Superdek/Server/blob/main/CraftBukkit/src/main/java/net/minecraft/server/EntityItem.java?plain=1#L79) |
| Damping Force                                         | (-(1 - 0.98), -(1 - 0.9800000190734863), -(1 - 0.98)) * v          | [code](https://github.com/Superdek/Server/blob/main/CraftBukkit/src/main/java/net/minecraft/server/EntityItem.java?plain=1#L105) |
| Friction and Damping Force (With Friction Constant f) | (-(1 - f), -(1 - 0.9800000190734863) - (1 - (-0.5)), -(1 - f)) * v | [code](src/main/java/net/minecraft/server/EntityItem.java) |